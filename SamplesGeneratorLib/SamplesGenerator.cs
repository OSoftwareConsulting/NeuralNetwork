/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace SamplesGeneratorLib;

// Abstract base class for Samples Generators
public abstract class SamplesGenerator
{
    protected int NbrValuesPerRecord { get; set; }  // number of inputs (defined or calculated) + nbrOutputs
    protected double[][] Records { get; set; }      // number of records x nbrValuesPerRecord 
    protected int NbrOutputs { get; }

    protected SamplesGenerator(
        int nbrOutputs)
    {
        if (nbrOutputs < 1)
        {
            throw new InvalidOperationException();
        }

        NbrValuesPerRecord = 0;
        NbrOutputs = nbrOutputs;
    }

    protected Samples GetSamples(
        double trainingFraction,
        bool normalizeInputs,
        Random rnd = null)
    {
        return new Samples(
            trainingFraction,
            normalizeInputs,
            Records,
            NbrValuesPerRecord,
            NbrOutputs,
            rnd);
    }

    protected Samples GetSamples(
        int nbrTrainingSamples,
        int nbrTestingSamples,
        bool normalizeInputs)
    {
        return new Samples(
            nbrTrainingSamples,
            nbrTestingSamples,
            normalizeInputs,
            Records,
            NbrValuesPerRecord,
            NbrOutputs);
    }
}
