/*
 * Copyright ©
 * 2023
 * Osella Ventures, LLC
 * All Rights Reserved
*/

//#define PRINTMSGS

using NeuralNetworkLib;
using SamplesGeneratorLib;

namespace TestSetupLib
{
    public class TestSetup
    {
        // General
        public Random Rnd { get; }

        // Data Set
        public int NbrInputs { get; }
        public int NbrOutputs { get; }
        public Samples Samples { get; }

        // Neural Network
        public NeuronLayerConfig[] LayerConfigs { get; }

        // Training
        public int NbrEpochs { get; }
        public double TrainingRate { get; }
        public double TrainingMomentum { get; }

        // Testing
        public INeuralNetworkFuncs NeuralNetworkFuncs { get; }

        public TestSetup(
            Random rnd,
            int nbrInputs,
            Samples samples,
            NeuronLayerConfig[] layerConfigs,
            int nbrEpochs,
            double trainingRate,
            double trainingMomentum,
            INeuralNetworkFuncs neuralNetworkFuncs)
        {
            Rnd = rnd;
            NbrInputs = nbrInputs;
            NbrOutputs = layerConfigs[layerConfigs.Count() - 1].NbrOutputs; // The number of outputs is the number of outputs in the output layer
            Samples = samples;
            LayerConfigs = layerConfigs;
            NbrEpochs = nbrEpochs;
            TrainingRate = trainingRate;
            TrainingMomentum = trainingMomentum;
            NeuralNetworkFuncs = neuralNetworkFuncs;

            NeuralNetworkFuncs.Configure(NbrInputs, NbrOutputs);

#if PRINTMSGS
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.Append($"{NbrInputs}");
            for (int l = 0; l < LayerConfigs.Length; l++)
            {
                sb.Append($"-{LayerConfigs[l].NbrOutputs}");
            }

            Console.WriteLine($"Creating a {sb} network");
#endif
        }
    }
}
