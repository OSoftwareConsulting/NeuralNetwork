/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace SetupLib;

// Used to create training and testing samples from a file
public class FileSamplesGeneratorDto
{
    public string CombinedFilePath { get; set; }

    public double? TrainingFraction { get; set; }

    public string TrainingFilePath { get; set; }

    public string TestingFilePath { get; set; }

    public char? Separator { get; set; }

    public int? SkipRows { get; set; }

    public int? SkipColumns { get; set; }

    public bool? RandomizeSamples { get; set; }

    public bool? NormalizeInputs { get; set; }
}
