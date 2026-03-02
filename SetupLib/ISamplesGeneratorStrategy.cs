using SamplesGeneratorLib;

namespace SetupLib;

internal interface ISamplesGeneratorStrategy
{
    bool CanHandle(SamplesGeneratorDto samplesGenerator);

    Samples Generate(
        string baseDirPath,
        SamplesGeneratorDto samplesGenerator,
        int nbrOutputs,
        Random rnd);
}
