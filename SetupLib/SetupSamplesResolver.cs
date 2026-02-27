using SamplesGeneratorLib;

namespace SetupLib;

internal sealed class SetupSamplesResolver : ISamplesFactory
{
    private readonly ISamplesGeneratorStrategy[] _strategies;

    public SetupSamplesResolver()
        : this(new ISamplesGeneratorStrategy[]
        {
            new FileSamplesGeneratorStrategy(),
            new FunctionSamplesGeneratorStrategy()
        })
    {
    }

    public SetupSamplesResolver(ISamplesGeneratorStrategy[] strategies)
    {
        _strategies = strategies ?? throw new ArgumentNullException(nameof(strategies));
    }

    public Samples CreateSamples(
        string baseDirPath,
        FileSamplesGeneratorDto fileSamplesGenerator,
        FunctionSamplesGeneratorDto functionSamplesGenerator,
        int nbrOutputs,
        Random rnd)
    {
        SetupValidators.ValidateSingleSamplesGenerator(fileSamplesGenerator, functionSamplesGenerator);

        foreach (var strategy in _strategies)
        {
            if (strategy.CanHandle(fileSamplesGenerator, functionSamplesGenerator))
            {
                return strategy.Generate(baseDirPath, fileSamplesGenerator, functionSamplesGenerator, nbrOutputs, rnd);
            }
        }

        throw new InvalidOperationException("Must specify one and only one Samples Generator");
    }
}
