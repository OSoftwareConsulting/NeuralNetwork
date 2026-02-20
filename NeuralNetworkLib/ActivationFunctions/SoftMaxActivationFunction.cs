/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace NeuralNetworkLib.ActivationFunctions;

public class SoftMaxActivationFunction : IActivationFunction
{
    public string TypeName { get; set; }

    /// <summary>
    /// Returns the raw input value; softmax normalization is applied in <see cref="PostProcess"/> for numerical stability.
    /// </summary>
    public double Compute(double x)
    {
        // Return the raw input; softmax is applied in PostProcess for numerical stability.
        return x;
    }

    /// <summary>
    /// Returns the derivative of the softmax function at the given point.
    /// </summary>
    public double Derivative(double x, double fofx)
    {
        return (1.0 - fofx) * fofx;
    }

    /// <summary>
    /// Applies numerically stable softmax normalization to the vector in place.
    /// </summary>
    public void PostProcess(double[] fofxary)
    {
        if (fofxary.Length == 0)
        {
            return;
        }

        double max = fofxary[0];
        for (int i = 1; i < fofxary.Length; i++)
        {
            if (fofxary[i] > max)
            {
                max = fofxary[i];
            }
        }

        double sum = 0.0;
        for (int i = 0; i < fofxary.Length; i++)
        {
            double exp = Math.Exp(fofxary[i] - max);
            fofxary[i] = exp;
            sum += exp;
        }

        for (int i = 0; i < fofxary.Length; i++)
        {
            fofxary[i] /= sum;
        }
    }
}
