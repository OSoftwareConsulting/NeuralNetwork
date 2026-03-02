using SamplesGeneratorLib;

namespace SetupLib;

internal interface ISamplesGeneratorStrategy
{
    bool CanHandle(SamplesGeneratorOptions options);

    Samples Generate(
        string baseDirPath,
        SamplesGeneratorOptions options,
        int nbrOutputs,
        Random rnd);
}
