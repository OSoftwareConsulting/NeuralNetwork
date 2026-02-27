using SamplesGeneratorLib;

namespace SetupLib;

internal static class SetupSamplesFactory
{
    private static readonly ISamplesFactory Resolver = new SetupSamplesResolver();

    public static Samples CreateSamples(
        string baseDirPath,
        FileSamplesGeneratorDto fileSamplesGenerator,
        FunctionSamplesGeneratorDto functionSamplesGenerator,
        int nbrOutputs,
        Random rnd)
    {
        return Resolver.CreateSamples(
            baseDirPath,
            fileSamplesGenerator,
            functionSamplesGenerator,
            nbrOutputs,
            rnd);
    }
}
