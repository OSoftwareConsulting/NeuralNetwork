/*
 * Copyright ©
 * 2025
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using System;
using UtilitiesLib;

namespace NeuralNetworkLib;

// Represents a Neural Network and its structure
public class NeuralNetwork
{
    // The Neuron Layers
    public NeuronLayer[] Layers { get; }

    // The random number generator to be used for all the layers' weights and biases initialization
    private readonly Random rnd;

    // The user-defined functions used during training and testing
    private readonly IUserDefinedFunctions userDefinedFunctions;

    // Used to store the errors for each training and testing sample after feed-forward processing of the sample inputs into outputs based on the sample's targets
    private readonly double[] errors;

    // Constructor for the NeuralNetwork class
    public NeuralNetwork(
        int nbrInputs,
        NeuronLayerConfig[] layerConfigs,
        Random rnd,
        IUserDefinedFunctions userDefinedFunctions)
    {
        this.rnd = rnd;
        this.userDefinedFunctions = userDefinedFunctions;

        // The number of layers L
        int nbrLayers = layerConfigs.Count();

        Layers = new NeuronLayer[nbrLayers];

        // Create the Neuron Layers based on their configuration
        for (int l = 0; l < nbrLayers; l++)
        {
            var layerConfig = layerConfigs[l];

            // Create the Neuron Layer based on its configuration
            Layers[l] = new NeuronLayer(nbrInputs, layerConfig.NbrOutputs, layerConfig.ActivationFunction, rnd, layerConfig.InitialWeightRange);

            // The number of inputs to the next layer is equal to the number of outputs from this layer
            nbrInputs = layerConfig.NbrOutputs;
        }

        // The number of outputs from the neural network equals the number of outputs (neurons) in the last (L - 1) layer
        int nbrOutputs = nbrInputs;

        errors = new double[nbrOutputs];
    }

    // Performs the training of the neural network for the given training samples
    public void Train(
        double[][] trainingInputs,
        double[][] trainingTargets,
        int nbrEpochs,
        double trainingRate,
        double trainingMomentum,
        bool debug)
    {
        int nbrTrainingSamples = trainingInputs.Length;

        // The training is divided into a number of Epochs
        for (int e = 0; e < nbrEpochs; e++)
        {
            if (debug && ((e % 10) == 0))
            {
                Console.WriteLine($"Training Epoch: {e}");
            }

            // Generate the random sequence in which the training samples are presented to the neural network in this epoch
            int[] sequence = Utilities.GenerateSequence(nbrTrainingSamples, rnd);

            // Perform the neural network training for this epoch over all training samples
            for (int i = 0; i < nbrTrainingSamples; i++)
            {
                int ii = sequence[i];

                double[] inputs = trainingInputs[ii];
                double[] targets = trainingTargets[ii];

                // Compute the outputs for the training inputs
                double[] outputs = ComputeOutputs(inputs);

                // Call the user-defined function to compute the error (difference) between the training sample's targets and neural network outputs
                userDefinedFunctions.ComputeErrors(targets, outputs, errors);

                // Update all the neuron layers' weights and biases based on the specified training rate and momentum
                Update(errors, trainingRate, trainingMomentum);
            }
        }

        if (debug)
        {
            Console.WriteLine($"Done Training");
        }
    }

    // Performs the testing of the neural network for the given testing samples
    public void Test(
        double[][] testingInputs,
        double[][] testingTargets,
        bool debug)
    {
        int nbrTestingSamples = testingInputs.Length;

        for (int i = 0; i < nbrTestingSamples; i++)
        {
            double[] inputs = testingInputs[i];
            double[] targets = testingTargets[i];

            // Compute the outputs for the testing inputs
            double[] outputs = ComputeOutputs(inputs);

            // Call the user-defined function to process the test result
            userDefinedFunctions.ProcessTestResult(i, inputs, targets, outputs, debug);
        }
    }

    // Computes the outputs for the given inputs
    private double[] ComputeOutputs(
        double[] inoutputs)
    {
        foreach (var layer in Layers)
        {
            // The outputs of one layer become the inputs to the next layer
            inoutputs = layer.ComputeOutputs(inoutputs);
        }

        // The outputs from the last layer are the outputs of the neural network
        return inoutputs;
    }

    // Updates all the layers' weights and biases
    private void Update(
        double[] errors,
        double trainingRate,
        double trainingMomentum)
    {
        // Start from the last layer back to the first layer
        for (int l = Layers.Count() - 1; l >= 0; l--)
        {
            var layer = Layers[l];

            // Errors for this layer are computed and then fed back to the previous layer
            errors = layer.ComputeErrors(errors);

            // Update the layer's weights and biases
            layer.Update(trainingRate, trainingMomentum);
        }
    }
}