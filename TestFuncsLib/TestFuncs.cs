/*
 * Copyright ©
 * 2023
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using NeuralNetworkLib;
using SamplesGeneratorLib;
using UtilitiesLib;

// This assembly is loaded at runtime and specified in the test setup JSON files
namespace TestFuncsLib
{
    // Example Samples Generator Function that computes Math.Sin
    public class MathSin : ISamplesGeneratorFunction
    {
        public void Compute(double[] x, double[] y)
        {
            y[0] = Math.Sin(x[0]);
        }
    }

    // An implementation of the INeuralNetworkFuncs interface that is used for Classification Prediction Problems using Class Binary Vectors
    // The neural network will produce outputs, and the index with the maximum output value corresponds to the class
    public class IndexOfMaxMatches : IUserDefinedFunctions
    {
        private int nbrTests;
        private int nbrInputs;
        private int nbrOutputs;
        private int nbrPassed;
        private double avgPassed;

        public IndexOfMaxMatches()
        {
            nbrTests = 0;
            nbrInputs = 0;
            nbrOutputs = 0;
            nbrPassed = 0;
            avgPassed = 0.0;
        }

        // Initialize data structures
        public void Configure(int nbrInputs, int nbrOutputs)
        {
            this.nbrInputs = nbrInputs;
            this.nbrOutputs = nbrOutputs;
        }

        // Error Difference Function
        public void ComputeErrors(double[] targets, double[] outputs, double[] errors)
        {
            for (int k = 0; k < nbrOutputs; k++)
            {
                errors[k] = targets[k] - outputs[k];
            }
        }

        // Called after each testing sample to process the result
        public void ProcessTestResult(int index, double[] inputs, double[] targets, double[] outputs)
        {
            nbrTests++;

            // Compute the Target & Output classes (Max. Index)
            int targetsMaxIdx = IndexOfMax(targets);
            int outputsMaxIdx = IndexOfMax(outputs);

            bool testPassed = targetsMaxIdx == outputsMaxIdx;
            if (testPassed)
            {
                nbrPassed++;
            }

            avgPassed = nbrPassed / (double)nbrTests;

            string pfStr = testPassed ? "Pass" : "Fail";

            Console.Write($"{index.ToString().PadLeft(4)}, ");
            Utilities.PrintValues(inputs);
            Console.WriteLine($", {targetsMaxIdx}, {outputsMaxIdx}, {pfStr}, {avgPassed.ToString("P1")}");
        }

        // Print-out a summary of the test results
        public void SummarizeTestResults()
        {
            Console.WriteLine($"Average Tests Passed: {avgPassed.ToString("P1")}");
            Console.ReadLine();
        }

        private static int IndexOfMax(double[] outputs)
        {
            int len = outputs.Length;
            int maxIdx = 0;
            double max = outputs[0];

            for (int n = 1; n < len; n++)
            {
                if (outputs[n] > max)
                {
                    max = outputs[n];
                    maxIdx = n;
                }
            }

            return maxIdx;
        }
    }

    // An implementation of the INeuralNetworkFuncs interface that is used for Regression Prediction Problems using Absolute of Errors Distance Metric
    public class AbsErrors : IUserDefinedFunctions
    {
        private int nbrTests;
        private int nbrInputs;
        private int nbrOutputs;
        private double[] absErrors;
        private double[] sumAbsErrors;
        private double[] avgAbsErrors;

        public AbsErrors()
        {
            nbrTests = 0;
            nbrInputs = 0;
            nbrOutputs = 0;
        }

        // Initialize data structures
        public void Configure(int nbrInputs, int nbrOutputs)
        {
            this.nbrInputs = nbrInputs;
            this.nbrOutputs = nbrOutputs;

            absErrors = new double[nbrOutputs];
            sumAbsErrors = new double[nbrOutputs];
            avgAbsErrors = new double[nbrOutputs];
        }

        // Error Difference Function
        public void ComputeErrors(double[] targets, double[] outputs, double[] errors)
        {
            for (int k = 0; k < nbrOutputs; k++)
            {
                errors[k] = targets[k] - outputs[k];
            }
        }

        // Called after each testing sample to process the result
        public void ProcessTestResult(int index, double[] inputs, double[] targets, double[] outputs)
        {
            nbrTests++;

            for (int i = 0; i < nbrOutputs; i++)
            {
                absErrors[i] = Math.Abs(targets[i] - outputs[i]);
                sumAbsErrors[i] += absErrors[i];
                avgAbsErrors[i] = sumAbsErrors[i] / nbrTests;
            }

            Console.Write($"{index.ToString().PadLeft(4)}, ");
            Utilities.PrintValues(inputs);
            Console.Write(", ");
            Utilities.PrintValues(targets);
            Console.Write(", ");
            Utilities.PrintValues(outputs);
            Console.Write(", ");
            Utilities.PrintValues(absErrors);
            Console.Write(", ");
            Utilities.PrintValues(avgAbsErrors, addEOL: true);
        }

        // Print-out a summary of the test results
        public void SummarizeTestResults()
        {
            Console.Write("Average Abs. Error(s): ");
            Utilities.PrintValues(avgAbsErrors, addEOL: true);
            Console.ReadLine();
        }
    }
}