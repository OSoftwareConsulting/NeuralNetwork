/*
 * Copyright ©
 * 2023
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace NeuralNetworkLib.ActivationFunctions
{
    public interface IActivationFunction
    {
        // f(x)
        double Compute(double x);

        // f'(x) or f'(f(x))
        double Derivative(double x, double fofx);

        // Typically normalization: e.g., Soft Max = f(x[i]) / sum(f(x[i]))
        void PostProcess(double[] ary);
    }
}
