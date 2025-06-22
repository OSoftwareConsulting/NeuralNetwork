/*
 * Copyright ©
 * 2025
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace NeuralNetworkLib.ActivationFunctions;

public class LinearActivationFunction : IActivationFunction
{
    private readonly double m;
    private readonly double b;

    public LinearActivationFunction()
    {
        m = 1.0;
        b = 0.0;
    }

    public LinearActivationFunction(
        double _m = 1.0,
        double _b = 0.0)
    {
        m = _m;
        b = _b;
    }

    public double Compute(double x)
    {
        return (m * x) + b;
    }

    public double Derivative(double x, double fofx)
    {
        return m;
    }

    public void PostProcess(double[] ary)
    {
    }
}
