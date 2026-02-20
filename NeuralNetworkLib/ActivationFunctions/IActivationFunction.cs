/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace NeuralNetworkLib.ActivationFunctions;

public interface IActivationFunction
{
    /// <summary>
    /// Gets or sets the activation function's type name.
    /// </summary>
    string TypeName { get; set;  }

    /// <summary>
    /// Computes f(x) for a single input before any vector-level post-processing.
    /// </summary>
    double Compute(double x);

    /// <summary>
    /// Computes the derivative at x. The <paramref name="fofx"/> parameter is the element value
    /// after <see cref="PostProcess"/> (if applied), otherwise it is just f(x).
    /// </summary>
    double Derivative(double x, double fofx);

    /// <summary>
    /// Optional vector-level post-processing (e.g., softmax normalization). Mutates <paramref name="ary"/> in place.
    /// </summary>
    void PostProcess(double[] ary);
}
