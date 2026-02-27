using NeuralNetworkLib;
using NeuralNetworkLib.ActivationFunctions;
using Newtonsoft.Json;

namespace SetupLib;

internal sealed class SetupLoaderService
{
    private readonly ISamplesFactory _samplesFactory;
    private readonly ITypeActivator _typeActivator;
    private readonly IPathResolver _pathResolver;
    private readonly IAssemblyLoader _assemblyLoader;

    public SetupLoaderService(
        ISamplesFactory samplesFactory,
        ITypeActivator typeActivator,
        IPathResolver pathResolver,
        IAssemblyLoader assemblyLoader)
    {
        _samplesFactory = samplesFactory ?? throw new ArgumentNullException(nameof(samplesFactory));
        _typeActivator = typeActivator ?? throw new ArgumentNullException(nameof(typeActivator));
        _pathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
        _assemblyLoader = assemblyLoader ?? throw new ArgumentNullException(nameof(assemblyLoader));
    }

    public NeuralNetworkTestSetup GetNeuralNetworkTestSetup(string setupFilePath)
    {
        var setup = JsonConvert.DeserializeObject<NeuralNetworkTestSetupDto>(File.ReadAllText(setupFilePath));
        if (setup == null)
        {
            throw new ArgumentNullException(nameof(setup));
        }

        var baseDirPath = Path.GetDirectoryName(setupFilePath);

        _assemblyLoader.LoadAssemblies(setup.AssemblyPaths, baseDirPath);

        Random rnd = setup.Seed.HasValue ? new Random(setup.Seed.Value) : new Random();

        var samples = _samplesFactory.CreateSamples(
            baseDirPath,
            setup.FileSamplesGenerator,
            setup.FunctionSamplesGenerator,
            setup.NbrOutputs,
            rnd);

        SetupValidators.ValidateMemoryFilePath(setup.MemoryFilePath);

        var memoryFilePath = _pathResolver.GetAbsoluteFilePath(setup.MemoryFilePath, baseDirPath);

        SetupValidators.ValidateUserDefinedFunctions(setup.UserDefinedFunctions);

        var userDefinedFunctions = GetUserDefinedFunctionsInstance(setup.UserDefinedFunctions);

        return new NeuralNetworkTestSetup(
            rnd,
            setup.Debug,
            samples,
            memoryFilePath,
            userDefinedFunctions);
    }

    public NeuralNetworkTrainAndTestSetup GetNeuralNetworkTrainAndTestSetup(string setupFilePath)
    {
        var setup = JsonConvert.DeserializeObject<NeuralNetworkTrainAndTestSetupDto>(File.ReadAllText(setupFilePath));
        if (setup == null)
        {
            throw new ArgumentNullException(nameof(setup));
        }

        var baseDirPath = Path.GetDirectoryName(setupFilePath);

        _assemblyLoader.LoadAssemblies(setup.AssemblyPaths, baseDirPath);

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

        var samples = _samplesFactory.CreateSamples(
            baseDirPath,
            setup.FileSamplesGenerator,
            setup.FunctionSamplesGenerator,
            nbrOutputs,
            rnd);

        SetupValidators.ValidateMemoryFilePath(setup.MemoryFilePath);

        var memoryFilePath = _pathResolver.GetAbsoluteFilePath(setup.MemoryFilePath, baseDirPath);

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

    public GeneticAlgorithmSetup GetGeneticAlgorithmSetup(string setupFilePath)
    {
        var setup = JsonConvert.DeserializeObject<GeneticAlgorithmSetupDto>(File.ReadAllText(setupFilePath));
        if (setup == null)
        {
            throw new ArgumentNullException(nameof(setup));
        }

        var baseDirPath = Path.GetDirectoryName(setupFilePath);

        _assemblyLoader.LoadAssemblies(setup.AssemblyPaths, baseDirPath);

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

        var samples = _samplesFactory.CreateSamples(
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

        var memoryFilePath = _pathResolver.GetAbsoluteFilePath(setup.MemoryFilePath, baseDirPath);

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

    private IActivationFunction GetActivationFunctionInstance(string strFullyQualifiedName)
    {
        var activationFunction = (IActivationFunction)_typeActivator.GetInstance(strFullyQualifiedName);
        if (activationFunction == null)
        {
            throw new InvalidOperationException($"Activation Function Instance {strFullyQualifiedName} was not loaded");
        }

        activationFunction.TypeName = strFullyQualifiedName;

        return activationFunction;
    }

    private IUserDefinedFunctions GetUserDefinedFunctionsInstance(string userDefinedFunctionsTypeName)
    {
        var userDefinedFunctions = (IUserDefinedFunctions)_typeActivator.GetInstance(userDefinedFunctionsTypeName);
        if (userDefinedFunctions == null)
        {
            throw new InvalidOperationException($"User-Defined Functions Instance {userDefinedFunctionsTypeName} was not loaded");
        }

        return userDefinedFunctions;
    }
}
