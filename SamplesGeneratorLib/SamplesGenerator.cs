/*
 * Copyright ©
 * 2025
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace SamplesGeneratorLib;

// Abstract base class for Samples Generators
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
        bool normalizeInputs,
        Random rnd = null)
    {
        return new Samples(
            trainingFraction,
            normalizeInputs,
            records,
            nbrValuesPerRecord,
            nbrOutputs,
            rnd);
    }
}
