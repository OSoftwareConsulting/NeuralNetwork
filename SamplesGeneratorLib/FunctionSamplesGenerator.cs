/*
 * Copyright ©
 * 2023
 * Osella Ventures, LLC
 * All Rights Reserved
*/

//#define PRINTMSGS

using UtilitiesLib;

namespace SamplesGeneratorLib
{
    public interface ISamplesGeneratorFunction
    {
        void Compute(double[] x, double[] y);
    }

    public class FunctionSamplesGenerator : SamplesGenerator
    {
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

        public static Samples GetSamples(
            int nbrOutputs,
            double trainingFraction,
            ISamplesGeneratorFunction dataGeneratorFunction,
            int nbrRecords,
            ValueRange[] valueRanges,   // ranges for inputs, defines the number of inputs
            Random rnd)
        {
            var fdg = new FunctionSamplesGenerator(
                nbrRecords,
                valueRanges.Length,
                nbrOutputs);

            // The records are created with random input values
            fdg.DoGenerateRecords(
                dataGeneratorFunction,
                valueRanges,
                rnd);

            // So the samples do not have to be randomized
            return fdg.GetSamples(
                trainingFraction,
                rnd: null);
        }

        private void DoGenerateRecords(
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
                randomizeInputs(valueRanges, inputs, rnd);

                dataGeneratorFunction.Compute(inputs, outputs);

                copyInputsAndOutputs(records[n], inputs, outputs);

#if PRINTMSGS
                Utilities.PrintValues(records[n]);
                Console.WriteLine();
#endif
            }
        }

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
}
