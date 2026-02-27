using SamplesGeneratorLib;

namespace SetupLib;

internal interface ISamplesGeneratorStrategy
{
    bool CanHandle(FileSamplesGeneratorDto fileSamplesGenerator, FunctionSamplesGeneratorDto functionSamplesGenerator);

    Samples Generate(
        string baseDirPath,
        FileSamplesGeneratorDto fileSamplesGenerator,
        FunctionSamplesGeneratorDto functionSamplesGenerator,
        int nbrOutputs,
        Random rnd);
}
