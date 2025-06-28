/*
 * Copyright ©
 * 2025
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using NeuralNetworkLib;
using SetupLib;

namespace NeuralNetworkExec;

public static class TrainAndTestMain
{
    public static void Main(string filePath)
    {
        var setup = SetupReader.GetNeuralNetworkTrainAndTestSetup(filePath);

        var neuralNetwork = new NeuralNetwork(
            setup.Samples.NbrInputs,
            setup.LayerConfigs,
            setup.UserDefinedFunctions,
            setup.Rnd);

        neuralNetwork.Train(
            setup.Samples.TrainingInputs,
            setup.Samples.TrainingTargets,
            setup.NbrEpochs,
            setup.TrainingRate,
            setup.TrainingMomentum,
            setup.Debug);

        neuralNetwork.Test(
            setup.Samples.TestingInputs,
            setup.Samples.TestingTargets,
            setup.Debug);

        setup.UserDefinedFunctions.SummarizeTestResults(setup.Debug);

        if (setup.MemoryFilePath != null)
        {
            neuralNetwork.Save(setup.MemoryFilePath);
        }
    }
}
