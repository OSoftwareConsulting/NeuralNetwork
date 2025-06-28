using NeuralNetworkLib;
using SamplesGeneratorLib;

namespace SetupLib;

public class NeuralNetworkTestSetup
{
    // General

    // The Random number generator used to compute all random numbers
    public Random Rnd { get; }

    public bool Debug { get; }

    // Data Set

    // The Training and Testing Samples
    public Samples Samples { get; }

    // Neural Network

    // The File Path of the Neural Network's Memory
    public string MemoryFilePath { get; }

    // Testing

    // The user-defined functions used during neural network training and testing (see IUserDefinedFunctions)
    public IUserDefinedFunctions UserDefinedFunctions { get; }

    public NeuralNetworkTestSetup(
        Random rnd,
        bool debug,
        Samples samples,
        string memoryFilePath,
        IUserDefinedFunctions userDefinedFunctions)
    {
        Rnd = rnd;
        Debug = debug;
        Samples = samples;
        MemoryFilePath = memoryFilePath;
        UserDefinedFunctions = userDefinedFunctions;

        // Initialize the User Defined Functions' data structures
        UserDefinedFunctions.Configure(Samples.NbrInputs, Samples.NbrOutputs);
    }
}
