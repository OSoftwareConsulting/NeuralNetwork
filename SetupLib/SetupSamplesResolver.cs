using SamplesGeneratorLib;

namespace SetupLib;

internal sealed class SetupSamplesResolver : ISamplesFactory
{
    private readonly ISamplesGeneratorStrategy[] _strategies;

    public SetupSamplesResolver()
        : this(new UtilitiesTypeActivator(), new UtilitiesPathResolver())
    {
    }

    public SetupSamplesResolver(ITypeActivator typeActivator, IPathResolver pathResolver)
        : this(
        [
            new FileSamplesGeneratorStrategy(pathResolver),
            new FunctionSamplesGeneratorStrategy(typeActivator)
        ])
    {
    }

    private SetupSamplesResolver(ISamplesGeneratorStrategy[] strategies)
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
        ValidateSingleSamplesGenerator(fileSamplesGenerator, functionSamplesGenerator);

        foreach (var strategy in _strategies)
        {
            if (strategy.CanHandle(fileSamplesGenerator, functionSamplesGenerator))
            {
                return strategy.Generate(baseDirPath, fileSamplesGenerator, functionSamplesGenerator, nbrOutputs, rnd);
            }
        }

        throw new InvalidOperationException("Must specify one and only one Samples Generator");
    }

    private static void ValidateSingleSamplesGenerator(
        FileSamplesGeneratorDto fileSamplesGenerator,
        FunctionSamplesGeneratorDto functionSamplesGenerator)
    {
        int nbrGenerators =
            (fileSamplesGenerator == null ? 0 : 1) +
            (functionSamplesGenerator == null ? 0 : 1);

        if (nbrGenerators != 1)
        {
            throw new InvalidOperationException("Must specify one and only one Samples Generator");
        }
    }
}
