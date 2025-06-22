/*
 * Copyright ©
 * 2025
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using SetupLib;

namespace NeuralNetworkExec;

public class NeuralNetworkMain
{
    public static void Main(string[] args)
    {
        try
        {
            if (args.Length < 1)
            {
                throw new ArgumentException("Usage: NeuralNetwork <path to the JSON test setup file>");
            }

            // Read and Generate the Neural Network Setup (Neural Network Structure and Training / Testing Samples)
            var setup = SetupReader.GetNeuralNetworkSetup(args[0]);

            NeuralNetworkRunner.RunNeuralNetwork(setup);

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}