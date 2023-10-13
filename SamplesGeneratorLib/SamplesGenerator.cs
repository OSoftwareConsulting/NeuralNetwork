/*
 * Copyright ©
 * 2023
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace SamplesGeneratorLib
{
    public abstract class SamplesGenerator
    {
        protected int nbrValuesPerRecord;   // number of inputs (defined or calculated) + nbrOutputs
        protected double[][] records;       // number of records x nbrValuesPerRecord 

        protected readonly int nbrOutputs;

        protected SamplesGenerator(
            int nbrOutputs)
        {
            if (nbrOutputs < 1)
            {
                throw new InvalidOperationException();
            }

            nbrValuesPerRecord = 0;

            this.nbrOutputs = nbrOutputs;
        }

        protected Samples GetSamples(
            double trainingFraction,
            Random rnd = null)
        {
            return new Samples(
                trainingFraction,
                records,
                nbrValuesPerRecord,
                nbrOutputs,
                rnd);
        }
    }
}
