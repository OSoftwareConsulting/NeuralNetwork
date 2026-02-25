/*
 * Copyright ©
 * 2026
 * Osella Ventures, LLC
 * All Rights Reserved
*/

namespace SamplesGeneratorLib;

/// <summary>
/// File-based Samples Generator
/// </summary>
public class FileSamplesGenerator : SamplesGenerator
{
    // Private constructor (calls the base class constructor)
    private FileSamplesGenerator(
        int nbrOutputs) :
        base(
            nbrOutputs)
    {
    }

    /// <summary>
    /// Entry point to read in and generate the training and testing sample sets
    /// If rnd is null, the samples are ordered as the records occur in the data file, otherwise the order of the samples is randomized 
    /// </summary>
    public static Samples GetSamples(
        string filePath,
        int nbrOutputs,
        double trainingFraction,
        bool normalizeInputs,
        char separator,
        int skipRows,
        int skipColumns,
        Random rnd = null)
    {
        var dfr = new FileSamplesGenerator(nbrOutputs);

        dfr.ReadRecordsFromFile(
            filePath,
            separator,
            skipRows,
            skipColumns);

        return dfr.GetSamples(
            trainingFraction,
            normalizeInputs,
            rnd);
    }

    /// <summary>
    /// Entry point to read in and generate the training and testing sample sets
    /// </summary>
    public static Samples GetSamples(
        string trainingFilePath,
        string testingFilePath,
        int nbrOutputs,
        bool normalizeInputs,
        char separator,
        int skipRows,
        int skipColumns)
    {
        var dfr = new FileSamplesGenerator(nbrOutputs);

        (int nbrTrainingSamples, int nbrTestingSamples) = dfr.ReadRecordsFromFiles(
            trainingFilePath,
            testingFilePath,
            separator,
            skipRows,
            skipColumns);

        return dfr.GetSamples(
            nbrTrainingSamples,
            nbrTestingSamples,
            normalizeInputs);
    }

    /// <summary>
    /// Reads the Records from the specified data file and copy them to the records matrix
    /// </summary>
    private void ReadRecordsFromFile(
        string filePath,
        char separator,
        int skipRows,
        int skipColumns)
    {
        List<Record> recordsList = new List<Record>();

        ReadRecordsFromFile(
            filePath,
            separator,
            skipRows,
            skipColumns,
            recordsList);

        CopyRecords(recordsList);
    }

    /// <summary>
    /// Reads the Records from the specified training and testing data files, and copy them to the records matrix
    /// </summary>
    private (int, int) ReadRecordsFromFiles(
        string trainingFilePath,
        string testingFilePath,
        char separator,
        int skipRows,
        int skipColumns)
    {
        List<Record> recordsList = new List<Record>();

        int nbrTrainingSamples = ReadRecordsFromFile(
            trainingFilePath,
            separator,
            skipRows,
            skipColumns,
            recordsList);
        int nbrTestingSamples = ReadRecordsFromFile(
            testingFilePath,
            separator,
            skipRows,
            skipColumns,
            recordsList);

        CopyRecords(recordsList);

        return (nbrTrainingSamples, nbrTestingSamples);
    }

    /// <summary>
    /// Reads the Records from the data file and store them in the records list
    /// </summary>
    private int ReadRecordsFromFile(
        string filePath,
        char separator,
        int skipRows,
        int skipColumns,
        List<Record> recordsList)
    {
        int nbrRecordsIn = recordsList.Count();

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
                if (NbrValuesPerRecord == 0)
                {
                    if (values.Length < NbrOutputs)
                    {
                        throw new InvalidOperationException($"The number of values per record {values.Length} must be less than the number of outputs {NbrOutputs}");
                    }

                    // Set the Number of Values Per Record
                    NbrValuesPerRecord = values.Length;
                }
                else if (values.Length != NbrValuesPerRecord)
                {
                    throw new InvalidOperationException($"The number of values per record {NbrValuesPerRecord} does not equal to the number of values read in {values.Length}");
                }

                // Add the record to the list of records
                recordsList.Add(new Record(values));
            }
        }

        return recordsList.Count() - nbrRecordsIn;
    }

    /// <summary>
    /// Copy the records from the records list to the records matrix
    /// </summary>
    private void CopyRecords(List<Record> recordsList)
    {
        int nbrRecords = recordsList.Count();

        Records = new double[nbrRecords][];

        int n = 0;
        foreach (var record in recordsList)
        {
            Records[n++] = record.Values;
        }
    }

    /// <summary>
    /// Private class used to store a record read in from the file
    /// </summary>
    private class Record
    {
        public double[] Values { get; }

        public Record(double[] values)
        {
            Values = values;
        }
    }
}