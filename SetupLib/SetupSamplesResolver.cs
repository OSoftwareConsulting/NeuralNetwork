using SamplesGeneratorLib;

namespace SetupLib;

internal sealed class SetupSamplesResolver : ISamplesFactory
{
    private readonly ISamplesGeneratorStrategy[] _strategies;

    public SetupSamplesResolver()
        : this(new UtilitiesTypeActivator(), new UtilitiesPathResolver())
    {
    }

    /// <summary>
    /// Lists all of the known and used Samples Generators
    /// </summary>
    /// <param name="typeActivator"></param>
    /// <param name="pathResolver"></param>
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
        SamplesGeneratorOptions options,
        int nbrOutputs,
        Random rnd)
    {
        ValidateSingleSamplesGenerator(options);

        foreach (var strategy in _strategies)
        {
            if (strategy.CanHandle(options))
            {
                return strategy.Generate(baseDirPath, options, nbrOutputs, rnd);
            }
        }

        // Should not get here due to the validation
        throw new InvalidOperationException("Must specify one and only one Samples Generator");
    }

    private static void ValidateSingleSamplesGenerator(SamplesGeneratorOptions options)
    {
        int nbrGenerators =
            (options?.File == null ? 0 : 1) +
            (options?.Function == null ? 0 : 1);

        if (nbrGenerators != 1)
        {
            throw new InvalidOperationException("Must specify one and only one Samples Generator");
        }
    }
}
