/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace SamplesGeneratorLib;

// File-based Samples Generator
public class FileSamplesGenerator : SamplesGenerator
{
    // Private constructor (calls the base class constructor)
    private FileSamplesGenerator(
        int nbrOutputs) :
        base(
            nbrOutputs)
    {
    }

    // Entry point to read in and generate the training and testing sample sets
    public static Samples GetSamples(
        int nbrOutputs,
        double trainingFraction,
        bool normalizeInputs,
        string filePath,
        char separator,
        int skipRows,
        int skipColumns,
        Random rnd = null)
    {
        var dfr = new FileSamplesGenerator(nbrOutputs);

        // Read in the records from the specified file
        dfr.ReadRecordsFromFile(
            filePath,
            separator,
            skipRows,
            skipColumns);

        // If rnd is null, the samples are ordered as the records occur in the data file
        // Otherwise the order of the samples is randomized 
        return dfr.GetSamples(
            trainingFraction,
            normalizeInputs,
            rnd);
    }

    // Reads the Records from the specified file
    private void ReadRecordsFromFile(
        string filePath,
        char separator,
        int skipRows,
        int skipColumns)
    {
        // The Records are maintained in a list
        List<Record> recordsList = new List<Record>();

        using (var rd = new StreamReader(filePath))
        {
            while (!rd.EndOfStream)
            {
                // Read in a line of input from the file
                var line = rd.ReadLine();
                if (line == null)
                {
                    break;
                }

                // Skip the specified number of first rows
                if (skipRows > 0)
                {
                    skipRows--;
                    continue;
                }

                // Split the line of charactors read in from the file by the specified separator charactor
                var splits = line.Split(separator);

                // Get the values (as strings), skipping the first number of columns specified
                var values = splits.Skip(skipColumns).Select(double.Parse).ToArray();

                // If this is the first record line
                if (nbrValuesPerRecord == 0)
                {
                    if (values.Length < nbrOutputs)
                    {
                        throw new InvalidOperationException($"The number of values per record {values.Length} must be less than the number of outputs {nbrOutputs}");
                    }

                    // Set the Number of Values Per Record
                    nbrValuesPerRecord = values.Length;
                }
                else if (values.Length != nbrValuesPerRecord)
                {
                    throw new InvalidOperationException($"The number of values per record {nbrValuesPerRecord} does not equal to the number of values read in {values.Length}");
                }

                // Add the record to the list of records
                recordsList.Add(new Record(values));
            }
        }

        // Copy the record values to the output records matrix
        int nbrRecords = recordsList.Count();

        records = new double[nbrRecords][];

        int n = 0;
        foreach (var record in recordsList)
        {
            records[n++] = record.Values;
        }
    }

    // Private class used to store a record read in from the file
    private class Record
    {
        public double[] Values { get; }

        public Record(double[] values)
        {
            Values = values;
        }
    }
}