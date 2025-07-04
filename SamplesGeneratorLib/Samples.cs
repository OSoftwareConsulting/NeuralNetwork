﻿/*
 * Copyright ©
 * 2025
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using UtilitiesLib;

namespace SamplesGeneratorLib;

// Represents the Training and Testing Sample Sets
// A sample consists of inputs and targets
public class Samples
{
    // Training Sample Set
    public double[][] TrainingInputs { get; }
    public double[][] TrainingTargets { get; }

    // Testing Sample Set
    public double[][] TestingInputs { get; }
    public double[][] TestingTargets { get; }

    public int NbrOutputs { get; }
    public int NbrInputs { get; }

    // Constructor that generates the training and testing samples based on the specified training fraction from the records
    // If a random number generator is specified, the order of the records is randomized before separating the records into the training and testing sample sets
    internal Samples(
        double trainingFraction,
        bool normalizeInputs,
        double[][] records,
        int nbrValuesPerRecord,
        int nbrOutputs,
        Random rnd = null)
    {
        if ((trainingFraction < 0.0) || (trainingFraction > 1.0))
        {
            throw new ArgumentException($"The specified Training Fraction {trainingFraction} is invalid");
        }
        if ((nbrOutputs < 1) || (nbrOutputs >= nbrValuesPerRecord))
        {
            throw new ArgumentException($"The specified Number of Outputs {nbrOutputs} is invalid");
        }

        NbrOutputs = nbrOutputs;
        NbrInputs = nbrValuesPerRecord - nbrOutputs;

        int nbrRecords = records.Count();
        int nbrTrainingSamples = (int)(nbrRecords * trainingFraction);
        int nbrTestingSamples = nbrRecords - nbrTrainingSamples;

        TrainingInputs = new double[nbrTrainingSamples][];
        TrainingTargets = new double[nbrTrainingSamples][];
        TestingInputs = new double[nbrTestingSamples][];
        TestingTargets = new double[nbrTestingSamples][];

        // Normalize the records' inputs
        if (normalizeInputs)
        {
            NormalizeInputs(records);
        }

        // Generate the sequence by which the records are added to the training and testing sample sets
        // If a random number generator is passed in, the samples are added in random order
        // Otherwise, they are added in the order in which the records were generated
        var sequence = Utilities.GenerateSequence(nbrRecords, rnd);

        int n = 0;

        // Add records to the training sample set
        for (int i = 0; i < nbrTrainingSamples; i++, n++)
        {
            int nn = sequence[n];

            TrainingInputs[i] = new double[NbrInputs];
            TrainingTargets[i] = new double[NbrOutputs];

            int m = 0;

            // First N record values are training inputs
            for (int j = 0; j < NbrInputs; j++, m++)
            {
                TrainingInputs[i][j] = records[nn][m];
            }

            // Next M record values are training targets
            for (int j = 0; j < NbrOutputs; j++, m++)
            {
                TrainingTargets[i][j] = records[nn][m];
            }
        }

        // Add records to the testing sample set
        for (int i = 0; i < nbrTestingSamples; i++, n++)
        {
            int nn = sequence[n];

            TestingInputs[i] = new double[NbrInputs];
            TestingTargets[i] = new double[NbrOutputs];

            int m = 0;

            // First N record values are testing inputs
            for (int j = 0; j < NbrInputs; j++, m++)
            {
                TestingInputs[i][j] = records[nn][m];

            }

            // Next M record values are testing targets
            for (int j = 0; j < NbrOutputs; j++, m++)
            {
                TestingTargets[i][j] = records[nn][m];
            }
        }
    }

    // Normalize the records' input values by the first record's inputs' order of magnitudes
    private void NormalizeInputs(
        double[][] records)
    {
        double[] inputFactors = new double[NbrInputs];
        double[] inputs = records[0];

        for (int j = 0; j < NbrInputs; j++)
        {
            inputFactors[j] = 1d / Math.Pow(10d, Utilities.OrderOfMagnitude(inputs[j]));
        }

        int nbrRecords = records.Count();
        for (int n = 0; n < nbrRecords; n++)
        {
            inputs = records[n];

            for (int j = 0; j < NbrInputs; j++)
            {
                inputs[j] *= inputFactors[j];
            }
        }
    }
}
