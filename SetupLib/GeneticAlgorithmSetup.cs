/*
 * Copyright ©
 * 2025
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using NeuralNetworkLib;
using NeuralNetworkLib.ActivationFunctions;
using SamplesGeneratorLib;

namespace SetupLib;

public class GANeuronLayerConfig
{
    // The number of neurons (outputs) in the layer (Min & Max)
    public int[] NbrOutputs { get; }

    // The Activation Functions to use in the layer
    public IActivationFunction[] ActivationFunction { get; }

    // The range for the initial values of the layer's neuron weights and bias (Min & Max)
    public double[] InitialWeightRange { get; }

    public GANeuronLayerConfig(
        int[] nbrOutputs,
        IActivationFunction[] activationFunction,
        double[] initialWeightRange)
    {
        NbrOutputs = nbrOutputs;
        ActivationFunction = activationFunction;
        InitialWeightRange = initialWeightRange;
    }
}

public class GeneticAlgorithmSetup
{
    // General

    // The Random number generator used to compute all random numbers
    public Random Rnd { get; }

    // Flag to use to enable/disable debug messages, etc.
    public bool Debug { get; }

    // Data Set

    // The Training and Testing Samples
    public Samples Samples { get; }

    // Neural Network

    // Number of Layers Min & Max
    public int[] NbrLayers { get; }

    // The neural network's layers' configurations
    public GANeuronLayerConfig LayerConfig { get; }

    // The Activation Function to use for the Output Layer
    public IActivationFunction OutputLayerActivationFunction { get; }

    // The File Path for the Neural Network's Memory
    public string MemoryFilePath { get; }

    // Training

    // Number of Training Epochs (iterations)
    public int NbrEpochs { get; }

    // The Training Rate for adjusting weights and biases (from the gradients)
    public double[] TrainingRate { get; }

    // The Training Momentum for adjusting weights and biases (from the difference of delta's)
    public double[] TrainingMomentum { get; }

    // Testing

    // The user-defined functions used during neural network training and testing (see IUserDefinedFunctions)
    public IUserDefinedFunctions UserDefinedFunctions { get; }

    // Genetic Algorithm

    // The number of GA Individuals
    public int PopulationSize { get; }

    // The fraction of the population selected to be included in the next generation
    public double SelectionPercentage { get; }

    // The fraction of the population selected to be mated in the next generation
    public double MatingPercentage { get; }

    // The probability fraction (0.0 to 1.0) that a gene will be mutated during mating
    public double MutationProbability { get; }

    // True when a lower fitness value is more optimal (e.g., errors), and 
    // False when a hihger fitness value is more optimal (e.g., test pass percentages)
    public bool FitnessLowerBetter { get; }

    public GeneticAlgorithmSetup(
        Random rnd,
        bool debug,
        Samples samples,
        int[] nbrLayers,
        GANeuronLayerConfig layerConfig,
        IActivationFunction outputLayerActivationFunction,
        string memoryFilePath,
        int nbrEpochs,
        double[] trainingRate,
        double[] trainingMomentum,
        IUserDefinedFunctions userDefinedFunctions,
        int populationSize,
        double selectionPercentage,
        double matingPercentage,
        double mutationProbability,
        bool fitnessLowerBetter)
    {
        Rnd = rnd;
        Debug = debug;
        Samples = samples;
        NbrLayers = nbrLayers;
        LayerConfig = layerConfig;
        OutputLayerActivationFunction = outputLayerActivationFunction;
        MemoryFilePath = memoryFilePath;
        NbrEpochs = nbrEpochs;
        TrainingRate = trainingRate;
        TrainingMomentum = trainingMomentum;
        UserDefinedFunctions = userDefinedFunctions;
        PopulationSize = populationSize;
        SelectionPercentage = selectionPercentage;
        MatingPercentage = matingPercentage;
        MutationProbability = mutationProbability;
        FitnessLowerBetter = fitnessLowerBetter;
    }
}
