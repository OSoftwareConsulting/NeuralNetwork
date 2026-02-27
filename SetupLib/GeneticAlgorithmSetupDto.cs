/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace SetupLib;

// The root JSON object
public class GeneticAlgorithmSetupDto
{
    public bool Debug { get; set; }

    public string[] AssemblyPaths { get; set; }

    public int? Seed { get; set; }

    public int NbrOutputs { get; set; }

    public FileSamplesGeneratorDto FileSamplesGenerator { get; set; }

    public FunctionSamplesGeneratorDto FunctionSamplesGenerator { get; set; }

    public int[] NbrLayers { get; set; }

    public GANeuronLayerConfigDto LayerConfig { get; set; }

    public string OutputLayerActivationFunction { get; set; }

    public string MemoryFilePath { get; set; }

    public int NbrEpochs { get; set; }

    public double[] TrainingRate { get; set; }

    public double[] TrainingMomentum { get; set; }

    public string UserDefinedFunctions { get; set; }

    public int PopulationSize { get; set; }

    public double SelectionPercentage { get; set; }

    public double MatingPercentage { get; set; }

    public double MutationProbability { get; set; }

    public bool FitnessLowerBetter { get; set; }
}
