/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using NeuralNetworkLib;
using NeuralNetworkLib.ActivationFunctions;
using Newtonsoft.Json;
using SamplesGeneratorLib;
using UtilitiesLib;

namespace SetupLib;

public static class SetupReader
{
    // Used to create training and testing samples from a file
    public class FileSamplesGeneratorDto
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

    public struct ValueRangeDto
    {
        // Minimum value for this range
        public double MinValue { get; set; }

        // Maximum value for this range
        public double MaxValue { get; set; }
    }

    // Used to create training and testing samples by calling a function with random inputs between min & max values
    public class FunctionSamplesGeneratorDto
    {
        // The name of the class that implements the ISamplesGeneratorFunction interface used to generate the outputs from the randomly generated inputs
        public string SamplesGeneratorFunction { get; set; }

        // The number of records to generate
        public int NbrRecords { get; set; }

        // An array of min and max input values used to generate inputs to the Samples Generator Function
        public ValueRangeDto[] ValueRanges { get; set; }

        // Perform the input normalization
        public bool? NormalizeInputs { get; set; }
    }

    // Specifies for the configuration of a Neuron Layer
    public class NeuronLayerConfigDto
    {
        // The number of outputs from this layer
        public int NbrOutputs { get; set; }

        // The activation function to use for this layer (required)
        public string ActivationFunction { get; set; }

        // The +/- range for the initial weight and bias values set randomly (-range .. +range)
        public double InitialWeightRange { get; set; }
    }

    // The root JSON object
    public class NeuralNetworkTestSetupDto
    {
        // General

        // Flag to use to enable/disable debug messages, etc.
        public bool Debug { get; set; }

        // If specified, AssemblyPaths is an array of file paths to assembly DLL's
        public string[] AssemblyPaths { get; set; }

        // Data Set

        // If specified, Seed is the int parameter to the Random class constructor, otherwise the random number generator is created without a parameter
        public int? Seed { get; set; }

        // The number of outputs from the Last Layer (Neural Network)
        public int NbrOutputs { get; set; }

        // Used to create training and testing samples from a file
        public FileSamplesGeneratorDto FileSamplesGenerator { get; set; }

        // Used to create training and testing samples by calling a function with random inputs between min & max values
        public FunctionSamplesGeneratorDto FunctionSamplesGenerator { get; set; }

        // Neural Network

        // The absolute File Path of the Neural Network's Memory
        public string MemoryFilePath { get; set; }

        // Testing

        // Specifies the class implementing the IUserDefinedFunctions interface
        public string UserDefinedFunctions { get; set; }
    }

    public static NeuralNetworkTestSetup GetNeuralNetworkTestSetup(string setupFilePath)
    {
        var setup = JsonConvert.DeserializeObject<NeuralNetworkTestSetupDto>(File.ReadAllText(setupFilePath));
        if (setup == null)
        {
            throw new ArgumentNullException(nameof(setup));
        }

        Utilities.LoadAssemblies(setup.AssemblyPaths, setupFilePath);

        Random rnd = setup.Seed.HasValue ? new Random(setup.Seed.Value) : new Random();

        if (setup.FileSamplesGenerator != null && setup.FunctionSamplesGenerator != null)
        {
            throw new InvalidOperationException("Cannot specify both a File Samples Generator and a Function Samples Generator");
        }

        Samples samples;

        if (setup.FileSamplesGenerator != null)
        {
            if (setup.FileSamplesGenerator.FilePath == null)
            {
                throw new InvalidOperationException("Must specify a File Path for the File Samples Generator");
            }

            string dataFilePath = Utilities.GetAbsoluteFilePath(setup.FileSamplesGenerator.FilePath, setupFilePath);

            samples = GetSamplesFromFile(
                setup.FileSamplesGenerator,
                dataFilePath,
                setup.NbrOutputs,
                trainingFraction: 0.0,
                rnd);
        }
        else if (setup.FunctionSamplesGenerator?.SamplesGeneratorFunction != null)
        {
            samples = GetSamplesFromDataGenerator(
                setup.FunctionSamplesGenerator,
                setup.NbrOutputs,
                trainingFraction: 0.0,
                rnd);
        }
        // Must specify a Samples Generator
        else
        {
            throw new InvalidOperationException("Must specify a Samples Generator");
        }

        if (string.IsNullOrEmpty(setup.MemoryFilePath))
        {
            throw new InvalidOperationException("Must specify a Memory File Path");
        }

        var memoryFilePath = Utilities.GetAbsoluteFilePath(setup.MemoryFilePath, setupFilePath);

        if (setup.UserDefinedFunctions == null)
        {
            throw new InvalidOperationException("Must specify User-Defined Functions");
        }

        var userDefinedFunctions = GetUserDefinedFunctionsInstance(setup.UserDefinedFunctions);

        return new NeuralNetworkTestSetup(
            rnd,
            setup.Debug,
            samples,
            memoryFilePath,
            userDefinedFunctions);
    }

    // The root JSON object
    public class NeuralNetworkTrainAndTestSetupDto
    {
        // General

        // Flag to use to enable/disable debug messages, etc.
        public bool Debug { get; set; }

        // If specified, AssemblyPaths is an array of file paths to assembly DLL's
        public string[] AssemblyPaths { get; set; }

        // Data Set

        // If specified, Seed is the int parameter to the Random class constructor, otherwise the random number generator is created without a parameter
        public int? Seed { get; set; }

        // Used to create training and testing samples from a file
        public FileSamplesGeneratorDto FileSamplesGenerator { get; set; }

        // Used to create training and testing samples by calling a function with random inputs between min & max values
        public FunctionSamplesGeneratorDto FunctionSamplesGenerator { get; set; }

        // A number between 0.0 and 1.0 to specify the fraction of the samples to use for training, and testing by implication
        public double TrainingFraction { get; set; }

        // Neural Network

        // Neuron Layer Configurations (must specify at least one)
        public NeuronLayerConfigDto[] LayerConfigs { get; set; }

        // The File Path of the Neural Network's Memory
        public string MemoryFilePath { get; set; }

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

    public static NeuralNetworkTrainAndTestSetup GetNeuralNetworkTrainAndTestSetup(string setupFilePath)
    {
        var setup = JsonConvert.DeserializeObject<NeuralNetworkTrainAndTestSetupDto>(File.ReadAllText(setupFilePath));
        if (setup == null)
        {
            throw new ArgumentNullException(nameof(setup));
        }

        Utilities.LoadAssemblies(setup.AssemblyPaths, setupFilePath);

        Random rnd = setup.Seed.HasValue ? new Random(setup.Seed.Value) : new Random();

        if (setup.TrainingFraction < 0.0 || setup.TrainingFraction > 1.0)
        {
            throw new InvalidOperationException("TrainingFraction must be between 0.0 and 1.0");
        }

        if (setup.LayerConfigs == null || (setup.LayerConfigs.Length == 0))
        {
            throw new InvalidOperationException("Must specify Neuron Layer Configurations");
        }

        var neuronLayerConfigs = new List<NeuralNetworkLib.NeuronLayerConfig>();
        foreach (var testSetupLayerConfig in setup.LayerConfigs)
        {
            var activationFunction = GetActivationFunctionInstance(testSetupLayerConfig.ActivationFunction);

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

        if (setup.FileSamplesGenerator != null && setup.FunctionSamplesGenerator != null)
        {
            throw new InvalidOperationException("Cannot specify both a File Samples Generator and a Function Samples Generator");
        }

        Samples samples;

        if (setup.FileSamplesGenerator != null)
        {
            if (setup.FileSamplesGenerator.FilePath == null)
            {
                throw new InvalidOperationException("Must specify a File Path for the File Samples Generator");
            }

            string dataFilePath = Utilities.GetAbsoluteFilePath(setup.FileSamplesGenerator.FilePath, setupFilePath);

            samples = GetSamplesFromFile(
                setup.FileSamplesGenerator,
                dataFilePath,
                nbrOutputs,
                setup.TrainingFraction,
                rnd);
        }
        else if (setup.FunctionSamplesGenerator?.SamplesGeneratorFunction != null)
        {
            samples = GetSamplesFromDataGenerator(
                setup.FunctionSamplesGenerator,
                nbrOutputs,
                setup.TrainingFraction,
                rnd);
        }
        // Must specify a Samples Generator
        else
        {
            throw new InvalidOperationException("Must specify a Samples Generator");
        }

        if (string.IsNullOrEmpty(setup.MemoryFilePath))
        {
            throw new InvalidOperationException("Must specify a Memory File Path");
        }

        var memoryFilePath = Utilities.GetAbsoluteFilePath(setup.MemoryFilePath, setupFilePath);

        if (setup.UserDefinedFunctions == null)
        {
            throw new InvalidOperationException("Must specify User-Defined Functions");
        }

        var userDefinedFunctions = GetUserDefinedFunctionsInstance(setup.UserDefinedFunctions);

        if (setup.NbrEpochs <= 0)
        {
            throw new InvalidOperationException("NbrEpochs must be greater than 0");
        }

        return new NeuralNetworkTrainAndTestSetup(
            rnd,
            setup.Debug,
            samples,
            neuronLayerConfigs.ToArray(),
            memoryFilePath,
            setup.NbrEpochs,
            setup.TrainingRate,
            setup.TrainingMomentum,
            userDefinedFunctions);
    }

    // Specifies for the configuration of a Neuron Layer
    public class GANeuronLayerConfigDto
    {
        // The number of outputs from this layer
        public int[] NbrOutputs { get; set; }

        // The activation function to use for this layer (required)
        public string[] ActivationFunction { get; set; }

        // The +/- range for the initial weight and bias values set randomly (-range .. +range)
        public double[] InitialWeightRange { get; set; }
    }

    // The root JSON object
    public class GeneticAlgorithmSetupDto
    {
        // General

        // Flag to use to enable/disable debug messages, etc.
        public bool Debug { get; set; }

        // If specified, AssemblyPaths is an array of file paths to assembly DLL's
        public string[] AssemblyPaths { get; set; }

        // Data Set

        // If specified, Seed is the int parameter to the Random class constructor, otherwise the random number generator is created without a parameter
        public int? Seed { get; set; }

        // The number of outputs from the Last Layer (Neural Network)
        public int NbrOutputs { get; set; }

        // Used to create training and testing samples from a file
        public FileSamplesGeneratorDto FileSamplesGenerator { get; set; }

        // Used to create training and testing samples by calling a function with random inputs between min & max values
        public FunctionSamplesGeneratorDto FunctionSamplesGenerator { get; set; }

        // A number between 0.0 and 1.0 to specify the fraction of the samples to use for training, and testing by implication
        public double TrainingFraction { get; set; }

        // Neural Network

        // The number of neural network layers
        public int[] NbrLayers { get; set; }

        // Neuron Layer Configuration
        public GANeuronLayerConfigDto LayerConfig { get; set; }

        // The Activation Function to use for the Output Layer
        public string OutputLayerActivationFunction { get; set; }

        // The File Path of the Neural Network's Memory
        public string MemoryFilePath { get; set; }

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

    public static GeneticAlgorithmSetup GetGeneticAlgorithmSetup(string setupFilePath)
    {
        var setup = JsonConvert.DeserializeObject<GeneticAlgorithmSetupDto>(File.ReadAllText(setupFilePath));
        if (setup == null)
        {
            throw new ArgumentNullException(nameof(setup));
        }

        Utilities.LoadAssemblies(setup.AssemblyPaths, setupFilePath);

        if (setup.LayerConfig == null)
        {
            throw new InvalidOperationException("Must specify Neuron Layer Configuration");
        }

        if (setup.LayerConfig.ActivationFunction == null || setup.LayerConfig.ActivationFunction.Length == 0)
        {
            throw new InvalidOperationException("Must specify Neuron Layer Configurations ActivationFunction");
        }

        Random rnd = setup.Seed.HasValue ? new Random(setup.Seed.Value) : new Random();

        if (setup.TrainingFraction < 0.0 || setup.TrainingFraction > 1.0)
        {
            throw new InvalidOperationException("TrainingFraction must be between 0.0 and 1.0");
        }

        var activationFunctions = new List<IActivationFunction>();
        foreach (var activationFunctionAssemblyPath in setup.LayerConfig.ActivationFunction)
        {
            var activationFunction = GetActivationFunctionInstance(activationFunctionAssemblyPath);

            activationFunctions.Add(activationFunction);
        }

        if (setup.LayerConfig.NbrOutputs == null || setup.LayerConfig.NbrOutputs.Length == 0)
        {
            throw new InvalidOperationException("Must specify NbrOutputs for the Neuron Layer Configuration");
        }

        if (setup.LayerConfig.InitialWeightRange == null || setup.LayerConfig.InitialWeightRange.Length == 0)
        {
            throw new InvalidOperationException("Must specify Neuron Layer Configurations InitialWeightRange");
        }

        var gaNeuronLayerConfig = new GANeuronLayerConfig(
            setup.LayerConfig.NbrOutputs,
            activationFunctions.ToArray(),
            setup.LayerConfig.InitialWeightRange);

        if (setup.FileSamplesGenerator != null && setup.FunctionSamplesGenerator != null)
        {
            throw new InvalidOperationException("Cannot specify both a File Samples Generator and a Function Samples Generator");
        }

        Samples samples;

        if (setup.FileSamplesGenerator != null)
        {
            if (setup.FileSamplesGenerator.FilePath == null)
            {
                throw new InvalidOperationException("Must specify a File Path for the File Samples Generator");
            }

            string dataFilePath = Utilities.GetAbsoluteFilePath(setup.FileSamplesGenerator.FilePath, setupFilePath);

            samples = GetSamplesFromFile(
                setup.FileSamplesGenerator,
                dataFilePath,
                setup.NbrOutputs,
                setup.TrainingFraction,
                rnd);
        }
        else if (setup.FunctionSamplesGenerator?.SamplesGeneratorFunction != null)
        {
            samples = GetSamplesFromDataGenerator(
                setup.FunctionSamplesGenerator,
                setup.NbrOutputs,
                setup.TrainingFraction,
                rnd);
        }
        // Must specify a Samples Generator
        else
        {
            throw new InvalidOperationException("Must specify a Samples Generator");
        }

        if (setup.OutputLayerActivationFunction == null)
        {
            throw new InvalidOperationException("Must specify an Output Layer Activation Function");
        }

        var outputLayerActivationFunction = GetActivationFunctionInstance(setup.OutputLayerActivationFunction);

        if (string.IsNullOrEmpty(setup.MemoryFilePath))
        {
            throw new InvalidOperationException("Must specify a Memory File Path");
        }

        var memoryFilePath = Utilities.GetAbsoluteFilePath(setup.MemoryFilePath, setupFilePath);

        if (setup.UserDefinedFunctions == null)
        {
            throw new InvalidOperationException("Must specify User-Defined Functions");
        }

        var userDefinedFunctions = GetUserDefinedFunctionsInstance(setup.UserDefinedFunctions);

        if (setup.NbrEpochs <= 0)
        {
            throw new InvalidOperationException("NbrEpochs must be greater than 0");
        }

        return new GeneticAlgorithmSetup(
            rnd,
            setup.Debug,
            samples,
            setup.NbrLayers,
            gaNeuronLayerConfig,
            outputLayerActivationFunction,
            memoryFilePath,
            setup.NbrEpochs,
            setup.TrainingRate,
            setup.TrainingMomentum,
            userDefinedFunctions,
            setup.PopulationSize,
            setup.SelectionPercentage,
            setup.MatingPercentage,
            setup.MutationProbability,
            setup.FitnessLowerBetter);
    }

    private static Samples GetSamplesFromFile(
        FileSamplesGeneratorDto fileSamplesGenerator,
        string dataFilePath,
        int nbrOutputs,
        double trainingFraction,
        Random rnd)
    {
        return FileSamplesGenerator.GetSamples(
            nbrOutputs,
            trainingFraction,
            fileSamplesGenerator.NormalizeInputs ?? false,
            dataFilePath,
            fileSamplesGenerator.Separator,
            fileSamplesGenerator.SkipRows,
            fileSamplesGenerator.SkipColumns,
            fileSamplesGenerator.RandomizeSamples ? rnd : null);
    }

    private static Samples GetSamplesFromDataGenerator(
        FunctionSamplesGeneratorDto functionSamplesGenerator,
        int nbrOutputs,
        double trainingFraction,
        Random rnd)
    {
        ISamplesGeneratorFunction dataGeneratorFunction = (ISamplesGeneratorFunction)Utilities.GetInstance(functionSamplesGenerator.SamplesGeneratorFunction);
        if (dataGeneratorFunction == null)
        {
            throw new InvalidOperationException($"Samples Generator Function Instance {functionSamplesGenerator.SamplesGeneratorFunction} was not loaded");
        }

        if (functionSamplesGenerator.ValueRanges == null || functionSamplesGenerator.ValueRanges.Length == 0)
        {
            throw new InvalidOperationException("Must specify at least one Value Range for the Function Samples Generator");
        }

        var valueRanges = new List<FunctionSamplesGenerator.ValueRange>();
        foreach (var valueRange in functionSamplesGenerator.ValueRanges)
        {
            // MinValue & MaxValue: specifies the range for the input set randomly
            valueRanges.Add(new FunctionSamplesGenerator.ValueRange(
                valueRange.MinValue,
                valueRange.MaxValue));
        }

        Samples samples = FunctionSamplesGenerator.GetSamples(
            nbrOutputs,
            trainingFraction,
            functionSamplesGenerator.NormalizeInputs ?? false,
            dataGeneratorFunction,
            functionSamplesGenerator.NbrRecords,
            valueRanges.ToArray(),
            rnd);

        return samples;
    }

    private static IActivationFunction GetActivationFunctionInstance(string strFullyQualifiedName)
    {
        var activationFunction = (IActivationFunction)Utilities.GetInstance(strFullyQualifiedName);
        if (activationFunction == null)
        {
            throw new InvalidOperationException($"Activation Function Instance {strFullyQualifiedName} was not loaded");
        }

        activationFunction.TypeName = strFullyQualifiedName;

        return activationFunction;
    }

    private static IUserDefinedFunctions GetUserDefinedFunctionsInstance(string userDefinedFunctionsTypeName)
    {
        var userDefinedFunctions = (IUserDefinedFunctions)Utilities.GetInstance(userDefinedFunctionsTypeName);
        if (userDefinedFunctions == null)
        {
            throw new InvalidOperationException($"User-Defined Functions Instance {userDefinedFunctionsTypeName} was not loaded");
        }

        return userDefinedFunctions;
    }
}
