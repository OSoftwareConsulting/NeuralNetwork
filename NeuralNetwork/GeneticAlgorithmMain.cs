/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using GeneticAlgorithmLib;
using NeuralNetworkLib;
using NeuralNetworkLib.ActivationFunctions;
using SetupLib;
using System.Text;

namespace NeuralNetworkExec;

public enum SettingTypes
{
    DOUBLE_ARRAY_INDEX,
    INT_ARRAY_INDEX,
    OBJECT_ARRAY_INDEX,
    DOUBLE_IN_RANGE,
    INT_IN_RANGE
}

public enum SettingIndexes
{
    TRAINING_RATE = 0,
    TRAINING_MOMENTUM = 1,
    NBR_LAYERS = 2,
    NBR_OUTPUTS = 3,
    ACTIVATION_FUNCTION = 4,
    INITIAL_WEIGHT_RANGE = 5
}

public static class NeuralNetworkSettingGenerator
{
    public static GeneticAlgorithmSetup Setup { get; set; }

    public static Gene RandomSettingValue(
        SettingIndexes index,
        SettingTypes type,
        object values)
    {
        switch (type)
        {
            case SettingTypes.DOUBLE_ARRAY_INDEX:
                {
                    int length = ((double[])values).Length;
                    return new Setting<int>(index, type, values, RandomIndex(length));
                }

            case SettingTypes.INT_ARRAY_INDEX:
                {
                    int length = ((int[])values).Length;
                    return new Setting<int>(index, type, values, RandomIndex(length));
                }

            case SettingTypes.OBJECT_ARRAY_INDEX:
                {
                    int length = ((object[])values).Length;
                    return new Setting<int>(index, type, values, RandomIndex(length));
                }

            case SettingTypes.DOUBLE_IN_RANGE:
                {
                    double[] range = (double[])values;
                    return new Setting<double>(index, type, values, RandomDouble(range[0], range[1]));
                }

            case SettingTypes.INT_IN_RANGE:
                {
                    int[] range = (int[])values;
                    return new Setting<int>(index, type, values, RandomInt(range[0], range[1]));
                }

            default:
                throw new NotImplementedException();
        }
    }

    private static int RandomIndex(int length) => Setup.Rnd.Next(0, length);
    private static double RandomDouble(double min, double max) => min + (Setup.Rnd.NextDouble() * (max - min));
    private static int RandomInt(int min, int max) => Setup.Rnd.Next(min, max + 1);
}

public class Setting<T> : Gene
{
    // Only allow fields to be set during initialization
    public SettingIndexes index { get; init; }
    public SettingTypes type { get; init; }
    public object values { get; init; }
    public T value { get; init; }

    public Setting(
        SettingIndexes _index,
        SettingTypes _type,
        object _values,
        T _value)
    {
        index = _index;
        type = _type;
        values = _values;
        value = _value;
    }

    public override Gene Mutate() => NeuralNetworkSettingGenerator.RandomSettingValue(index, type, values);
}

public class NeuralNetworkInstance : GAIndividual
{
    public NeuralNetwork NeuralNetwork { get; private set; }

    public NeuralNetworkInstance(bool construct) : base()
    {
        if (!construct)
        {
            return;
        }

        var setup = NeuralNetworkSettingGenerator.Setup;

        AddGene(NeuralNetworkSettingGenerator.RandomSettingValue(SettingIndexes.TRAINING_RATE, SettingTypes.DOUBLE_ARRAY_INDEX, setup.TrainingRate));
        AddGene(NeuralNetworkSettingGenerator.RandomSettingValue(SettingIndexes.TRAINING_MOMENTUM, SettingTypes.DOUBLE_ARRAY_INDEX, setup.TrainingMomentum));
        AddGene(NeuralNetworkSettingGenerator.RandomSettingValue(SettingIndexes.NBR_LAYERS, SettingTypes.INT_IN_RANGE, setup.NbrLayers));
        AddGene(NeuralNetworkSettingGenerator.RandomSettingValue(SettingIndexes.NBR_OUTPUTS, SettingTypes.INT_IN_RANGE, setup.LayerConfig.NbrOutputs));
        AddGene(NeuralNetworkSettingGenerator.RandomSettingValue(SettingIndexes.ACTIVATION_FUNCTION, SettingTypes.OBJECT_ARRAY_INDEX, setup.LayerConfig.ActivationFunction));
        AddGene(NeuralNetworkSettingGenerator.RandomSettingValue(SettingIndexes.INITIAL_WEIGHT_RANGE, SettingTypes.DOUBLE_IN_RANGE, setup.LayerConfig.InitialWeightRange));
    }

    public override void ComputeFitness()
    {
        if (FitnessComputed)
        {
            return;
        }

        var gaSetup = NeuralNetworkSettingGenerator.Setup;
        var settings = GetNeuralNetworkSettings();

        int L = settings.NbrLayers - 1;
        var layerConfigs = new List<NeuronLayerConfig>();
        for (int i = 0; i < settings.NbrLayers; i++)
        {
            var layerConfig = new NeuronLayerConfig(
                nbrOutputs: (i == L) ? gaSetup.Samples.NbrOutputs : settings.NbrOutputs,
                activationFunction: (i == L) ? gaSetup.OutputLayerActivationFunction : settings.ActivationFunction,
                initialWeightRange: settings.InitialWeightRange);

            layerConfigs.Add(layerConfig);
        }

        var nnSetup = new NeuralNetworkTrainAndTestSetup(
            rnd: gaSetup.Rnd,
            debug: gaSetup.Debug,
            samples: gaSetup.Samples,
            layerConfigs: layerConfigs.ToArray(),
            memoryFilePath: gaSetup.MemoryFilePath,
            nbrEpochs: gaSetup.NbrEpochs,
            trainingRate: settings.TrainingRate,
            trainingMomentum: settings.TrainingMomentum,
            userDefinedFunctions: gaSetup.UserDefinedFunctions);

        Console.WriteLine($"Computing Fitness: {Id}\n\n{ToString()}");

        NeuralNetwork = new NeuralNetwork(
            nnSetup.Samples.NbrInputs,
            nnSetup.LayerConfigs,
            nnSetup.UserDefinedFunctions,
            nnSetup.Rnd);

        NeuralNetwork.Train(
            nnSetup.Samples.TrainingInputs,
            nnSetup.Samples.TrainingTargets,
            nnSetup.NbrEpochs,
            nnSetup.TrainingRate,
            nnSetup.TrainingMomentum,
            nnSetup.Debug);

        NeuralNetwork.Test(
            nnSetup.Samples.TestingInputs,
            nnSetup.Samples.TestingTargets,
            nnSetup.Debug);

        nnSetup.UserDefinedFunctions.SummarizeTestResults(nnSetup.Debug);

        Fitness = nnSetup.UserDefinedFunctions.ComputeScore();

        Console.WriteLine($"\n\tFitness: {Fitness:F4}\n");

        FitnessComputed = true;
    }

    public override string ToString()
    {
        var settings = GetNeuralNetworkSettings();

        StringBuilder sb = new StringBuilder();

        sb.Append($"\t{SettingIndexes.TRAINING_RATE.ToString()}: {settings.TrainingRate:F4}\n");
        sb.Append($"\t{SettingIndexes.TRAINING_MOMENTUM.ToString()}: {settings.TrainingMomentum:F4}\n");
        sb.Append($"\t{SettingIndexes.NBR_LAYERS.ToString()}: {settings.NbrLayers}\n");
        sb.Append($"\t{SettingIndexes.NBR_OUTPUTS.ToString()}: {settings.NbrOutputs}\n");
        sb.Append($"\t{SettingIndexes.ACTIVATION_FUNCTION.ToString()}: {settings.ActivationFunction}\n");
        sb.Append($"\t{SettingIndexes.INITIAL_WEIGHT_RANGE.ToString()}: {settings.InitialWeightRange:F4}");
        if (FitnessComputed)
        {
            sb.Append($"\n\tFitness: {Fitness:F4}");
        }

        return sb.ToString();
    }

    private Setting<T> GetSettingAt<T>(int index) => Chromosome[index] as Setting<T>;
    private T GetAt<T>(int index) => GetSettingAt<T>(index).value;

    private NeuralNetworkSettings GetNeuralNetworkSettings()
    {
        var gaSetup = NeuralNetworkSettingGenerator.Setup;

        return new NeuralNetworkSettings(
            trainingRate: gaSetup.TrainingRate[GetAt<int>((int)SettingIndexes.TRAINING_RATE)],
            trainingMomentum: gaSetup.TrainingMomentum[GetAt<int>((int)SettingIndexes.TRAINING_MOMENTUM)],
            nbrLayers: GetAt<int>((int)SettingIndexes.NBR_LAYERS),
            nbrOutputs: GetAt<int>((int)SettingIndexes.NBR_OUTPUTS),
            activationFunction: gaSetup.LayerConfig.ActivationFunction[GetAt<int>((int)SettingIndexes.ACTIVATION_FUNCTION)],
            initialWeightRange: GetAt<double>((int)SettingIndexes.INITIAL_WEIGHT_RANGE));
    }

    public class NeuralNetworkSettings
    {
        public double TrainingRate { get; }
        public double TrainingMomentum { get; }
        public int NbrLayers { get; }
        public int NbrOutputs { get; }
        public IActivationFunction ActivationFunction { get; }
        public double InitialWeightRange { get; }

        public NeuralNetworkSettings(
            double trainingRate,
            double trainingMomentum,
            int nbrLayers,
            int nbrOutputs,
            IActivationFunction activationFunction,
            double initialWeightRange)
        {
            TrainingRate = trainingRate;
            TrainingMomentum = trainingMomentum;
            NbrLayers = nbrLayers;
            NbrOutputs = nbrOutputs;
            ActivationFunction = activationFunction;
            InitialWeightRange = initialWeightRange;
        }
    }

}

public class NeuralNetworkGeneticAlgorithm : GeneticAlgorithm
{
    private readonly GeneticAlgorithmSetup setup;

    public NeuralNetworkGeneticAlgorithm(GeneticAlgorithmSetup setup) : base(
        random: setup.Rnd,
        populationSize: setup.PopulationSize,
        selectionPercentage: setup.SelectionPercentage,
        matingPercentage: setup.MatingPercentage,
        mutationProbability: setup.MutationProbability,
        fitnessLowerBetter: setup.FitnessLowerBetter)
    {
        this.setup = setup;
    }

    protected override GAIndividual CreateIndividual(bool construct) => new NeuralNetworkInstance(construct);

    protected override void SummarizeGeneration()
    {
        Console.WriteLine($"Generation: {Generation,4}\n");
        foreach (var individual in OrderedPopulation)
        {
            Console.WriteLine($"{individual.ToString()}\n");
        }

        var optimalNeuralNetwork = FittestIndividual() as NeuralNetworkInstance;
        if (optimalNeuralNetwork.NeuralNetwork != null && setup.MemoryFilePath != null)
        {
            optimalNeuralNetwork.NeuralNetwork.Save(setup.MemoryFilePath);
        }
    }

    protected override bool SearchCompleted()
    {
        return Generation == 100;
    }
}

public static class GeneticAlgorithmMain
{
    public static void Main(string filePath)
    {
        var setup = SetupLoader.GetGeneticAlgorithmSetup(filePath);

        NeuralNetworkSettingGenerator.Setup = setup;

        var ga = new NeuralNetworkGeneticAlgorithm(setup);

        ga.Search();
    }
}
