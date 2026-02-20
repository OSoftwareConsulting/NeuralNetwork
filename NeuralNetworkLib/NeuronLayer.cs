/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using NeuralNetworkLib.ActivationFunctions;
using UtilitiesLib;

namespace NeuralNetworkLib;

// Represents a Neuron Layer and its structure
public abstract class NeuronLayer
{
    // Data structures to perform training and testing
    public int NbrInputs { get; }           // Ni , indexed by j
    public int NbrOutputs { get; }          // No , indexed by k

    // The layer's activation function
    public IActivationFunction ActivationFunction { get; }

    // Data structures to perform training and testing
    protected readonly double[] outputs;    // No
    protected readonly double[][] weights;  // No x Ni
    protected readonly double[] biases;     // No
    protected readonly double[] nets;       // No (noted by z in the literature)

    /// <summary>
    /// Base constructor for the NeuronLayer class that initializes the layer's data structures based on the number of inputs, number of outputs, and activation function
    /// </summary>
    protected NeuronLayer(
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

    /// <summary>
    /// Abstract method that computes the layer's outputs based on the specified inputs and the layer's weights, biases, and activation function
    /// </summary>
    public abstract double[] ComputeOutputs(double[] inputs);

    /// <summary>
    /// Hook invoked after input validation and before forward computation. Override to capture inputs or prepare state without mutating the input array.
    /// </summary>
    protected virtual void OnBeforeCompute(double[] inputs)
    {
    }

    /// <summary>
    /// Validates input size, performs any pre-compute hook, and then computes the layer outputs
    /// </summary>
    protected virtual double[] ComputeOutputsBase(
        double[] inputs)
    {
        // Validate the number of inputs
        if (inputs.Length != NbrInputs)
        {
            throw new ArgumentException(
                $"Expected {NbrInputs} inputs but received {inputs.Length}.",
                nameof(inputs));
        }

        // Call the pre-compute hook to allow derived classes to perform any necessary processing before the outputs are computed
        OnBeforeCompute(inputs);

        // Cache the inputs in a local variable for faster access in the loop below
        var inputValues = inputs;

        // For each of the layer's neurons, compute its net value and output (from the layer's activation function)
        for (int k = 0; k < NbrOutputs; k++)
        {
            // Compute the net value: sum(W*x+b)
            nets[k] = biases[k];
            var weightsK = weights[k];

            for (int j = 0; j < NbrInputs; j++)
            {
                nets[k] += weightsK[j] * inputValues[j];
            }

            // Compute the neuron's output (activation)
            outputs[k] = ActivationFunction.Compute(nets[k]);
        }

        // Call the Layer's Activation Function's to Post-Process the layer's outputs
        ActivationFunction.PostProcess(outputs);

        return outputs;
    }

    /// <summary>
    /// Write the neural network layers's memory (weights and biases) to the specified BinaryWriter
    /// </summary>
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

    /// <summary>
    /// Read the neural network layer's memory (weights and biases) to the specified BinaryReader
    /// </summary>
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
}

public class  TrainingAndTestingNeuronLayer : NeuronLayer
{
    // Training data structures
    private readonly double[] inputs;       // Ni
    private readonly double[][] gradientsW; // No x Ni
    private readonly double[][] deltaW;     // No x Ni
    private readonly double[][] prevDeltaW; // No x Ni (for momentum calculation)
    private readonly double[] gradientsB;   // No
    private readonly double[] deltaB;       // No
    private readonly double[] prevDeltaB;   // No (for momentum calculation)
    private readonly double[] derivatives;  // No
    private readonly double[] signals;      // No
    private readonly double[] errorsOut;    // Ni (errors passed to the previous layer)

    // The random number generator to be used for the layer's weights and biases initialization
    private readonly Random rnd;

    /// <summary>
    /// Constructor for the NeuronLayer class for Training and Testing
    /// </summary>
    public TrainingAndTestingNeuronLayer(
        int nbrInputs,
        int nbrOutputs,
        IActivationFunction activationFunction,
        Random rnd,
        double initialWeightRange) : base(
            nbrInputs,
            nbrOutputs,
            activationFunction)
    {
        this.rnd = rnd;

        inputs = new double[nbrInputs];
        gradientsW = new double[nbrOutputs][];
        deltaW = new double[nbrOutputs][];
        prevDeltaW = new double[nbrOutputs][];
        for (int k = 0; k < nbrOutputs; k++)
        {
            gradientsW[k] = new double[nbrInputs];
            deltaW[k] = new double[nbrInputs];
            prevDeltaW[k] = new double[nbrInputs];
        }
        gradientsB = new double[nbrOutputs];
        deltaB = new double[nbrOutputs];
        prevDeltaB = new double[nbrOutputs];
        derivatives = new double[nbrOutputs];
        signals = new double[nbrOutputs];
        errorsOut = new double[nbrInputs];

        Reset(initialWeightRange);
    }

    /// <summary>
    /// Delegates to the base compute logic after validation and pre-compute hook
    /// </summary>
    public override double[] ComputeOutputs(
        double[] inputs)
    {
        return base.ComputeOutputsBase(inputs);
    }

    /// <summary>
    /// Hook that is called before the base compute logic to allow derived classes to perform any necessary processing before the outputs are computed
    /// </summary>
    protected override void OnBeforeCompute(double[] inputs)
    {
        // Save the inputs for updating the weights (during training only)
        Array.Copy(inputs, this.inputs, NbrInputs);
    }

    /// <summary>
    /// Computes the errors that are propagated from this layer back to the previous layer
    /// Errors In -> Errors Out
    /// </summary>
    public double[] ComputeErrors(
        double[] errorsIn)
    {
        if (errorsIn.Length != NbrOutputs)
        {
            throw new ArgumentException(
                $"Expected {NbrOutputs} errors but received {errorsIn.Length}.",
                nameof(errorsIn));
        }

        // Cache the arrays in local variables for faster access in the loops below
        var signalsLocal = signals;
        var errorsOutLocal = errorsOut;
        var weightsLocal = weights;

        // Compute the layer's signal array
        for (int k = 0; k < NbrOutputs; k++)
        {
            derivatives[k] = ActivationFunction.Derivative(nets[k], outputs[k]);

            signalsLocal[k] = errorsIn[k] * derivatives[k];
        }

        // Compute the errors propagated to the previous layer
        for (int j = 0; j < NbrInputs; j++)
        {
            errorsOutLocal[j] = 0.0;

            for (int k = 0; k < NbrOutputs; k++)
            {
                var weightsK = weightsLocal[k];
                errorsOutLocal[j] += weightsK[j] * signalsLocal[k];
            }
        }

        return errorsOutLocal;
    }

    /// <summary>
    /// Updates the layers' weight matrix and bias array based on the learning rate and momentum
    /// </summary>
    public void Update(
        double rate,
        double momentum)
    {
        var inputsLocal = inputs;
        var signalsLocal = signals;
        var weightsLocal = weights;
        var gradientsWLocal = gradientsW;
        var deltaWLocal = deltaW;
        var prevDeltaWLocal = prevDeltaW;

        // Update the layers' weight matrix and bias array
        for (int k = 0; k < NbrOutputs; k++)
        {
            // Update the neuron's bias
            gradientsB[k] = signalsLocal[k];
            deltaB[k] = rate * gradientsB[k];
            biases[k] += deltaB[k] + (prevDeltaB[k] * momentum);
            prevDeltaB[k] = deltaB[k];

            // Update the neuron's weight array
            var weightsK = weightsLocal[k];
            var gradientsWK = gradientsWLocal[k];
            var deltaWK = deltaWLocal[k];
            var prevDeltaWK = prevDeltaWLocal[k];
            for (int j = 0; j < NbrInputs; j++)
            {
                gradientsWK[j] = signalsLocal[k] * inputsLocal[j];
                deltaWK[j] = rate * gradientsWK[j];
                weightsK[j] += deltaWK[j] + (prevDeltaWK[j] * momentum);
                prevDeltaWK[j] = deltaWK[j];
            }
        }
    }

    /// <summary>
    /// Initializes the layer's weights and biases based on the specified value +/- range
    /// </summary>
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
                weights[k][j] = rnd.NextDouble(minValue, valueRange);
            }
        }
    }
}

public class TestingOnlyNeuronLayer : NeuronLayer
{
    /// <summary>
    /// Constructor for the NeuronLayer class for Testing Only
    /// </summary>
    public TestingOnlyNeuronLayer(
        int nbrInputs,
        int nbrOutputs,
        IActivationFunction activationFunction) : base(
            nbrInputs,
            nbrOutputs,
            activationFunction)
    {
    }

    /// <summary>
    /// Delegates to the base compute logic
    /// </summary>
    public override double[] ComputeOutputs(double[] inputs)
    {
        return base.ComputeOutputsBase(inputs);
    }
}
