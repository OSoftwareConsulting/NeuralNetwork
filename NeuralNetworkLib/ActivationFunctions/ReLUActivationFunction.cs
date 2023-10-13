﻿/*
 * Copyright ©
 * 2023
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace NeuralNetworkLib.ActivationFunctions
{
    public class ReLUActivationFunction : IActivationFunction
    {
        public double Compute(double x)
        {
            return x < 0.0 ? 0.0 : x;
        }

        public double Derivative(double x, double fofx)
        {
            return x < 0.0 ? 0.0 : 1.0;
        }

        public void PostProcess(double[] ary)
        {
        }
    }
}
