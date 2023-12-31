﻿/*
 * Copyright ©
 * 2023
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace NeuralNetworkLib.ActivationFunctions
{
    public class TanhActivationFunction : IActivationFunction
    {
        public double Compute(double x)
        {
            if (x < -20.0)
            {
                return -1.0;
            }
            else if (x > 20.0)
            {
                return 1.0;
            }

            return Math.Tanh(x);
        }

        public double Derivative(double x, double fofx)
        {
            return 1.0 - (fofx * fofx);
        }

        public void PostProcess(double[] ary)
        {
        }
    }
}
