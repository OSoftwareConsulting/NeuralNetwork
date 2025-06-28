/*
 * Copyright ©
 * 2025
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using NeuralNetworkLib;
using SetupLib;

namespace NeuralNetworkExec;

public static class TestMain
{
    public static void Main(string filePath)
    {
        var setup = SetupReader.GetNeuralNetworkTestSetup(filePath);

        var neuralNetwork = NeuralNetwork.Load(
            setup.MemoryFilePath,
            setup.UserDefinedFunctions);

        neuralNetwork.Test(
            setup.Samples.TestingInputs,
            setup.Samples.TestingTargets,
            setup.Debug);

        setup.UserDefinedFunctions.SummarizeTestResults(setup.Debug);
    }
}
