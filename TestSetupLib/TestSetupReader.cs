/*
 * Copyright ©
 * 2023
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using NeuralNetworkLib;
using NeuralNetworkLib.ActivationFunctions;
using Newtonsoft.Json;
using SamplesGeneratorLib;
using System.Reflection;

namespace TestSetupLib
{
    public static class TestSetupReader
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
        }

        // Specifies for the configuration a Neuron Layer
        public class NeuronLayerConfig
        {
            // The number of outputs from this layer
            public int NbrOutputs { get; set; }

            // The activation function to use for this layer
            public string ActivationFunction { get; set; }

            // The +/- range for the initial weight and bias values set randomly (-range .. +range)
            public double InitialWeightRange { get; set; }
        }

        // The root JSON object
        public class TestSetup
        {
            // General

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

            // Training

            // The number of training iterations
            public int NbrEpochs { get; set; }

            // The fraction of the gradient amount used to adjust the layers' neurons' weights and biases
            public double TrainingRate { get; set; }

            // The fraction of the delta amount used to adjust the layers' neurons' weights and biases
            public double TrainingMomentum { get; set; }

            // Neural Network

            // Specifies the class implementing the IUserDefinedFunctions interface
            public string UserDefinedFunctions { get; set; }

            // Neuron Layer Configurations (must specify at least one)
            public NeuronLayerConfig[] LayerConfigs { get; set; }
        }

        public static TestSetupLib.TestSetup GetTestSetup(string filePath)
        {
            var testSetup = JsonConvert.DeserializeObject<TestSetup>(File.ReadAllText(filePath));

            if (testSetup.AssemblyPaths != null)
            {
                // load Assemblies from specified file paths
                foreach (var assemblyPath in testSetup.AssemblyPaths)
                {
                    Assembly.LoadFrom(assemblyPath);
                }
            }

            Random rnd = testSetup.Seed.HasValue ? new Random(testSetup.Seed.Value) : new Random();

            var neuronLayerConfigs = new List<NeuralNetworkLib.NeuronLayerConfig>();
            foreach (var testSetupLayerConfig in testSetup.LayerConfigs)
            {
                IActivationFunction activationFunction = (IActivationFunction)GetInstance(testSetupLayerConfig.ActivationFunction);

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

            if (testSetup.FileSamplesGenerator != null)
            {
                string dataFilePath = Path.IsPathRooted(testSetup.FileSamplesGenerator.FilePath) ?
                    testSetup.FileSamplesGenerator.FilePath :
                    Path.Combine(Path.GetDirectoryName(filePath), testSetup.FileSamplesGenerator.FilePath);

                samples = SamplesGeneratorLib.FileSamplesGenerator.GetSamples(
                    nbrOutputs,
                    testSetup.TrainingFraction,
                    dataFilePath,
                    testSetup.FileSamplesGenerator.Separator,
                    testSetup.FileSamplesGenerator.SkipRows,
                    testSetup.FileSamplesGenerator.SkipColumns,
                    testSetup.FileSamplesGenerator.RandomizeSamples ? rnd : null);
            }
            else if (testSetup.FunctionSamplesGenerator != null)
            {
                ISamplesGeneratorFunction dataGeneratorFunction = (ISamplesGeneratorFunction)GetInstance(testSetup.FunctionSamplesGenerator.SamplesGeneratorFunction);

                var valueRanges = new List<SamplesGeneratorLib.FunctionSamplesGenerator.ValueRange>();
                foreach (var valueRange in testSetup.FunctionSamplesGenerator.ValueRanges)
                {
                    // MinValue & MaxValue: specifies the range for the input set randomly
                    valueRanges.Add(new SamplesGeneratorLib.FunctionSamplesGenerator.ValueRange(
                        valueRange.MinValue,
                        valueRange.MaxValue));
                }

                samples = SamplesGeneratorLib.FunctionSamplesGenerator.GetSamples(
                    nbrOutputs,
                    testSetup.TrainingFraction,
                    dataGeneratorFunction,
                    testSetup.FunctionSamplesGenerator.NbrRecords,
                    valueRanges.ToArray(),
                    rnd);
            }
            // Must specify a Samples Generator
            else
            {
                throw new InvalidOperationException("Must specify a Samples Generator");
            }

            IUserDefinedFunctions userDefinedFunctions = (IUserDefinedFunctions)GetInstance(testSetup.UserDefinedFunctions);

            return new TestSetupLib.TestSetup(
                rnd,
                testSetup.NbrInputs,
                samples,
                neuronLayerConfigs.ToArray(),
                testSetup.NbrEpochs,
                testSetup.TrainingRate,
                testSetup.TrainingMomentum,
                userDefinedFunctions);
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
}
