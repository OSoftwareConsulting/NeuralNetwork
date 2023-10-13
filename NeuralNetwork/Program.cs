/*
 * Copyright ©
 * 2023
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using NeuralNetworkLib;
using TestSetupLib;

namespace NeuralNetworkMain
{
    public class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 1)
                {
                    throw new ArgumentException("Must specify the path to the JSON test setup file");
                }

                var testSetup = TestSetupReader.GetTestSetup(args[0]);

                var neuralNetwork = new NeuralNetwork(
                    testSetup.NbrInputs,
                    testSetup.LayerConfigs,
                    testSetup.Rnd,
                    testSetup.NeuralNetworkFuncs);

                neuralNetwork.Train(
                    testSetup.Samples.TrainingInputs,
                    testSetup.Samples.TrainingOutputs,
                    testSetup.NbrEpochs,
                    testSetup.TrainingRate,
                    testSetup.TrainingMomentum);

                neuralNetwork.Test(
                    testSetup.Samples.TestingInputs,
                    testSetup.Samples.TestingOutputs);

                testSetup.NeuralNetworkFuncs.SummarizeTestResults();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
        }
    }
}