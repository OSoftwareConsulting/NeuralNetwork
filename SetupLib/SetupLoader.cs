/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using NeuralNetworkLib;

namespace SetupLib;

public static class SetupLoader
{
    private static readonly SetupLoaderService Loader = new(
        new SetupSamplesResolver(),
        new UtilitiesTypeActivator(),
        new UtilitiesPathResolver(),
        new UtilitiesAssemblyLoader());

    public static NeuralNetworkTestSetup GetNeuralNetworkTestSetup(string setupFilePath)
    {
        return Loader.GetNeuralNetworkTestSetup(setupFilePath);
    }

    public static NeuralNetworkTrainAndTestSetup GetNeuralNetworkTrainAndTestSetup(string setupFilePath)
    {
        return Loader.GetNeuralNetworkTrainAndTestSetup(setupFilePath);
    }

    public static GeneticAlgorithmSetup GetGeneticAlgorithmSetup(string setupFilePath)
    {
        return Loader.GetGeneticAlgorithmSetup(setupFilePath);
    }
}
