/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using UtilitiesLib;

namespace SamplesGeneratorLib;

public interface ISamplesGeneratorFunction
{
    void Compute(double[] x, double[] y);
}

public class FunctionSamplesGenerator : SamplesGenerator
{
    // Value range for an input
    public struct ValueRange
    {
        public double MinValue { get; }
        public double MaxValue { get; }
        public double Range { get; }

        public ValueRange(double minValue, double maxValue)
        {
            if (maxValue < minValue)
            {
                throw new InvalidOperationException();
            }

            MinValue = minValue;
            MaxValue = maxValue;
            Range = maxValue - minValue;
        }
    }

    // Private constructor (calls the base class constructor)
    private FunctionSamplesGenerator(
        int nbrRecords,
        int nbrInputs,
        int nbrOutputs) :
        base(
            nbrOutputs: nbrOutputs)
    {
        nbrValuesPerRecord = nbrInputs + nbrOutputs;
        records = new double[nbrRecords][];
        for (int n = 0; n < nbrRecords; n++)
        {
            records[n] = new double[nbrValuesPerRecord];
        }
    }

    // Entry point to generate the records and training and testing sample sets
    public static Samples GetSamples(
        int nbrOutputs,
        double trainingFraction,
        bool normalizeInputs,
        ISamplesGeneratorFunction dataGeneratorFunction,
        int nbrRecords,
        ValueRange[] valueRanges,   // ranges for inputs, defines the number of inputs
        Random rnd)
    {
        var fdg = new FunctionSamplesGenerator(
            nbrRecords,
            valueRanges.Length,
            nbrOutputs);

        // Generate the records by randomly setting the input values
        fdg.GenerateRecords(
            dataGeneratorFunction,
            valueRanges,
            rnd);

        // The samples do not have to be randomized since the inputs are generated randomly
        return fdg.GetSamples(
            trainingFraction,
            normalizeInputs,
            rnd: null);
    }

    private void GenerateRecords(
        ISamplesGeneratorFunction dataGeneratorFunction,
        ValueRange[] valueRanges,
        Random rnd)
    {
        int nbrRecords = records.GetLength(0);
        int nbrInputs = valueRanges.Length;

        double[] inputs = new double[nbrInputs];
        double[] outputs = new double[nbrOutputs];

        for (int n = 0; n < nbrRecords; n++)
        {
            // Randomize the inputs based on the specified input value ranges
            randomizeInputs(valueRanges, inputs, rnd);

            // Call the user-define samples generator function (ISamplesGeneratorFunction) to compute the functions' output values
            dataGeneratorFunction.Compute(inputs, outputs);

            // Copy the inputs and outputs to the record
            copyInputsAndOutputs(records[n], inputs, outputs);
        }
    }

    // Randomizes the inputs based on the specified input value ranges
    private void randomizeInputs(
        ValueRange[] valueRanges,
        double[] inputs,
        Random rnd)
    {
        int nbrInputs = inputs.Length;
        for (int i = 0; i < nbrInputs; i++)
        {
            inputs[i] = rnd.NextDouble(valueRanges[i].MinValue, valueRanges[i].Range);
        }
    }

    // Copies the input and output arrays to the record array
    private void copyInputsAndOutputs(
        double[] record,
        double[] inputs,
        double[] outputs)
    {
        int nbrInputs = inputs.Length;
        int n = 0;
        for (int i = 0; i < nbrInputs; i++, n++)
        {
            record[n] = inputs[i];
        }
        for (int i = 0; i < nbrOutputs; i++, n++)
        {
            record[n] = outputs[i];
        }
    }
}
