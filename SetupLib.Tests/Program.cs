using SamplesGeneratorLib;
using SetupLib;

namespace SetupLib.Tests;

internal static class Program
{
    private static int Main()
    {
        var tests = new (string Name, Action Test)[]
        {
            ("ValidateLayerConfigs checks null and empty", SetupValidatorsTests.ValidateLayerConfigsChecksNullAndEmpty),
            ("ValidateGANeuronLayerConfig checks required fields", SetupValidatorsTests.ValidateGANeuronLayerConfigChecksRequiredFields),
            ("ValidateMemoryFilePath checks null and empty", SetupValidatorsTests.ValidateMemoryFilePathChecksNullAndEmpty),
            ("ValidateUserDefinedFunctions checks null only", SetupValidatorsTests.ValidateUserDefinedFunctionsChecksNullOnly),
            ("ValidateNbrEpochs checks positive values", SetupValidatorsTests.ValidateNbrEpochsChecksPositiveValues),
            ("CreateSamples rejects missing generator definitions", SetupSamplesResolverTests.CreateSamplesRejectsMissingGeneratorDefinitions),
            ("CreateSamples from combined file uses expected split", SetupSamplesResolverTests.CreateSamplesFromCombinedFileUsesExpectedSplit),
            ("CreateSamples rejects missing training fraction for combined file", SetupSamplesResolverTests.CreateSamplesRejectsMissingTrainingFractionForCombinedFile),
            ("CreateSamples rejects randomize with separate files", SetupSamplesResolverTests.CreateSamplesRejectsRandomizeWithSeparateFiles),
            ("CreateSamples from function generator returns valid samples", SetupSamplesResolverTests.CreateSamplesFromFunctionGeneratorReturnsValidSamples),
            ("CreateSamples rejects invalid function generator ranges", SetupSamplesResolverTests.CreateSamplesRejectsInvalidFunctionGeneratorRanges),
            ("CreateSamples rejects multiple generator definitions", SetupSamplesResolverTests.CreateSamplesRejectsMultipleGeneratorDefinitions),
            ("Static SetupSamplesFactory wrapper delegates to resolver", SetupSamplesResolverTests.StaticWrapperDelegatesToResolver)
        };

        int failures = 0;
        foreach (var (name, test) in tests)
        {
            try
            {
                test();
                Console.WriteLine($"PASS: {name}");
            }
            catch (Exception ex)
            {
                failures++;
                Console.WriteLine($"FAIL: {name}");
                Console.WriteLine(ex.ToString());
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Total: {tests.Length}, Failed: {failures}");
        return failures == 0 ? 0 : 1;
    }
}

internal static class TestAssert
{
    public static void True(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException(message);
        }
    }

    public static void Equal<T>(T expected, T actual, string message)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
        {
            throw new InvalidOperationException($"{message}. Expected: {expected}, Actual: {actual}");
        }
    }

    public static void NearlyEqual(double expected, double actual, double epsilon, string message)
    {
        if (Math.Abs(expected - actual) > epsilon)
        {
            throw new InvalidOperationException($"{message}. Expected: {expected}, Actual: {actual}, Epsilon: {epsilon}");
        }
    }

    public static T Throws<T>(Action action, string message) where T : Exception
    {
        try
        {
            action();
        }
        catch (T ex)
        {
            return ex;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"{message}. Expected {typeof(T).Name}, but got {ex.GetType().Name}.", ex);
        }

        throw new InvalidOperationException($"{message}. Expected {typeof(T).Name}, but no exception was thrown.");
    }
}

internal static class SetupValidatorsTests
{
    public static void ValidateLayerConfigsChecksNullAndEmpty()
    {
        TestAssert.Throws<InvalidOperationException>(
            () => SetupValidators.ValidateLayerConfigs(null),
            "Null layer configs should be rejected");

        TestAssert.Throws<InvalidOperationException>(
            () => SetupValidators.ValidateLayerConfigs(Array.Empty<NeuronLayerConfigDto>()),
            "Empty layer configs should be rejected");

        SetupValidators.ValidateLayerConfigs(
            new[] { new NeuronLayerConfigDto { NbrOutputs = 1, ActivationFunction = "X", InitialWeightRange = 1.0 } });
    }

    public static void ValidateGANeuronLayerConfigChecksRequiredFields()
    {
        TestAssert.Throws<InvalidOperationException>(
            () => SetupValidators.ValidateGANeuronLayerConfig(null),
            "Null GA layer config should be rejected");

        TestAssert.Throws<InvalidOperationException>(
            () => SetupValidators.ValidateGANeuronLayerConfig(
                new GANeuronLayerConfigDto
                {
                    NbrOutputs = [1, 2],
                    ActivationFunction = null,
                    InitialWeightRange = [0.1, 0.5]
                }),
            "Missing activation functions should be rejected");

        SetupValidators.ValidateGANeuronLayerConfig(
            new GANeuronLayerConfigDto
            {
                NbrOutputs = [1, 2],
                ActivationFunction = ["A", "B"],
                InitialWeightRange = [0.1, 0.5]
            });
    }

    public static void ValidateMemoryFilePathChecksNullAndEmpty()
    {
        TestAssert.Throws<InvalidOperationException>(
            () => SetupValidators.ValidateMemoryFilePath(null),
            "Null memory path should be rejected");

        TestAssert.Throws<InvalidOperationException>(
            () => SetupValidators.ValidateMemoryFilePath(string.Empty),
            "Empty memory path should be rejected");

        SetupValidators.ValidateMemoryFilePath("memory.bin");
    }

    public static void ValidateUserDefinedFunctionsChecksNullOnly()
    {
        TestAssert.Throws<InvalidOperationException>(
            () => SetupValidators.ValidateUserDefinedFunctions(null),
            "Null user-defined functions should be rejected");

        SetupValidators.ValidateUserDefinedFunctions(string.Empty);
    }

    public static void ValidateNbrEpochsChecksPositiveValues()
    {
        TestAssert.Throws<InvalidOperationException>(
            () => SetupValidators.ValidateNbrEpochs(0),
            "Zero epochs should be rejected");
        TestAssert.Throws<InvalidOperationException>(
            () => SetupValidators.ValidateNbrEpochs(-1),
            "Negative epochs should be rejected");

        SetupValidators.ValidateNbrEpochs(1);
    }
}

internal static class SetupSamplesResolverTests
{
    private static readonly ISamplesFactory SamplesFactory = new SetupSamplesResolver();

    public static void CreateSamplesRejectsMissingGeneratorDefinitions()
    {
        TestAssert.Throws<InvalidOperationException>(
            () => SamplesFactory.CreateSamples(Environment.CurrentDirectory, null, null, nbrOutputs: 1, rnd: new Random(1)),
            "Missing generators should be rejected");
    }

    public static void CreateSamplesFromCombinedFileUsesExpectedSplit()
    {
        using var temp = new TempDir();
        File.WriteAllText(
            Path.Combine(temp.Path, "combined.csv"),
            "1,2,0\n3,4,1\n5,6,0\n7,8,1\n");

        var fileDto = new FileSamplesGeneratorDto
        {
            CombinedFilePath = "combined.csv",
            TrainingFraction = 0.5
        };

        var samples = SamplesFactory.CreateSamples(temp.Path, fileDto, null, nbrOutputs: 1, rnd: new Random(1));

        TestAssert.Equal(2, samples.TrainingInputs.Length, "Training sample count");
        TestAssert.Equal(2, samples.TestingInputs.Length, "Testing sample count");
        TestAssert.Equal(2, samples.NbrInputs, "Input dimension");
        TestAssert.Equal(1, samples.NbrOutputs, "Output dimension");
        TestAssert.NearlyEqual(1d, samples.TrainingInputs[0][0], 1e-12, "First training input[0]");
        TestAssert.NearlyEqual(2d, samples.TrainingInputs[0][1], 1e-12, "First training input[1]");
        TestAssert.NearlyEqual(0d, samples.TrainingTargets[0][0], 1e-12, "First training target");
    }

    public static void CreateSamplesRejectsMissingTrainingFractionForCombinedFile()
    {
        using var temp = new TempDir();
        File.WriteAllText(Path.Combine(temp.Path, "combined.csv"), "1,2,0\n3,4,1\n");

        var fileDto = new FileSamplesGeneratorDto
        {
            CombinedFilePath = "combined.csv"
        };

        TestAssert.Throws<InvalidOperationException>(
            () => SamplesFactory.CreateSamples(temp.Path, fileDto, null, nbrOutputs: 1, rnd: new Random(1)),
            "Combined file without TrainingFraction should be rejected");
    }

    public static void CreateSamplesRejectsRandomizeWithSeparateFiles()
    {
        using var temp = new TempDir();
        File.WriteAllText(Path.Combine(temp.Path, "train.csv"), "1,2,0\n");
        File.WriteAllText(Path.Combine(temp.Path, "test.csv"), "3,4,1\n");

        var fileDto = new FileSamplesGeneratorDto
        {
            TrainingFilePath = "train.csv",
            TestingFilePath = "test.csv",
            RandomizeSamples = true
        };

        TestAssert.Throws<InvalidOperationException>(
            () => SamplesFactory.CreateSamples(temp.Path, fileDto, null, nbrOutputs: 1, rnd: new Random(1)),
            "Separate files with randomization should be rejected");
    }

    public static void CreateSamplesFromFunctionGeneratorReturnsValidSamples()
    {
        var fnDto = new FunctionSamplesGeneratorDto
        {
            SamplesGeneratorFunction = typeof(LinearSamplesGeneratorFunction).AssemblyQualifiedName,
            NbrRecords = 20,
            TrainingFraction = 0.6,
            NormalizeInputs = false,
            ValueRanges =
            [
                new ValueRangeDto { MinValue = 0, MaxValue = 10 },
                new ValueRangeDto { MinValue = 1, MaxValue = 3 }
            ]
        };

        var samples = SamplesFactory.CreateSamples(
            baseDirPath: Environment.CurrentDirectory,
            fileSamplesGenerator: null,
            functionSamplesGenerator: fnDto,
            nbrOutputs: 1,
            rnd: new Random(7));

        TestAssert.Equal(12, samples.TrainingInputs.Length, "Training sample count");
        TestAssert.Equal(8, samples.TestingInputs.Length, "Testing sample count");
        TestAssert.Equal(2, samples.NbrInputs, "Input dimension");
        TestAssert.Equal(1, samples.NbrOutputs, "Output dimension");

        for (int i = 0; i < samples.TrainingInputs.Length; i++)
        {
            var input = samples.TrainingInputs[i];
            var target = samples.TrainingTargets[i][0];
            TestAssert.NearlyEqual(input[0] + input[1], target, 1e-9, $"Training target mismatch at index {i}");
        }
    }

    public static void CreateSamplesRejectsInvalidFunctionGeneratorRanges()
    {
        var fnDto = new FunctionSamplesGeneratorDto
        {
            SamplesGeneratorFunction = typeof(LinearSamplesGeneratorFunction).AssemblyQualifiedName,
            NbrRecords = 10,
            TrainingFraction = 0.5,
            ValueRanges = Array.Empty<ValueRangeDto>()
        };

        TestAssert.Throws<InvalidOperationException>(
            () => SamplesFactory.CreateSamples(
                baseDirPath: Environment.CurrentDirectory,
                fileSamplesGenerator: null,
                functionSamplesGenerator: fnDto,
                nbrOutputs: 1,
                rnd: new Random(3)),
            "Function generator without ranges should be rejected");
    }

    public static void CreateSamplesRejectsMultipleGeneratorDefinitions()
    {
        using var temp = new TempDir();
        File.WriteAllText(Path.Combine(temp.Path, "combined.csv"), "1,2,0\n3,4,1\n");

        var fileDto = new FileSamplesGeneratorDto
        {
            CombinedFilePath = "combined.csv",
            TrainingFraction = 0.5
        };

        var fnDto = new FunctionSamplesGeneratorDto
        {
            SamplesGeneratorFunction = typeof(LinearSamplesGeneratorFunction).AssemblyQualifiedName,
            NbrRecords = 10,
            TrainingFraction = 0.5,
            ValueRanges = [new ValueRangeDto { MinValue = 0, MaxValue = 1 }]
        };

        TestAssert.Throws<InvalidOperationException>(
            () => SamplesFactory.CreateSamples(temp.Path, fileDto, fnDto, nbrOutputs: 1, rnd: new Random(1)),
            "Both generators should be rejected");
    }

    public static void StaticWrapperDelegatesToResolver()
    {
        using var temp = new TempDir();
        File.WriteAllText(
            Path.Combine(temp.Path, "combined.csv"),
            "1,2,0\n3,4,1\n5,6,0\n7,8,1\n");

        var fileDto = new FileSamplesGeneratorDto
        {
            CombinedFilePath = "combined.csv",
            TrainingFraction = 0.5
        };

        var resolverSamples = SamplesFactory.CreateSamples(temp.Path, fileDto, null, nbrOutputs: 1, rnd: new Random(1));
        var wrapperSamples = SetupSamplesFactory.CreateSamples(temp.Path, fileDto, null, nbrOutputs: 1, rnd: new Random(1));

        TestAssert.Equal(resolverSamples.TrainingInputs.Length, wrapperSamples.TrainingInputs.Length, "Wrapper training sample count");
        TestAssert.Equal(resolverSamples.TestingInputs.Length, wrapperSamples.TestingInputs.Length, "Wrapper testing sample count");
        TestAssert.Equal(resolverSamples.NbrInputs, wrapperSamples.NbrInputs, "Wrapper input dimension");
        TestAssert.Equal(resolverSamples.NbrOutputs, wrapperSamples.NbrOutputs, "Wrapper output dimension");
    }
}

public sealed class LinearSamplesGeneratorFunction : ISamplesGeneratorFunction
{
    public void Compute(double[] x, double[] y) => y[0] = x[0] + x[1];
}

internal sealed class TempDir : IDisposable
{
    public string Path { get; }

    public TempDir()
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"setup-lib-tests-{Guid.NewGuid():N}");
        Directory.CreateDirectory(Path);
    }

    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
