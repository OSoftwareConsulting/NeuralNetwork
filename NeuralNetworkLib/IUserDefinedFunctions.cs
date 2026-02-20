/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace NeuralNetworkLib;

// Declares the methods called by the Neural Network Executable & Library
// User-Defined Test Functions must implement this interface and specify it in the Test Setup File
public interface IUserDefinedFunctions
{
    // Initialize data structures
    void Configure(int nbrInputs, int nbrOutputs);

    // Error Difference Function
    void ComputeErrors(double[] targets, double[] outputs, double[] errors);

    // Called after each testing sample to process the result
    void ProcessTestResult(int index, double[] inputs, double[] targets, double[] outputs, bool debug);

    // Compute a Score summarizing the Test Results
    double ComputeScore();

    // Print-out a summary of the test results
    void SummarizeTestResults(bool debug);
}
