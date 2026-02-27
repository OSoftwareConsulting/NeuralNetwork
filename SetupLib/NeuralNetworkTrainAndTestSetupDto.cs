/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace SetupLib;

// The root JSON object
public class NeuralNetworkTrainAndTestSetupDto
{
    public bool Debug { get; set; }

    public string[] AssemblyPaths { get; set; }

    public int? Seed { get; set; }

    public FileSamplesGeneratorDto FileSamplesGenerator { get; set; }

    public FunctionSamplesGeneratorDto FunctionSamplesGenerator { get; set; }

    public NeuronLayerConfigDto[] LayerConfigs { get; set; }

    public string MemoryFilePath { get; set; }

    public int NbrEpochs { get; set; }

    public double TrainingRate { get; set; }

    public double TrainingMomentum { get; set; }

    public string UserDefinedFunctions { get; set; }
}
