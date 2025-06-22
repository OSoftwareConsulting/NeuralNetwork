/*
 * Copyright ©
 * 2025
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using NeuralNetworkLib;
using NeuralNetworkLib.ActivationFunctions;
using Newtonsoft.Json;
using SamplesGeneratorLib;
using System.Reflection;

namespace SetupLib;

public static class SetupReader
{
    // Used to create training and testing samples from a file
    public class FileSamplesGenerator
    {
        // The samples file path specified relative to the test setup JSON file
        public string FilePath { get; set; }

        // The character that separates sample values (',' for CSV, etc)
        public char Separator { get; set; }

        // The number of first rows to skip in the samples file (e.g., for headers)
        public int SkipRows { get; set; }

        // The number of first columns to skip in each record (e.g., for dates)
        public int SkipColumns { get; set; }

        // If true, the samples read in from the file are randomized in the training and testing sets, otherwise they are taken in the order in the file
        public bool RandomizeSamples { get; set; }

        // Perform the input normalization
        public bool? NormalizeInputs { get; set; }
    }

    public struct ValueRange
    {
        // Minimum value for this range
        public double MinValue { get; set; }

        // Maximum value for this range
        public double MaxValue { get; set; }
    }

    // Used to create training and testing samples by calling a function with random inputs between min & max values
    public class FunctionSamplesGenerator
    {
        // The name of the class that implements the ISamplesGeneratorFunction interface used to generate the outputs from the randomly generated inputs
        public string SamplesGeneratorFunction { get; set; }

        // The number of records to generate
        public int NbrRecords { get; set; }

        // An array of min and max input values used to generate inputs to the Samples Generator Function
        public ValueRange[] ValueRanges { get; set; }

        // Perform the input normalization
        public bool? NormalizeInputs { get; set; }
    }

    // Specifies for the configuration of a Neuron Layer
    public class NeuronLayerConfig
    {
        // The number of outputs from this layer
        public int NbrOutputs { get; set; }

        // The activation function to use for this layer (required)
        public string ActivationFunction { get; set; }

        // The +/- range for the initial weight and bias values set randomly (-range .. +range)
        public double InitialWeightRange { get; set; }
    }

    // The root JSON object
    public class NeuralNetworkSetup
    {
        // General

        // Flag to use to enable/disable debug messages, etc.
        public bool Debug { get; set; }

        // If specified, AssemblyPaths is an array of file paths to assembly DLL's
        public string[] AssemblyPaths { get; set; }

        // Data Set

        // If specified, Seed is the int parameter to the Random class constructor, otherwise the random number generator is created without a parameter
        public int? Seed { get; set; }

        // The number of inputs to the neural network
        public int NbrInputs { get; set; }

        // Used to create training and testing samples from a file
        public FileSamplesGenerator FileSamplesGenerator { get; set; }

        // Used to create training and testing samples by calling a function with random inputs between min & max values
        public FunctionSamplesGenerator FunctionSamplesGenerator { get; set; }

        // A number between 0.0 and 1.0 to specify the fraction of the samples to use for training, and testing by implication
        public double TrainingFraction { get; set; }

        // Neural Network

        // Neuron Layer Configurations (must specify at least one)
        public NeuronLayerConfig[] LayerConfigs { get; set; }

        // Training

        // The number of training iterations
        public int NbrEpochs { get; set; }

        // The fraction of the gradient amount used to adjust the layers' neurons' weights and biases
        public double TrainingRate { get; set; }

        // The fraction of the delta amount used to adjust the layers' neurons' weights and biases
        public double TrainingMomentum { get; set; }

        // Testing

        // Specifies the class implementing the IUserDefinedFunctions interface
        public string UserDefinedFunctions { get; set; }
    }

    public static SetupLib.NeuralNetworkSetup GetNeuralNetworkSetup(string filePath)
    {
        var nnSetup = JsonConvert.DeserializeObject<NeuralNetworkSetup>(File.ReadAllText(filePath));

        if (nnSetup == null)
        {
            throw new ArgumentNullException(nameof(nnSetup));
        }

        if (nnSetup.AssemblyPaths != null)
        {
            // load Assemblies from specified file paths
            foreach (var assemblyPath in nnSetup.AssemblyPaths)
            {
                Assembly.LoadFrom(assemblyPath);
            }
        }

        Random rnd = nnSetup.Seed.HasValue ? new Random(nnSetup.Seed.Value) : new Random();

        var neuronLayerConfigs = new List<NeuralNetworkLib.NeuronLayerConfig>();
        foreach (var testSetupLayerConfig in nnSetup.LayerConfigs)
        {
            var afInstance = GetInstance(testSetupLayerConfig.ActivationFunction);

            if (afInstance == null)
            {
                throw new NullReferenceException("Activation Function Instance was not loaded");
            }

            IActivationFunction activationFunction = (IActivationFunction)afInstance;

            neuronLayerConfigs.Add(new NeuralNetworkLib.NeuronLayerConfig(
                testSetupLayerConfig.NbrOutputs,
                activationFunction,
                testSetupLayerConfig.InitialWeightRange));
        }

        int nbrLayers = neuronLayerConfigs.Count();

        if (nbrLayers == 0)
        {
            throw new InvalidOperationException("Must specify at least one Neuron Layer Configuration");
        }

        int nbrOutputs = neuronLayerConfigs[nbrLayers - 1].NbrOutputs;

        Samples samples;

        if (nnSetup.FileSamplesGenerator != null)
        {
            string dataFilePath = Path.IsPathRooted(nnSetup.FileSamplesGenerator.FilePath) ?
                nnSetup.FileSamplesGenerator.FilePath :
                Path.Combine(
                    Path.GetDirectoryName(filePath) ?? throw new NullReferenceException(), 
                    nnSetup.FileSamplesGenerator.FilePath);

            samples = SamplesGeneratorLib.FileSamplesGenerator.GetSamples(
                nbrOutputs,
                nnSetup.TrainingFraction,
                nnSetup.FileSamplesGenerator.NormalizeInputs ?? false,
                dataFilePath,
                nnSetup.FileSamplesGenerator.Separator,
                nnSetup.FileSamplesGenerator.SkipRows,
                nnSetup.FileSamplesGenerator.SkipColumns,
                nnSetup.FileSamplesGenerator.RandomizeSamples ? rnd : null);
        }
        else if (nnSetup.FunctionSamplesGenerator?.SamplesGeneratorFunction != null)
        {
            var sgfInstance = GetInstance(nnSetup.FunctionSamplesGenerator.SamplesGeneratorFunction);

            if (sgfInstance == null)
            {
                throw new NullReferenceException("Samples Generator Function Instance was not loaded");
            }

            ISamplesGeneratorFunction dataGeneratorFunction = (ISamplesGeneratorFunction)sgfInstance;

            var valueRanges = new List<SamplesGeneratorLib.FunctionSamplesGenerator.ValueRange>();
            foreach (var valueRange in nnSetup.FunctionSamplesGenerator.ValueRanges)
            {
                // MinValue & MaxValue: specifies the range for the input set randomly
                valueRanges.Add(new SamplesGeneratorLib.FunctionSamplesGenerator.ValueRange(
                    valueRange.MinValue,
                    valueRange.MaxValue));
            }

            samples = SamplesGeneratorLib.FunctionSamplesGenerator.GetSamples(
                nbrOutputs,
                nnSetup.TrainingFraction,
                nnSetup.FunctionSamplesGenerator.NormalizeInputs ?? false,
                dataGeneratorFunction,
                nnSetup.FunctionSamplesGenerator.NbrRecords,
                valueRanges.ToArray(),
                rnd);
        }
        // Must specify a Samples Generator
        else
        {
            throw new InvalidOperationException("Must specify a Samples Generator");
        }

        var udfInstance = GetInstance(nnSetup.UserDefinedFunctions);

        if (udfInstance == null)
        {
            throw new NullReferenceException("User-Defiined Function Instance was not loaded");
        }

        IUserDefinedFunctions userDefinedFunctions = (IUserDefinedFunctions)udfInstance;

        return new SetupLib.NeuralNetworkSetup(
            rnd,
            nnSetup.Debug,
            nnSetup.NbrInputs,
            samples,
            neuronLayerConfigs.ToArray(),
            nnSetup.NbrEpochs,
            nnSetup.TrainingRate,
            nnSetup.TrainingMomentum,
            userDefinedFunctions);
    }

    // Specifies for the configuration of a Neuron Layer
    public class GANeuronLayerConfig
    {
        // The number of outputs from this layer
        public int[] NbrOutputs { get; set; }

        // The activation function to use for this layer (required)
        public string[] ActivationFunction { get; set; }

        // The +/- range for the initial weight and bias values set randomly (-range .. +range)
        public double[] InitialWeightRange { get; set; }
    }

    // The root JSON object
    public class GeneticAlgorithmSetup
    {
        // General

        // Flag to use to enable/disable debug messages, etc.
        public bool Debug { get; set; }

        // If specified, AssemblyPaths is an array of file paths to assembly DLL's
        public string[] AssemblyPaths { get; set; }

        // Data Set

        // If specified, Seed is the int parameter to the Random class constructor, otherwise the random number generator is created without a parameter
        public int? Seed { get; set; }

        // The number of inputs to the neural network
        public int NbrInputs { get; set; }

        // The number of outputs from the Last Layer (Neural Network)
        public int NbrOutputs { get; set; }

        // Used to create training and testing samples from a file
        public FileSamplesGenerator FileSamplesGenerator { get; set; }

        // Used to create training and testing samples by calling a function with random inputs between min & max values
        public FunctionSamplesGenerator FunctionSamplesGenerator { get; set; }

        // A number between 0.0 and 1.0 to specify the fraction of the samples to use for training, and testing by implication
        public double TrainingFraction { get; set; }

        // Neural Network

        // The number of neural network layers
        public int[] NbrLayers { get; set; }

        // Neuron Layer Configuration
        public GANeuronLayerConfig LayerConfig { get; set; }

        // The Activation Function to use for the Output Layer
        public string OutputLayerActivationFunction { get; set; }

        // Training

        // The number of training iterations
        public int NbrEpochs { get; set; }

        // The fraction of the gradient amount used to adjust the layers' neurons' weights and biases
        public double[] TrainingRate { get; set; }

        // The fraction of the delta amount used to adjust the layers' neurons' weights and biases
        public double[] TrainingMomentum { get; set; }

        // Testing

        // Specifies the class implementing the IUserDefinedFunctions interface
        public string UserDefinedFunctions { get; set; }

        // Genetic Algorithm

        // The number of GA Individuals
        public int PopulationSize { get; set; }

        // The fraction of the population selected to be included in the next generation
        public double SelectionPercentage { get; set; }

        // The fraction of the population selected to be mated in the next generation
        public double MatingPercentage { get; set; }

        // The probability fraction (0.0 to 1.0) that a gene will be mutated during mating
        public double MutationProbability { get; set; }

        // True when a lower fitness value is more optimal (e.g., errors), and 
        // False when a hihger fitness value is more optimal (e.g., test pass percentages)
        public bool FitnessLowerBetter { get; set; }
    }

    public static SetupLib.GeneticAlgorithmSetup GetGeneticAlgorithmSetup(string filePath)
    {
        var gaSetup = JsonConvert.DeserializeObject<GeneticAlgorithmSetup>(File.ReadAllText(filePath));

        if (gaSetup == null)
        {
            throw new ArgumentNullException(nameof(gaSetup));
        }

        if (gaSetup.AssemblyPaths != null)
        {
            // load Assemblies from specified file paths
            foreach (var assemblyPath in gaSetup.AssemblyPaths)
            {
                Assembly.LoadFrom(assemblyPath);
            }
        }

        Random rnd = gaSetup.Seed.HasValue ? new Random(gaSetup.Seed.Value) : new Random();

        var activationFunctions = new List<IActivationFunction>();
        foreach (var activationFunctionAssemblyPath in gaSetup.LayerConfig.ActivationFunction)
        {
            var afInstance = GetInstance(activationFunctionAssemblyPath);

            if (afInstance == null)
            {
                throw new NullReferenceException($"Activation Function Instance {activationFunctionAssemblyPath} was not loaded");
            }

            IActivationFunction activationFunction = (IActivationFunction)afInstance;

            activationFunctions.Add(activationFunction);
        }

        var gaNeuronLayerConfig = new SetupLib.GANeuronLayerConfig(
            gaSetup.LayerConfig.NbrOutputs,
            activationFunctions.ToArray(),
            gaSetup.LayerConfig.InitialWeightRange);

        Samples samples;

        if (gaSetup.FileSamplesGenerator != null)
        {
            string dataFilePath = Path.IsPathRooted(gaSetup.FileSamplesGenerator.FilePath) ?
                gaSetup.FileSamplesGenerator.FilePath :
                Path.Combine(
                    Path.GetDirectoryName(filePath) ?? throw new NullReferenceException(),
                    gaSetup.FileSamplesGenerator.FilePath);

            samples = SamplesGeneratorLib.FileSamplesGenerator.GetSamples(
                gaSetup.NbrOutputs,
                gaSetup.TrainingFraction,
                gaSetup.FileSamplesGenerator.NormalizeInputs ?? false,
                dataFilePath,
                gaSetup.FileSamplesGenerator.Separator,
                gaSetup.FileSamplesGenerator.SkipRows,
                gaSetup.FileSamplesGenerator.SkipColumns,
                gaSetup.FileSamplesGenerator.RandomizeSamples ? rnd : null);
        }
        else if (gaSetup.FunctionSamplesGenerator?.SamplesGeneratorFunction != null)
        {
            var sgfInstance = GetInstance(gaSetup.FunctionSamplesGenerator.SamplesGeneratorFunction);

            if (sgfInstance == null)
            {
                throw new NullReferenceException("Samples Generator Function Instance was not loaded");
            }

            ISamplesGeneratorFunction dataGeneratorFunction = (ISamplesGeneratorFunction)sgfInstance;

            var valueRanges = new List<SamplesGeneratorLib.FunctionSamplesGenerator.ValueRange>();
            foreach (var valueRange in gaSetup.FunctionSamplesGenerator.ValueRanges)
            {
                // MinValue & MaxValue: specifies the range for the input set randomly
                valueRanges.Add(new SamplesGeneratorLib.FunctionSamplesGenerator.ValueRange(
                    valueRange.MinValue,
                    valueRange.MaxValue));
            }

            samples = SamplesGeneratorLib.FunctionSamplesGenerator.GetSamples(
                gaSetup.NbrOutputs,
                gaSetup.TrainingFraction,
                gaSetup.FunctionSamplesGenerator.NormalizeInputs ?? false,
                dataGeneratorFunction,
                gaSetup.FunctionSamplesGenerator.NbrRecords,
                valueRanges.ToArray(),
                rnd);
        }
        // Must specify a Samples Generator
        else
        {
            throw new InvalidOperationException("Must specify a Samples Generator");
        }

        var olafInstance = GetInstance(gaSetup.OutputLayerActivationFunction);

        if (olafInstance == null)
        {
            throw new NullReferenceException($"Activation Function Instance {gaSetup.OutputLayerActivationFunction} was not loaded");
        }

        IActivationFunction outputLayerActivationFunction = (IActivationFunction)olafInstance;

        var udfInstance = GetInstance(gaSetup.UserDefinedFunctions);

        if (udfInstance == null)
        {
            throw new NullReferenceException($"User-Defiined Function Instance {gaSetup.UserDefinedFunctions} was not loaded");
        }

        IUserDefinedFunctions userDefinedFunctions = (IUserDefinedFunctions)udfInstance;

        return new SetupLib.GeneticAlgorithmSetup(
            rnd,
            gaSetup.Debug,
            gaSetup.NbrInputs,
            gaSetup.NbrOutputs,
            samples,
            gaSetup.NbrLayers,
            gaNeuronLayerConfig,
            outputLayerActivationFunction,
            gaSetup.NbrEpochs,
            gaSetup.TrainingRate,
            gaSetup.TrainingMomentum,
            userDefinedFunctions,
            gaSetup.PopulationSize,
            gaSetup.SelectionPercentage,
            gaSetup.MatingPercentage,
            gaSetup.MutationProbability,
            gaSetup.FitnessLowerBetter);
    }

    private static object GetInstance(string strFullyQualifiedName)
    {
        if (string.IsNullOrEmpty(strFullyQualifiedName))
        {
            throw new ArgumentNullException($"Instance name must be specified.");
        }

        Type type = Type.GetType(strFullyQualifiedName);
        if (type != null)
        {
            return Activator.CreateInstance(type);
        }

        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = asm.GetType(strFullyQualifiedName);
            if (type != null)
            {
                return Activator.CreateInstance(type);
            }
        }

        throw new InvalidOperationException($"Type {strFullyQualifiedName} was not found.");
    }
}
