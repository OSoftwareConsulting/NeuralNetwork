using SamplesGeneratorLib;
using UtilitiesLib;

namespace SetupLib;

internal static class SetupSamplesFactory
{
    private const bool DefNormalizeInputs = false;
    private const char DefValuesSeparator = ',';
    private const int DefSkipRows = 0;
    private const int DefSkipCols = 0;
    private const bool DefRandomizeSamples = false;

    public static Samples CreateSamples(
        string baseDirPath,
        FileSamplesGeneratorDto fileSamplesGenerator,
        FunctionSamplesGeneratorDto functionSamplesGenerator,
        int nbrOutputs,
        Random rnd)
    {
        SetupValidators.ValidateSingleSamplesGenerator(fileSamplesGenerator, functionSamplesGenerator);

        if (fileSamplesGenerator != null)
        {
            return GetSamplesFromFile(baseDirPath, fileSamplesGenerator, nbrOutputs, rnd);
        }

        if (functionSamplesGenerator?.SamplesGeneratorFunction != null)
        {
            return GetSamplesFromDataGenerator(functionSamplesGenerator, nbrOutputs, rnd);
        }

        throw new InvalidOperationException("Must specify one and only one Samples Generator");
    }

    private static Samples GetSamplesFromFile(
        string baseDirPath,
        FileSamplesGeneratorDto fileSamplesGenerator,
        int nbrOutputs,
        Random rnd)
    {
        if (!string.IsNullOrEmpty(fileSamplesGenerator.CombinedFilePath))
        {
            string dataFilePath = Utilities.GetAbsoluteFilePath(fileSamplesGenerator.CombinedFilePath, baseDirPath);

            if (!fileSamplesGenerator.TrainingFraction.HasValue)
            {
                throw new InvalidOperationException("Must specify a training fraction when using a combined samples file");
            }

            return FileSamplesGenerator.GetSamples(
                dataFilePath,
                nbrOutputs,
                fileSamplesGenerator.TrainingFraction.Value,
                fileSamplesGenerator.NormalizeInputs.GetValueOrDefault(DefNormalizeInputs),
                fileSamplesGenerator.Separator.GetValueOrDefault(DefValuesSeparator),
                fileSamplesGenerator.SkipRows.GetValueOrDefault(DefSkipRows),
                fileSamplesGenerator.SkipColumns.GetValueOrDefault(DefSkipCols),
                fileSamplesGenerator.RandomizeSamples.GetValueOrDefault(DefRandomizeSamples) ? rnd : null);
        }

        if (!string.IsNullOrEmpty(fileSamplesGenerator.TrainingFilePath) &&
            !string.IsNullOrEmpty(fileSamplesGenerator.TestingFilePath))
        {
            if (fileSamplesGenerator.TrainingFraction.HasValue)
            {
                throw new InvalidOperationException("Must not specify a training fraction when using separate training and testing samples files");
            }

            if (fileSamplesGenerator.RandomizeSamples.HasValue && fileSamplesGenerator.RandomizeSamples.Value)
            {
                throw new InvalidOperationException("Must not specify to randomize samples when using separate training and testing samples files");
            }

            string trainingDataFilePath = Utilities.GetAbsoluteFilePath(fileSamplesGenerator.TrainingFilePath, baseDirPath);
            string testingDataFilePath = Utilities.GetAbsoluteFilePath(fileSamplesGenerator.TestingFilePath, baseDirPath);

            return FileSamplesGenerator.GetSamples(
                trainingDataFilePath,
                testingDataFilePath,
                nbrOutputs,
                fileSamplesGenerator.NormalizeInputs.GetValueOrDefault(DefNormalizeInputs),
                fileSamplesGenerator.Separator.GetValueOrDefault(DefValuesSeparator),
                fileSamplesGenerator.SkipRows.GetValueOrDefault(DefSkipRows),
                fileSamplesGenerator.SkipColumns.GetValueOrDefault(DefSkipCols));
        }

        throw new InvalidOperationException("Must specify either a Combined File Path or both Training and Testing File Paths for the File Samples Generator");
    }

    private static Samples GetSamplesFromDataGenerator(
        FunctionSamplesGeneratorDto functionSamplesGenerator,
        int nbrOutputs,
        Random rnd)
    {
        ISamplesGeneratorFunction dataGeneratorFunction =
            (ISamplesGeneratorFunction)Utilities.GetInstance(functionSamplesGenerator.SamplesGeneratorFunction);
        if (dataGeneratorFunction == null)
        {
            throw new InvalidOperationException(
                $"Samples Generator Function Instance {functionSamplesGenerator.SamplesGeneratorFunction} was not loaded");
        }

        if (functionSamplesGenerator.ValueRanges == null || functionSamplesGenerator.ValueRanges.Length == 0)
        {
            throw new InvalidOperationException("Must specify at least one Value Range for the Function Samples Generator");
        }

        var valueRanges = new List<FunctionSamplesGenerator.ValueRange>();
        foreach (var valueRange in functionSamplesGenerator.ValueRanges)
        {
            valueRanges.Add(new FunctionSamplesGenerator.ValueRange(valueRange.MinValue, valueRange.MaxValue));
        }

        return FunctionSamplesGenerator.GetSamples(
            nbrOutputs,
            functionSamplesGenerator.TrainingFraction,
            functionSamplesGenerator.NormalizeInputs ?? false,
            dataGeneratorFunction,
            functionSamplesGenerator.NbrRecords,
            valueRanges.ToArray(),
            rnd);
    }
}
