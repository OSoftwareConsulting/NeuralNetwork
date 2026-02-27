namespace SetupLib;

internal static class SetupValidators
{
    public static void ValidateLayerConfigs(NeuronLayerConfigDto[] layerConfigs)
    {
        if (layerConfigs == null || layerConfigs.Length == 0)
        {
            throw new InvalidOperationException("Must specify Neuron Layer Configurations");
        }
    }

    public static void ValidateGANeuronLayerConfig(GANeuronLayerConfigDto layerConfig)
    {
        if (layerConfig == null)
        {
            throw new InvalidOperationException("Must specify Neuron Layer Configuration");
        }

        if (layerConfig.ActivationFunction == null || layerConfig.ActivationFunction.Length == 0)
        {
            throw new InvalidOperationException("Must specify Neuron Layer Configurations ActivationFunction");
        }

        if (layerConfig.NbrOutputs == null || layerConfig.NbrOutputs.Length == 0)
        {
            throw new InvalidOperationException("Must specify NbrOutputs for the Neuron Layer Configuration");
        }

        if (layerConfig.InitialWeightRange == null || layerConfig.InitialWeightRange.Length == 0)
        {
            throw new InvalidOperationException("Must specify Neuron Layer Configurations InitialWeightRange");
        }
    }

    public static void ValidateMemoryFilePath(string memoryFilePath)
    {
        if (string.IsNullOrEmpty(memoryFilePath))
        {
            throw new InvalidOperationException("Must specify a Memory File Path");
        }
    }

    public static void ValidateUserDefinedFunctions(string userDefinedFunctionsTypeName)
    {
        if (userDefinedFunctionsTypeName == null)
        {
            throw new InvalidOperationException("Must specify User-Defined Functions");
        }
    }

    public static void ValidateNbrEpochs(int nbrEpochs)
    {
        if (nbrEpochs <= 0)
        {
            throw new InvalidOperationException("NbrEpochs must be greater than 0");
        }
    }
}
