using SamplesGeneratorLib;

namespace SetupLib;

internal interface ISamplesFactory
{
    Samples CreateSamples(
        string baseDirPath,
        SamplesGeneratorDto samplesGenerator,
        int nbrOutputs,
        Random rnd);
}
