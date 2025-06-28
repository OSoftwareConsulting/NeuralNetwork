/*
 * Copyright ©
 * 2025
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using NeuralNetworkLib.ActivationFunctions;
using UtilitiesLib;

namespace NeuralNetworkLib;

// Represents a Neuron Layer and its structure
public class NeuronLayer
{
    // Data structures to perform training and testing

    public int NbrInputs { get; }           // Ni , indexed by j
    public int NbrOutputs { get; }          // No , indexed by k

    private readonly double[] outputs;      // No
    private readonly double[] inputs;       // Ni
    private readonly double[][] weights;    // No x Ni
    private readonly double[][] weightsT;   // Ni x No
    private readonly double[][] gradientsW; // No x Ni
    private readonly double[][] deltaW;     // No x Ni
    private readonly double[][] prevDeltaW; // No x Ni (for momentum calculation)
    private readonly double[] biases;       // No
    private readonly double[] gradientsB;   // No
    private readonly double[] deltaB;       // No
    private readonly double[] prevDeltaB;   // No (for momentum calculation)
    private readonly double[] nets;         // No (noted by z in the literature)
    private readonly double[] derivatives;  // No
    private readonly double[] signals;      // No
    private readonly double[] errorsOut;    // Ni (errors passed to the previous layer)

    // The layer's activation function
    public IActivationFunction ActivationFunction {  get; }

    // The random number generator to be used for the layer's weights and biases initialization
    private readonly Random rnd;

    // Constructor for the NeuronLayer class for Training
    public NeuronLayer(
        int nbrInputs,
        int nbrOutputs,
        IActivationFunction activationFunction,
        Random rnd,
        double initialWeightRange)
    {
        NbrInputs = nbrInputs;
        NbrOutputs = nbrOutputs;
        ActivationFunction = activationFunction;

        this.rnd = rnd;

        inputs = new double[nbrInputs];
        outputs = new double[nbrOutputs];
        weights = new double[nbrOutputs][];
        gradientsW = new double[nbrOutputs][];
        deltaW = new double[nbrOutputs][];
        prevDeltaW = new double[nbrOutputs][];
        for (int k = 0; k < nbrOutputs; k++)
        {
            weights[k] = new double[nbrInputs];
            gradientsW[k] = new double[nbrInputs];
            deltaW[k] = new double[nbrInputs];
            prevDeltaW[k] = new double[nbrInputs];
        }
        weightsT = new double[nbrInputs][];
        for (int j = 0; j < nbrInputs; j++)
        {
            weightsT[j] = new double[nbrOutputs];
        }
        biases = new double[nbrOutputs];
        gradientsB = new double[nbrOutputs];
        deltaB = new double[nbrOutputs];
        prevDeltaB = new double[nbrOutputs];
        nets = new double[nbrOutputs];
        derivatives = new double[nbrOutputs];
        signals = new double[nbrOutputs];
        errorsOut = new double[nbrInputs];

        Reset(initialWeightRange);
    }

    // Constructor for the NeuronLayer class for Operation
    public NeuronLayer(
        int nbrInputs,
        int nbrOutputs,
        IActivationFunction activationFunction)
    {
        NbrInputs = nbrInputs;
        NbrOutputs = nbrOutputs;
        ActivationFunction = activationFunction;

        outputs = new double[nbrOutputs];
        weights = new double[nbrOutputs][];
        for (int k = 0; k < nbrOutputs; k++)
        {
            weights[k] = new double[nbrInputs];
        }
        biases = new double[nbrOutputs];
        nets = new double[nbrOutputs];
    }

    // Performs the feed-forward processing of the inputs to the layer to compute the outputs from the layer
    public double[] ComputeOutputs(
        double[] inputs,
        bool forTraining)
    {
        // Save inputs for updating the weights
        if (forTraining)
        {
            for (int j = 0; j < NbrInputs; j++)
            {
                this.inputs[j] = inputs[j];
            }
        }

        // For each of the layer's neurons, compute its net value and output (from the layer's activation function)
        for (int k = 0; k < NbrOutputs; k++)
        {
            // Compute the net value: sum(W*x+b)

            nets[k] = biases[k];

            for (int j = 0; j < NbrInputs; j++)
            {
                nets[k] += weights[k][j] * inputs[j];
            }

            // Compute the neuron's output (activation)
            outputs[k] = ActivationFunction.Compute(nets[k]);
        }

        // Call the Layer's Activation Function's to Post-Process the layer's outputs
        ActivationFunction.PostProcess(outputs);

        return outputs;
    }

    // Computes the errors that are propagated from this layer back to the previous layer
    // Errors In -> Errors Out
    public double[] ComputeErrors(
        double[] errorsIn)
    {
        // Compute the layer's signal array
        for (int k = 0; k < NbrOutputs; k++)
        {
            derivatives[k] = ActivationFunction.Derivative(nets[k], outputs[k]);

            signals[k] = errorsIn[k] * derivatives[k];
        }

        // Compute the errors propagated to the previous layer
        for (int j = 0; j < NbrInputs; j++)
        {
            errorsOut[j] = 0.0;

            for (int k = 0; k < NbrOutputs; k++)
            {
                errorsOut[j] += weightsT[j][k] * signals[k];
            }
        }

        return errorsOut;
    }

    // Updates the layers' weight matrix and bias array based on the learning rate and momentum
    public void Update(
        double rate,
        double momentum)
    {
        // Update the layers' weight matrix and bias array
        for (int k = 0; k <NbrOutputs; k++)
        {
            // Update the neuron's bias
            gradientsB[k] = signals[k];
            deltaB[k] = rate * gradientsB[k];
            biases[k] += deltaB[k] + (prevDeltaB[k] * momentum);
            prevDeltaB[k] = deltaB[k];

            // Update the neuron's weight array
            for (int j = 0; j < NbrInputs; j++)
            {
                gradientsW[k][j] = signals[k] * inputs[j];
                deltaW[k][j] = rate * gradientsW[k][j];
                weights[k][j] += deltaW[k][j] + (prevDeltaW[k][j] * momentum);
                weightsT[j][k] = weights[k][j];
                prevDeltaW[k][j] = deltaW[k][j];
            }
        }
    }

    // Write the neural network layers's memory (weights and biases) to the specified BinaryWriter
    public void Write(BinaryWriter bw)
    {
        for (int k = 0; k < NbrOutputs; k++)
        {
            for (int j = 0; j < NbrInputs; j++)
            {
                bw.Write(weights[k][j]);
            }
        }

        for (int k = 0; k < NbrOutputs; k++)
        {
            bw.Write(biases[k]);
        }
    }

    // Read the neural network layer's memory (weights and biases) to the specified BinaryReader
    public void Read(BinaryReader br)
    {
        for (int k = 0; k < NbrOutputs; k++)
        {
            for (int j = 0; j < NbrInputs; j++)
            {
                weights[k][j] = br.ReadDouble();
            }
        }

        for (int k = 0; k < NbrOutputs; k++)
        {
            biases[k] = br.ReadDouble();
        }
    }

    // Initializes the layer's weights and biases based on the specified value +/- range
    private void Reset(
        double initialWeightRange)
    {
        double minValue = -initialWeightRange;
        double valueRange = 2 * initialWeightRange;

        for (int k = 0; k < NbrOutputs; k++)
        {
            biases[k] = rnd.NextDouble(minValue, valueRange);

            for (int j = 0; j < NbrInputs; j++)
            {
                weightsT[j][k] = weights[k][j] = rnd.NextDouble(minValue, valueRange);
            }
        }
    }
}
