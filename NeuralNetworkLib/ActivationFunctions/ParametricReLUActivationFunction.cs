/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace NeuralNetworkLib.ActivationFunctions;

public class ParametricReLUActivationFunction : IActivationFunction
{
    public string TypeName { get; set; }

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
