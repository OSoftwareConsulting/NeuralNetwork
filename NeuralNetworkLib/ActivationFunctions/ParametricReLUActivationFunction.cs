/*
 * Copyright ©
 * 2025
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace NeuralNetworkLib.ActivationFunctions;

public class ParametricReLUActivationFunction : IActivationFunction
{
    private readonly double parameter;

    public ParametricReLUActivationFunction(double parameter)
    {
        this.parameter = parameter;
    }

    public double Compute(double x)
    {
        return x < 0.0 ? (parameter * x) : x;
    }

    public double Derivative(double x, double fofx)
    {
        return x < 0.0 ? parameter : 1.0;
    }

    public void PostProcess(double[] ary)
    {
    }
}
