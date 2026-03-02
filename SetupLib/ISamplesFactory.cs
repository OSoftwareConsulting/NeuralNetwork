using SamplesGeneratorLib;

namespace SetupLib;

internal interface ISamplesFactory
{
    Samples CreateSamples(
        string baseDirPath,
        SamplesGeneratorOptions options,
        int nbrOutputs,
        Random rnd);
}
