/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace NeuralNetworkLib.ActivationFunctions;

public class SigmoidActivationFunction : IActivationFunction
{
    public string TypeName { get; set; }

    public double Compute(double x)
    {
        if (x >= 0.0)
        {
            return 1.0 / (1.0 + Math.Exp(-x));
        }

        double expX = Math.Exp(x);
        return expX / (1.0 + expX);
    }

    public double Derivative(double x, double fofx)
    {
        return fofx * (1.0 - fofx);
    }

    public void PostProcess(double[] ary)
    {
    }
}
