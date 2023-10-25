using NeuralNetworkLib.ActivationFunctions;

namespace NeuralNetworkLib
{
    // Declares the structure of a Neuron Layer used to create the Neural Network
    public class NeuronLayerConfig
    {
        // The number of neurons (outputs) in the layer
        public int NbrOutputs { get; }

        // The Activation Function to use in this layer
        public IActivationFunction ActivationFunction { get; }

        // The range for the initial values of the layer's neuron weights and bias
        public double InitialWeightRange { get; }

        public NeuronLayerConfig(
            int nbrOutputs,
            IActivationFunction activationFunction,
            double initialWeightRange)
        {
            NbrOutputs = nbrOutputs;
            ActivationFunction = activationFunction;
            InitialWeightRange = initialWeightRange;
        }
    }
}
