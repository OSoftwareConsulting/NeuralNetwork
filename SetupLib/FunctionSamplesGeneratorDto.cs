/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace SetupLib;

// Used to create training and testing samples by calling a function with random inputs between min & max values
public class FunctionSamplesGeneratorDto
{
    public string SamplesGeneratorFunction { get; set; }

    public int NbrRecords { get; set; }

    public double TrainingFraction { get; set; }

    public ValueRangeDto[] ValueRanges { get; set; }

    public bool? NormalizeInputs { get; set; }
}
