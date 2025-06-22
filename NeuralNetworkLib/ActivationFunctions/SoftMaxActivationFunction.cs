/*
 * Copyright ©
 * 2025
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace NeuralNetworkLib.ActivationFunctions;

public class SoftMaxActivationFunction : IActivationFunction
{
    public double Compute(double x)
    {
        return Math.Exp(x);
    }

    public double Derivative(double x, double fofx)
    {
        return (1.0 - fofx) * fofx;
    }

    public void PostProcess(double[] fofxary)
    {
        double sum = 0.0;
        foreach (var fofx in fofxary)
        {
            sum += fofx;
        }
        for (int i = 0; i < fofxary.Length; i++)
        {
            fofxary[i] /= sum;
        }
    }
}
