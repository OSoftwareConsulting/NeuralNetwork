/*
 * Copyright ©
 * 2025
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using NeuralNetworkLib;
using SamplesGeneratorLib;

namespace SetupLib;

public class NeuralNetworkTrainAndTestSetup
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

    // The neural network's layers' configurations
    public NeuronLayerConfig[] LayerConfigs { get; }

    // The File Path for the Neural Network's Memory
    public string MemoryFilePath { get; }

    // Training

    // Number of Training Epochs (iterations)
    public int NbrEpochs { get; }

    // The Training Rate for adjusting weights and biases (from the gradients)
    public double TrainingRate { get; }

    // The Training Momentum for adjusting weights and biases (from the difference of delta's)
    public double TrainingMomentum { get; }

    // Testing

    // The user-defined functions used during neural network training and testing (see IUserDefinedFunctions)
    public IUserDefinedFunctions UserDefinedFunctions { get; }

    public NeuralNetworkTrainAndTestSetup(
        Random rnd,
        bool debug,
        Samples samples,
        NeuronLayerConfig[] layerConfigs,
        string memoryFilePath,
        int nbrEpochs,
        double trainingRate,
        double trainingMomentum,
        IUserDefinedFunctions userDefinedFunctions)
    {
        Rnd = rnd;
        Debug = debug;
        Samples = samples;
        LayerConfigs = layerConfigs;
        MemoryFilePath = memoryFilePath;
        NbrEpochs = nbrEpochs;
        TrainingRate = trainingRate;
        TrainingMomentum = trainingMomentum;
        UserDefinedFunctions = userDefinedFunctions;

        // Initialize the User Defined Functions' data structures
        UserDefinedFunctions.Configure(Samples.NbrInputs, Samples.NbrOutputs);
    }
}
