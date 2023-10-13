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
        public class FileSamplesGenerator
        {
            public string FilePath { get; set; }
            public char Separator { get; set; }
            public int SkipRows { get; set; }
            public int SkipColumns { get; set; }
            public int NbrOutputs { get; set; }
            public bool RandomizeSamples { get; set; }
        }

        public struct ValueRange
        {
            public double MinValue { get; set; }
            public double MaxValue { get; set; }
        }

        public class FunctionSamplesGenerator
        {
            public string SamplesGeneratorFunction { get; set; }
            public int NbrRecords { get; set; }
            public ValueRange[] ValueRanges { get; set; }
        }

        public class NeuronLayerConfig
        {
            public int NbrOutputs { get; set; }
            public string ActivationFunction { get; set; }
            public double InitialWeightRange { get; set; }
        }

        public class TestSetup
        {
            // General
            public string[] AssemblyPaths { get; set; }

            // Data Set
            public int? Seed { get; set; }
            public int NbrInputs { get; set; }
            public FileSamplesGenerator FileSamplesGenerator { get; set; }
            public FunctionSamplesGenerator FunctionSamplesGenerator { get; set; }
            public double TrainingFraction { get; set; }

            // Training
            public int NbrEpochs { get; set; }
            public double TrainingRate { get; set; }
            public double TrainingMomentum { get; set; }

            // Neural Network
            public string NeuralNetworkFuncs { get; set; }
            public NeuronLayerConfig[] LayerConfigs { get; set; }
        }

        public static TestSetupLib.TestSetup GetTestSetup(string filePath)
        {
            var testSetup = JsonConvert.DeserializeObject<TestSetup>(File.ReadAllText(filePath));

            // AssemblyPaths: If specified, AssemblyPaths is an array of file paths to assembly DLL's
            if (testSetup.AssemblyPaths != null)
            {
                // load Assemblies from specified file paths
                foreach (var assemblyPath in testSetup.AssemblyPaths)
                {
                    Assembly.LoadFrom(assemblyPath);
                }
            }

            // Seed: If specified, Seed is the int parameter to the constructor, otherwise the random number generator is created without a parameter
            Random rnd = testSetup.Seed.HasValue ? new Random(testSetup.Seed.Value) : new Random();

            // LayerConfigs: Neuron Layer Configurations (must specify at least one)
            var neuronLayerConfigs = new List<NeuralNetworkLib.NeuronLayerConfig>();
            foreach (var testSetupLayerConfig in testSetup.LayerConfigs)
            {
                // ActivationFunction: the activation function to use for this layer
                IActivationFunction activationFunction = (IActivationFunction)GetInstance(testSetupLayerConfig.ActivationFunction);

                // NbrOutputs: the number of outputs from this layer
                // InitialWeightRange: the range for the initial weight and bias values set randomly
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

            // FileSamplesGenerator: used to create training and testing samples from a file
            if (testSetup.FileSamplesGenerator != null)
            {
                // The samples file path is specified relative to the test setup JSON file
                string dataFilePath = Path.IsPathRooted(testSetup.FileSamplesGenerator.FilePath) ?
                    testSetup.FileSamplesGenerator.FilePath :
                    Path.Combine(Path.GetDirectoryName(filePath), testSetup.FileSamplesGenerator.FilePath);

                // TrainingFraction: a number between 0.0 and 1.0 to specify the fraction of the samples to use for training, and testing by implication
                // Separator: the character that separates sample values (',' for CSV, etc)
                // SkipRows: the number of rows to skip in the samples file (e.g., for headers)
                // SkipColumns: the number of colums to skip in each record (e.g., for dates)
                // RandomizeSamples: if true, the samples read in from the file are randomized in the training and testing sets, otherwise they are taken in the order in the file
                samples = SamplesGeneratorLib.FileSamplesGenerator.GetSamples(
                    nbrOutputs,
                    testSetup.TrainingFraction,
                    dataFilePath,
                    testSetup.FileSamplesGenerator.Separator,
                    testSetup.FileSamplesGenerator.SkipRows,
                    testSetup.FileSamplesGenerator.SkipColumns,
                    testSetup.FileSamplesGenerator.RandomizeSamples ? rnd : null);
            }
            // FunctionSamplesGenerator: used to create training and testing samples by calling a function with random inputs between min & max values
            else if (testSetup.FunctionSamplesGenerator != null)
            {
                // SamplesGeneratorFunction: the class that implements the ISamplesGeneratorFunction interface used to generate the outputs from the randomly generated inputs
                ISamplesGeneratorFunction dataGeneratorFunction = (ISamplesGeneratorFunction)GetInstance(testSetup.FunctionSamplesGenerator.SamplesGeneratorFunction);

                // ValueRanges: an array of min and max input values
                var valueRanges = new List<SamplesGeneratorLib.FunctionSamplesGenerator.ValueRange>();
                foreach (var valueRange in testSetup.FunctionSamplesGenerator.ValueRanges)
                {
                    // MinValue & MaxValue: specifies the range for the input set randomly
                    valueRanges.Add(new SamplesGeneratorLib.FunctionSamplesGenerator.ValueRange(
                        valueRange.MinValue,
                        valueRange.MaxValue));
                }

                // NbrRecords: the number of samples to generate
                samples = SamplesGeneratorLib.FunctionSamplesGenerator.GetSamples(
                    nbrOutputs,
                    testSetup.TrainingFraction,
                    dataGeneratorFunction,
                    testSetup.FunctionSamplesGenerator.NbrRecords,
                    valueRanges.ToArray(),
                    rnd);
            }
            // Must specify a Sample Generator
            else
            {
                throw new InvalidOperationException("Must specify a Samples Generator");
            }

            // NeuralNetworkFuncs: specifies the class implementing the INeuralNetworkFuncs interface
            INeuralNetworkFuncs neuralNetworkFuncs = (INeuralNetworkFuncs)GetInstance(testSetup.NeuralNetworkFuncs);

            return new TestSetupLib.TestSetup(
                rnd,
                testSetup.NbrInputs,
                samples,
                neuronLayerConfigs.ToArray(),
                testSetup.NbrEpochs,
                testSetup.TrainingRate,
                testSetup.TrainingMomentum,
                neuralNetworkFuncs);
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
