using SamplesGeneratorLib;

namespace SetupLib;

internal sealed class FunctionSamplesGeneratorStrategy : ISamplesGeneratorStrategy
{
    private readonly ITypeActivator _typeActivator;

    public FunctionSamplesGeneratorStrategy(ITypeActivator typeActivator)
    {
        _typeActivator = typeActivator ?? throw new ArgumentNullException(nameof(typeActivator));
    }

    public bool CanHandle(FileSamplesGeneratorDto fileSamplesGenerator, FunctionSamplesGeneratorDto functionSamplesGenerator)
    {
        return functionSamplesGenerator?.SamplesGeneratorFunction != null;
    }

    public Samples Generate(
        string baseDirPath,
        FileSamplesGeneratorDto fileSamplesGenerator,
        FunctionSamplesGeneratorDto functionSamplesGenerator,
        int nbrOutputs,
        Random rnd)
    {
        ISamplesGeneratorFunction dataGeneratorFunction =
            (ISamplesGeneratorFunction)_typeActivator.GetInstance(functionSamplesGenerator.SamplesGeneratorFunction);
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
