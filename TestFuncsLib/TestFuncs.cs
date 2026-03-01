/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using NeuralNetworkLib;
using SamplesGeneratorLib;
using UtilitiesLib;

// This assembly is loaded at runtime and specified in the test setup JSON files
namespace TestFuncsLib;

// An implementation of the IUserDefinedFunctions interface that is used for Classification Prediction Problems using Class Binary Vectors
// The neural network will produce outputs, and the index with the maximum output value corresponds to the class
public class IndexOfMaxMatches : IUserDefinedFunctions
{
    private int nbrOutputs;
    private int nbrTests;
    private int nbrPassed;
    private double avgPassed;

    public IndexOfMaxMatches()
    {
        nbrOutputs = 0;
        nbrTests = 0;
        nbrPassed = 0;
        avgPassed = 0.0;
    }

    // Initialize data structures
    public void Configure(
        int nbrInputs,
        int nbrOutputs)
    {
        this.nbrOutputs = nbrOutputs;
    }

    // Error Difference Function
    public void ComputeErrors(
        double[] targets,
        double[] outputs,
        double[] errors)
    {
        for (int k = 0; k < nbrOutputs; k++)
        {
            errors[k] = targets[k] - outputs[k];
        }
    }

    // Called after each testing sample to process the result
    public void ProcessTestResult(
        int index,
        double[] inputs,
        double[] targets,
        double[] outputs,
        bool debug)
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

        if (debug)
        {
            string pfStr = testPassed ? "Pass" : "Fail";
            Console.Write($"{index.ToString().PadLeft(4)}, ");
            Utilities.PrintValues(inputs);
            Console.WriteLine($", {targetsMaxIdx}, {outputsMaxIdx}, {pfStr}, {avgPassed.ToString("P1")}");
        }
    }

    // Compute a Score summarizing the Test Results
    public double ComputeScore()
    {
        return avgPassed;
    }

    // Print-out a summary of the test results
    public void SummarizeTestResults(bool debug)
    {
        if (debug)
        {
            Console.WriteLine($"Average Tests Passed: {avgPassed.ToString("P1")}");
        }
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

// Example class that implements the ISamplesGeneratorFunction interface that computes Math.Sin
public class MathSin : ISamplesGeneratorFunction
{
    public void Compute(double[] x, double[] y)
    {
        y[0] = Math.Sin(x[0]);
    }
}

// An implementation of the IUserDefinedFunctions interface that is used for Regression Prediction Problems using Absolute of Errors Distance Metric
public class AbsErrors : IUserDefinedFunctions
{
    private int nbrOutputs;
    private int nbrTests;
    private double[] absErrors;
    private double[] sumAbsErrors;
    private double[] avgAbsErrors;

    public AbsErrors()
    {
        nbrOutputs = 0;
        nbrTests = 0;
    }

    // Initialize data structures
    public void Configure(
        int nbrInputs,
        int nbrOutputs)
    {
        this.nbrOutputs = nbrOutputs;

        absErrors = new double[nbrOutputs];
        sumAbsErrors = new double[nbrOutputs];
        avgAbsErrors = new double[nbrOutputs];
    }

    // Error Difference Function
    public void ComputeErrors(
        double[] targets,
        double[] outputs,
        double[] errors)
    {
        for (int k = 0; k < nbrOutputs; k++)
        {
            errors[k] = targets[k] - outputs[k];
        }
    }

    // Called after each testing sample to process the result
    public void ProcessTestResult(
        int index,
        double[] inputs,
        double[] targets,
        double[] outputs,
        bool debug)
    {
        nbrTests++;

        for (int i = 0; i < nbrOutputs; i++)
        {
            absErrors[i] = Math.Abs(targets[i] - outputs[i]);
            sumAbsErrors[i] += absErrors[i];
            avgAbsErrors[i] = sumAbsErrors[i] / nbrTests;
        }

        if (debug)
        {
            Console.Write($"{index,4}, ");
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
    }

    // Compute a Score summarizing the Test Results
    public double ComputeScore()
    {
        double avgAbsError = 0.0;

        for (int i = 0; i < nbrOutputs; i++)
        {
            avgAbsError += avgAbsErrors[i];
        }

        avgAbsError /= nbrOutputs;

        return avgAbsError;
    }

    // Print-out a summary of the test results
    public void SummarizeTestResults(bool debug)
    {
        if (debug)
        {
            Console.Write("Average Abs. Error(s): ");
            Utilities.PrintValues(avgAbsErrors, addEOL: true);
        }
    }
}
