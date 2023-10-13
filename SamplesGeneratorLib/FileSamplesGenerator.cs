/*
 * Copyright ©
 * 2023
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace SamplesGeneratorLib
{
    public class FileSamplesGenerator : SamplesGenerator
    {
        private FileSamplesGenerator(
            int nbrOutputs) :
            base(
                nbrOutputs)
        {
        }

        public static Samples GetSamples(
            int nbrOutputs,
            double trainingFraction,
            string filePath,
            char separator,
            int skipRows,
            int skipColumns,
            Random rnd = null)
        {
            var dfr = new FileSamplesGenerator(nbrOutputs);

            dfr.ReadSamplesFromFile(
                filePath,
                separator,
                skipRows,
                skipColumns);

            // If rnd is null, the samples are ordered as the records occur in the data file, otherwise the order of the samples are randomized 
            return dfr.GetSamples(
                trainingFraction,
                rnd);
        }

        private void ReadSamplesFromFile(
            string filePath,
            char separator,
            int skipRows,
            int skipColumns)
        {
            List<Record> recordsList = new List<Record>();

            using (var rd = new StreamReader(filePath))
            {
                while (!rd.EndOfStream)
                {
                    var line = rd.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    if (skipRows > 0)
                    {
                        skipRows--;
                        continue;
                    }

                    var splits = line.Split(separator);

                    var values = splits.Skip(skipColumns).Select(double.Parse).ToArray();

                    if (nbrValuesPerRecord == 0)
                    {
                        if (values.Length < nbrOutputs)
                        {
                            throw new InvalidOperationException();
                        }
                        nbrValuesPerRecord = values.Length;
                    }
                    else if (values.Length != nbrValuesPerRecord)
                    {
                        throw new InvalidOperationException();
                    }

                    recordsList.Add(new Record(values));
                }
            }

            int nbrRecords = recordsList.Count();

            records = new double[nbrRecords][];

            int n = 0;
            foreach (var record in recordsList)
            {
                records[n++] = record.Values;
            }
        }

        private class Record
        {
            public double[] Values { get; }

            public Record(double[] values)
            {
                Values = values;
            }
        }
    }
}