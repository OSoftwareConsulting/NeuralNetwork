/*
 * Copyright ©
 * 2023
 * Osella Ventures, LLC
 * All Rights Reserved
*/

//#define PRINTMSGS

using NeuralNetworkLib.ActivationFunctions;
using UtilitiesLib;

namespace NeuralNetworkLib
{
    public interface INeuralNetworkFuncs
    {
        void Configure(int nbrInputs, int nbrOutputs);
        void ComputeErrors(double[] targets, double[] outputs, double[] errors);
        void ProcessTestResult(int index, double[] inputs, double[] targets, double[] outputs);
        void SummarizeTestResults();
    }

    public class NeuronLayerConfig
    {
        public int NbrOutputs { get; }
        public IActivationFunction ActivationFunction { get; }
        public double InitialWeightRange { get; }

        public NeuronLayerConfig(
            int nbrOutputs,
            IActivationFunction activationFunction,
            double initialWeightRange)
        {
            NbrOutputs = nbrOutputs;
            ActivationFunction = activationFunction;
            InitialWeightRange = initialWeightRange;
        }
    }

    public class NeuralNetwork
    {
        public NeuronLayer[] Layers { get; }

        private readonly int nbrOutputs;
        private readonly Random rnd;
        private readonly INeuralNetworkFuncs neuralNetworkFuncs;
        private readonly double[] errors;

        public NeuralNetwork(
            int nbrInputs,
            NeuronLayerConfig[] layerConfigs,
            Random rnd,
            INeuralNetworkFuncs neuralNetworkFuncs)
        {
            this.rnd = rnd;
            this.neuralNetworkFuncs = neuralNetworkFuncs;

            int nbrLayers = layerConfigs.Count();

            Layers = new NeuronLayer[nbrLayers];

            for (int l = 0; l < nbrLayers; l++)
            {
                var layerConfig = layerConfigs[l];

                Layers[l] = new NeuronLayer(l, nbrInputs, layerConfig.NbrOutputs, layerConfig.ActivationFunction, rnd, layerConfig.InitialWeightRange);

                // The number of inputs to the next layer is equal to the number of outputs from this layer
                nbrInputs = layerConfig.NbrOutputs;
            }

            nbrOutputs = nbrInputs;

            errors = new double[nbrOutputs];
        }

        public void Train(
            double[][] trainingInputs,
            double[][] trainingOutputs,
            int nbrEpochs,
            double trainingRate,
            double trainingMomentum)
        {
            int nbrTrainingSamples = trainingInputs.Length;

#if PRINTMSGS
            Console.WriteLine($"Training: {nbrTrainingSamples} samples and {nbrEpochs} epochs");
#endif

            for (int e = 0; e < nbrEpochs; e++)
            {
                int[] sequence = Utilities.GenerateSequence(nbrTrainingSamples, rnd);

#if PRINTMSGS
                Console.WriteLine($"Training Epoch {e}");
#endif

                for (int i = 0; i < nbrTrainingSamples; i++)
                {
                    int ii = sequence[i];

                    double[] inputs = trainingInputs[ii];
                    double[] targets = trainingOutputs[ii];

                    double[] outputs = ComputeOutputs(inputs);

                    neuralNetworkFuncs.ComputeErrors(targets, outputs, errors);

#if PRINTMSGS
                    Console.WriteLine($"{i} -> {ii}");
                    Utilities.PrintValues(inputs, addEOL: true);
                    Utilities.PrintValues(targets, addEOL: true);
                    Utilities.PrintValues(outputs, addEOL: true);
                    Utilities.PrintValues(errors, addEOL: true);
#endif

                    Update(errors, trainingRate, trainingMomentum);
                }
            }
        }

        public void Test(
            double[][] testingInputs,
            double[][] testingOutputs)
        {
            int nbrTestingSamples = testingInputs.Length;

#if PRINTMSGS
            Console.WriteLine($"Testing: {nbrTestingSamples}");
#endif

            for (int i = 0; i < nbrTestingSamples; i++)
            {
                double[] inputs = testingInputs[i];
                double[] targets = testingOutputs[i];

                double[] outputs = ComputeOutputs(inputs);

#if PRINTMSGS
                Console.WriteLine($"{i}:");
                Utilities.PrintValues(inputs, addEOL: true);
                Utilities.PrintValues(targets, addEOL: true);
                Utilities.PrintValues(outputs, addEOL: true);
#endif

                neuralNetworkFuncs.ProcessTestResult(i, inputs, targets, outputs);
            }
        }

        private double[] ComputeOutputs(
            double[] inoutputs)
        {
            foreach (var layer in Layers)
            {
                // The outputs of one layer become the inputs to the next layer
                inoutputs = layer.ComputeOutputs(inoutputs);
            }
            // The outputs from the last layer are the outputs of the neural network
            return inoutputs;
        }

        private void Update(
            double[] errors,
            double trainingRate,
            double trainingMomentum)
        {
            for (int l = Layers.Count() - 1; l >= 0; l--)
            {
                var layer = Layers[l];

                // Errors for this layer are computed and then fed back to the previous layer
                errors = layer.ComputeErrors(errors);

                layer.Update(trainingRate, trainingMomentum);
            }
        }
    }
}