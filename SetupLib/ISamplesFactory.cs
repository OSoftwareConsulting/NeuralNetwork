using SamplesGeneratorLib;

namespace SetupLib;

internal interface ISamplesFactory
{
    Samples CreateSamples(
        string baseDirPath,
        FileSamplesGeneratorDto fileSamplesGenerator,
        FunctionSamplesGeneratorDto functionSamplesGenerator,
        int nbrOutputs,
        Random rnd);
}
