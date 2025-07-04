﻿/*
 * Copyright ©
 * 2025
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace NeuralNetworkLib.ActivationFunctions;

public class SigmoidActivationFunction : IActivationFunction
{
    public string TypeName { get; set; }

    public double Compute(double x)
    {
        return 1.0 / (1.0 + Math.Exp(-x));
    }

    public double Derivative(double x, double fofx)
    {
        return fofx * (1.0 - fofx);
    }

    public void PostProcess(double[] ary)
    {
    }
}
