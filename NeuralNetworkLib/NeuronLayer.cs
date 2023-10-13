/*
 * Copyright ©
 * 2023
 * Osella Ventures, LLC
 * All Rights Reserved
*/

//#define PRINTMSGS

using NeuralNetworkLib.ActivationFunctions;
using UtilitiesLib;

namespace NeuralNetworkLib
{
    public class NeuronLayer
    {
        private readonly int index;

        private readonly int nbrInputs;         // Ni , indexed by j
        private readonly int nbrOutputs;        // No , indexed by k
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

        private readonly IActivationFunction af;
        private readonly Random rnd;

        public NeuronLayer(
            int index,
            int nbrInputs,
            int nbrOutputs,
            IActivationFunction af,
            Random rnd,
            double initialWeightRange)
        {
            this.index = index;
            this.nbrInputs = nbrInputs;
            this.nbrOutputs = nbrOutputs;
            this.af = af;
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

        public double[] ComputeOutputs(
            double[] inputs)
        {
            // Save inputs for updating the weights
            for (int j = 0; j < nbrInputs; j++)
            {
                this.inputs[j] = inputs[j];
            }

            for (int k = 0; k < nbrOutputs; k++)
            {
                nets[k] = biases[k];

                for (int j = 0; j < nbrInputs; j++)
                {
                    nets[k] += weights[k][j] * inputs[j];
                }

                outputs[k] = af.Compute(nets[k]);
            }

            af.PostProcess(outputs);

#if PRINTMSGS
            Console.WriteLine($"Computed Outputs for Layer {index}:");
            Utilities.PrintArray($"Inputs:", inputs);
            Utilities.PrintArray($"Biases:", biases);
            Utilities.PrintMatrix($"Weights:", weights);
            Utilities.PrintArray($"Nets:", nets);
            Utilities.PrintArray($"Outputs:", outputs);
            Console.WriteLine();
#endif

            return outputs;
        }

        public double[] ComputeErrors(
            double[] errorsIn)
        {
            for (int k = 0; k < nbrOutputs; k++)
            {
                derivatives[k] = af.Derivative(nets[k], outputs[k]);

                signals[k] = errorsIn[k] * derivatives[k];
            }

            for (int j = 0; j < nbrInputs; j++)
            {
                errorsOut[j] = 0.0;

                for (int k = 0; k < nbrOutputs; k++)
                {
                    errorsOut[j] += weightsT[j][k] * signals[k];
                }
            }

#if PRINTMSGS
            Console.WriteLine($"Computed Errors for Layer {index}:");
            Utilities.PrintArray($"ErrorsIn:", errorsIn);
            Utilities.PrintArray($"Derivatives:", derivatives);
            Utilities.PrintArray($"Signals:", signals);
            Utilities.PrintMatrix($"WeightsT:", weightsT);
            Utilities.PrintArray($"ErrorsOut:", errorsOut);
            Console.WriteLine();
#endif

            return errorsOut;
        }

        public void Update(
            double rate,
            double momentum)
        {
            for (int k = 0; k < nbrOutputs; k++)
            {
                gradientsB[k] = signals[k];
                deltaB[k] = rate * gradientsB[k];
                biases[k] += deltaB[k] + (prevDeltaB[k] * momentum);
                prevDeltaB[k] = deltaB[k];

                for (int j = 0; j < nbrInputs; j++)
                {
                    gradientsW[k][j] = signals[k] * inputs[j];
                    deltaW[k][j] = rate * gradientsW[k][j];
                    weights[k][j] += deltaW[k][j] + (prevDeltaW[k][j] * momentum);
                    weightsT[j][k] = weights[k][j];
                    prevDeltaW[k][j] = deltaW[k][j];
                }
            }

#if PRINTMSGS
            Console.WriteLine($"Updated Layer {index}:");
            Utilities.PrintArray($"Biases:", biases);
            Utilities.PrintMatrix($"Weights:", weights);
            Utilities.PrintMatrix($"WeightsT:", weightsT);
            Console.WriteLine();
#endif
        }

        private void Reset(
            double initialWeightRange)
        {
            double minValue = -initialWeightRange;
            double valueRange = 2 * initialWeightRange;

            for (int k = 0; k < nbrOutputs; k++)
            {
                biases[k] = rnd.NextDouble(minValue, valueRange);

                for (int j = 0; j < nbrInputs; j++)
                {
                    weightsT[j][k] = weights[k][j] = rnd.NextDouble(minValue, valueRange);
                }
            }

#if PRINTMSGS
            Console.WriteLine($"Reset Layer {index}:");
            Utilities.PrintArray($"Biases:", biases);
            Utilities.PrintMatrix($"Weights:", weights);
            Console.WriteLine();
#endif
        }

        public void Print()
        {
            Console.WriteLine($"Layer {index}:");
            Utilities.PrintArray($"Biases:", biases);
            Utilities.PrintMatrix($"Weights:", weights);
            Console.WriteLine();
        }
    }
}
