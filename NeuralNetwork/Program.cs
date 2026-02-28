/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Parsing;

namespace NeuralNetworkExec;

public class NeuralNetworkMain
{
    private enum Modes
    {
        Test,
        TrainAndTest,
        GeneticAlgorithm
    }

    /// <summary>
    /// Entry point for the NeuralNetwork executable
    /// </summary>
    public static int Main(string[] args)
    {
        bool pauseRequested = false;

        int exitCode = 0;

        try
        {
            Option<FileInfo> fileOption = new("--file", "-f")
            {
                Description = "The setup file to read and use for training and/or testing."
            };
            fileOption.Arity = ArgumentArity.ExactlyOne;

            Option<string> modeOption = new("--mode", "-m")
            {
                Description = "Set the train and/or test mode",
            };
            modeOption.Arity = ArgumentArity.ExactlyOne;
            modeOption.AcceptOnlyFromAmong("test", "t", "trainandtest", "tt", "geneticalgorithm", "ga");

            Option<bool> pauseOption = new("--pause", "-p")
            {
                Description = "Wait for a key press before exiting."
            };

            RootCommand rootCommand = new("Neural Network Training and Testing tool")
            {
                fileOption,
                modeOption,
                pauseOption
            };

            ParseResult parseResult = rootCommand.Parse(args);

            if (parseResult.Errors.Any())
            {
                foreach (ParseError parseError in parseResult.Errors)
                {
                    Console.Error.WriteLine(parseError.Message);
                }
                exitCode = 1;
                Environment.ExitCode = exitCode;
                return exitCode;
            }

            pauseRequested = parseResult.GetValue(pauseOption);

            string filePath = string.Empty;
            if (parseResult.GetValue(fileOption) is FileInfo parsedFile)
            {
                filePath = parsedFile.FullName;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                Console.Error.WriteLine($"Must specify an input file");
                exitCode = 1;
                Environment.ExitCode = exitCode;
                return exitCode;
            }

            if (!File.Exists(filePath))
            {
                Console.Error.WriteLine($"File '{filePath}' does not exist");
                exitCode = 1;
                Environment.ExitCode = exitCode;
                return exitCode;
            }

            Modes mode = parseResult.GetValue(modeOption) switch
                {
                    "test" => Modes.Test,
                    "t" => Modes.Test,
                    "trainandtest" => Modes.TrainAndTest,
                    "tt" => Modes.TrainAndTest,
                    "geneticalgorithm" => Modes.GeneticAlgorithm,
                    "ga" => Modes.GeneticAlgorithm,
                    null => Modes.Test,
                    _ => Modes.Test
                };

            switch (mode)
            {
                case Modes.Test:
                    TestMain.Main(filePath);
                    break;
                case Modes.TrainAndTest:
                    TrainAndTestMain.Main(filePath);
                    break;
                case Modes.GeneticAlgorithm:
                    GeneticAlgorithmMain.Main(filePath);
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            exitCode = 1;
        }
        finally
        {
            if (ShouldPauseOnExit(pauseRequested))
            {
                Console.WriteLine("\nPress any key to exit...");
                Console.ReadKey(intercept: true);
            }
        }

        Environment.ExitCode = exitCode;
        return exitCode;
    }

    private static bool ShouldPauseOnExit(bool pauseRequested)
    {
        if (!pauseRequested)
        {
            return false;
        }

        // Pause only when attached to an interactive console.
        return Environment.UserInteractive &&
               !Console.IsInputRedirected &&
               !Console.IsOutputRedirected &&
               !Console.IsErrorRedirected;
    }
}
