/*
 * Copyright ©
 * 2025
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using NeuralNetworkLib;
using SamplesGeneratorLib;

namespace SetupLib;

public class NeuralNetworkSetup
{
    // General

    // The Random number generator used to compute all random numbers
    public Random Rnd { get; }

    // Flag to use to enable/disable debug messages, etc.
    public bool Debug { get; }

    // Data Set

    // Number of Neural Network Inputs
    public int NbrInputs { get; }

    // Number of Neural Network Outputs
    public int NbrOutputs { get; }

    // The Training and Testing Samples
    public Samples Samples { get; }

    // Neural Network

    // The neural network's layers' configurations
    public NeuronLayerConfig[] LayerConfigs { get; }

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

    public NeuralNetworkSetup(
        Random rnd,
        bool debug,
        int nbrInputs,
        Samples samples,
        NeuronLayerConfig[] layerConfigs,
        int nbrEpochs,
        double trainingRate,
        double trainingMomentum,
        IUserDefinedFunctions userDefinedFunctions)
    {
        Rnd = rnd;
        Debug = debug;
        NbrInputs = nbrInputs;
        NbrOutputs = layerConfigs[layerConfigs.Count() - 1].NbrOutputs; // The number of outputs from the neural network is equal to the number of outputs from the last layer (L - 1)
        Samples = samples;
        LayerConfigs = layerConfigs;
        NbrEpochs = nbrEpochs;
        TrainingRate = trainingRate;
        TrainingMomentum = trainingMomentum;
        UserDefinedFunctions = userDefinedFunctions;

        // Initialize the User Defined Functions' data structures
        UserDefinedFunctions.Configure(NbrInputs, NbrOutputs);
    }
}
