using SamplesGeneratorLib;

namespace SetupLib;

internal sealed class FileSamplesGeneratorStrategy : ISamplesGeneratorStrategy
{
    private readonly IPathResolver _pathResolver;

    private const bool DefNormalizeInputs = false;
    private const char DefValuesSeparator = ',';
    private const int DefSkipRows = 0;
    private const int DefSkipCols = 0;
    private const bool DefRandomizeSamples = false;

    public FileSamplesGeneratorStrategy(IPathResolver pathResolver)
    {
        _pathResolver = pathResolver ?? throw new ArgumentNullException(nameof(pathResolver));
    }

    public bool CanHandle(FileSamplesGeneratorDto fileSamplesGenerator, FunctionSamplesGeneratorDto functionSamplesGenerator)
    {
        return fileSamplesGenerator != null;
    }

    public Samples Generate(
        string baseDirPath,
        FileSamplesGeneratorDto fileSamplesGenerator,
        FunctionSamplesGeneratorDto functionSamplesGenerator,
        int nbrOutputs,
        Random rnd)
    {
        if (!string.IsNullOrEmpty(fileSamplesGenerator.CombinedFilePath))
        {
            string dataFilePath = _pathResolver.GetAbsoluteFilePath(fileSamplesGenerator.CombinedFilePath, baseDirPath);

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

            string trainingDataFilePath = _pathResolver.GetAbsoluteFilePath(fileSamplesGenerator.TrainingFilePath, baseDirPath);
            string testingDataFilePath = _pathResolver.GetAbsoluteFilePath(fileSamplesGenerator.TestingFilePath, baseDirPath);

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
}
