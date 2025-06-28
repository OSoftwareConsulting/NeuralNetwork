/*
 * Copyright ©
 * 2025
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace NeuralNetworkLib.ActivationFunctions;

public interface IActivationFunction
{
    // The Type Name
    string TypeName { get; set;  }

    // f(x)
    double Compute(double x);

    // f'(x) or f'(f(x))
    double Derivative(double x, double fofx);

    // Typically normalization: e.g., Soft Max = f(x[i]) / sum(f(x[i]))
    void PostProcess(double[] ary);
}
