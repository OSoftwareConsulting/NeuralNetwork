/*
 * Copyright ©
 * 2025
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

    /*
     * --file C:\ov\NeuralNetwork\DataSets\magic04\magic04-ga.json --mode ga
     */

    public static void Main(string[] args)
    {
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

            RootCommand rootCommand = new("Neural Network Training and Testing tool")
            {
                fileOption,
                modeOption
            };

            ParseResult parseResult = rootCommand.Parse(args);

            if (parseResult.Errors.Any())
            {
                foreach (ParseError parseError in parseResult.Errors)
                {
                    Console.Error.WriteLine(parseError.Message);
                }
                return;
            }

            string filePath = string.Empty;
            if (parseResult.GetValue(fileOption) is FileInfo parsedFile)
            {
                filePath = parsedFile.FullName;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                Console.Error.WriteLine($"Must specify an input file");
                return;
            }

            if (!File.Exists(filePath))
            {
                Console.Error.WriteLine($"File '{filePath}' does not exist");
                return;
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
        }
        finally
        {
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}