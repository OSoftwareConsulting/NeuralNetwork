/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using NeuralNetworkLib;
using NeuralNetworkLib.ActivationFunctions;
using Newtonsoft.Json;
using UtilitiesLib;

namespace SetupLib;

public static class SetupLoader
{
    public static NeuralNetworkTestSetup GetNeuralNetworkTestSetup(string setupFilePath)
    {
        var setup = JsonConvert.DeserializeObject<NeuralNetworkTestSetupDto>(File.ReadAllText(setupFilePath));
        if (setup == null)
        {
            throw new ArgumentNullException(nameof(setup));
        }

        var baseDirPath = Path.GetDirectoryName(setupFilePath);

        Utilities.LoadAssemblies(setup.AssemblyPaths, baseDirPath);

        Random rnd = setup.Seed.HasValue ? new Random(setup.Seed.Value) : new Random();

        var samples = SetupSamplesFactory.CreateSamples(
            baseDirPath,
            setup.FileSamplesGenerator,
            setup.FunctionSamplesGenerator,
            setup.NbrOutputs,
            rnd);

        SetupValidators.ValidateMemoryFilePath(setup.MemoryFilePath);

        var memoryFilePath = Utilities.GetAbsoluteFilePath(setup.MemoryFilePath, baseDirPath);

        SetupValidators.ValidateUserDefinedFunctions(setup.UserDefinedFunctions);

        var userDefinedFunctions = GetUserDefinedFunctionsInstance(setup.UserDefinedFunctions);

        return new NeuralNetworkTestSetup(
            rnd,
            setup.Debug,
            samples,
            memoryFilePath,
            userDefinedFunctions);
    }

    public static NeuralNetworkTrainAndTestSetup GetNeuralNetworkTrainAndTestSetup(string setupFilePath)
    {
        var setup = JsonConvert.DeserializeObject<NeuralNetworkTrainAndTestSetupDto>(File.ReadAllText(setupFilePath));
        if (setup == null)
        {
            throw new ArgumentNullException(nameof(setup));
        }

        var baseDirPath = Path.GetDirectoryName(setupFilePath);

        Utilities.LoadAssemblies(setup.AssemblyPaths, baseDirPath);

        Random rnd = setup.Seed.HasValue ? new Random(setup.Seed.Value) : new Random();

        SetupValidators.ValidateLayerConfigs(setup.LayerConfigs);

        var neuronLayerConfigs = new List<NeuronLayerConfig>();
        foreach (var testSetupLayerConfig in setup.LayerConfigs)
        {
            var activationFunction = GetActivationFunctionInstance(testSetupLayerConfig.ActivationFunction);

            neuronLayerConfigs.Add(new NeuronLayerConfig(
                testSetupLayerConfig.NbrOutputs,
                activationFunction,
                testSetupLayerConfig.InitialWeightRange));
        }

        int nbrLayers = neuronLayerConfigs.Count;

        if (nbrLayers == 0)
        {
            throw new InvalidOperationException("Must specify at least one Neuron Layer Configuration");
        }

        int nbrOutputs = neuronLayerConfigs[nbrLayers - 1].NbrOutputs;

        var samples = SetupSamplesFactory.CreateSamples(
            baseDirPath,
            setup.FileSamplesGenerator,
            setup.FunctionSamplesGenerator,
            nbrOutputs,
            rnd);

        SetupValidators.ValidateMemoryFilePath(setup.MemoryFilePath);

        var memoryFilePath = Utilities.GetAbsoluteFilePath(setup.MemoryFilePath, baseDirPath);

        SetupValidators.ValidateUserDefinedFunctions(setup.UserDefinedFunctions);

        var userDefinedFunctions = GetUserDefinedFunctionsInstance(setup.UserDefinedFunctions);

        SetupValidators.ValidateNbrEpochs(setup.NbrEpochs);

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

    public static GeneticAlgorithmSetup GetGeneticAlgorithmSetup(string setupFilePath)
    {
        var setup = JsonConvert.DeserializeObject<GeneticAlgorithmSetupDto>(File.ReadAllText(setupFilePath));
        if (setup == null)
        {
            throw new ArgumentNullException(nameof(setup));
        }

        var baseDirPath = Path.GetDirectoryName(setupFilePath);

        Utilities.LoadAssemblies(setup.AssemblyPaths, baseDirPath);

        SetupValidators.ValidateGANeuronLayerConfig(setup.LayerConfig);

        Random rnd = setup.Seed.HasValue ? new Random(setup.Seed.Value) : new Random();

        var activationFunctions = new List<IActivationFunction>();
        foreach (var activationFunctionAssemblyPath in setup.LayerConfig.ActivationFunction)
        {
            var activationFunction = GetActivationFunctionInstance(activationFunctionAssemblyPath);

            activationFunctions.Add(activationFunction);
        }

        var gaNeuronLayerConfig = new GANeuronLayerConfig(
            setup.LayerConfig.NbrOutputs,
            activationFunctions.ToArray(),
            setup.LayerConfig.InitialWeightRange);

        var samples = SetupSamplesFactory.CreateSamples(
            baseDirPath,
            setup.FileSamplesGenerator,
            setup.FunctionSamplesGenerator,
            setup.NbrOutputs,
            rnd);

        if (setup.OutputLayerActivationFunction == null)
        {
            throw new InvalidOperationException("Must specify an Output Layer Activation Function");
        }

        var outputLayerActivationFunction = GetActivationFunctionInstance(setup.OutputLayerActivationFunction);

        SetupValidators.ValidateMemoryFilePath(setup.MemoryFilePath);

        var memoryFilePath = Utilities.GetAbsoluteFilePath(setup.MemoryFilePath, baseDirPath);

        SetupValidators.ValidateUserDefinedFunctions(setup.UserDefinedFunctions);

        var userDefinedFunctions = GetUserDefinedFunctionsInstance(setup.UserDefinedFunctions);

        SetupValidators.ValidateNbrEpochs(setup.NbrEpochs);

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
