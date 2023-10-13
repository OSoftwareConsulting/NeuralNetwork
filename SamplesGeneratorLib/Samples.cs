/*
 * Copyright ©
 * 2023
 * Osella Ventures, LLC
 * All Rights Reserved
*/

//#define PRINTMSGS

using UtilitiesLib;

namespace SamplesGeneratorLib
{
    public class Samples
    {
        public double[][] TrainingInputs { get; }
        public double[][] TrainingOutputs { get; }
        public double[][] TestingInputs { get; }
        public double[][] TestingOutputs { get; }

        internal Samples(
            double trainingFraction,
            double[][] records,
            int nbrValuesPerRecord,
            int nbrOutputs,
            Random rnd = null)
        {
            if ((trainingFraction < 0.0) || (trainingFraction > 1.0))
            {
                throw new InvalidOperationException();
            }

            int nbrRecords = records.Count();
            int nbrTrainingSamples = (int)(nbrRecords * trainingFraction);
            int nbrTestingSamples = nbrRecords - nbrTrainingSamples;
            int nbrInputs = nbrValuesPerRecord - nbrOutputs;

            TrainingInputs = new double[nbrTrainingSamples][];
            TrainingOutputs = new double[nbrTrainingSamples][];
            TestingInputs = new double[nbrTestingSamples][];
            TestingOutputs = new double[nbrTestingSamples][];

            var sequence = Utilities.GenerateSequence(nbrRecords, rnd);

            int n = 0;

#if PRINTMSGS
            Console.WriteLine("Training Samples");
#endif

            for (int i = 0; i < nbrTrainingSamples; i++, n++)
            {
                int nn = sequence[n];

                TrainingInputs[i] = new double[nbrInputs];
                TrainingOutputs[i] = new double[nbrOutputs];

                int m = 0;

                for (int j = 0; j < nbrInputs; j++, m++)
                {
                    TrainingInputs[i][j] = records[nn][m];
                }

                for (int j = 0; j < nbrOutputs; j++, m++)
                {
                    TrainingOutputs[i][j] = records[nn][m];
                }

#if PRINTMSGS
                Utilities.PrintValues(TrainingInputs[i]);
                Utilities.PrintValues(TrainingOutputs[i]);
                Console.WriteLine();
#endif
            }

#if PRINTMSGS
            Console.WriteLine("Testing Samples");
#endif

            for (int i = 0; i < nbrTestingSamples; i++, n++)
            {
                int nn = sequence[n];

                TestingInputs[i] = new double[nbrInputs];
                TestingOutputs[i] = new double[nbrOutputs];

                int m = 0;

                for (int j = 0; j < nbrInputs; j++, m++)
                {
                    TestingInputs[i][j] = records[nn][m];

                }

                for (int j = 0; j < nbrOutputs; j++, m++)
                {
                    TestingOutputs[i][j] = records[nn][m];
                }

#if PRINTMSGS
                Utilities.PrintValues(TestingInputs[i]);
                Utilities.PrintValues(TestingOutputs[i]);
                Console.WriteLine();
#endif
            }
        }
    }
}
