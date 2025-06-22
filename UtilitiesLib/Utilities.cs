/*
 * Copyright ©
 * 2025
 * Osella Ventures, LLC
 * All Rights Reserved
*/

using System.Text;

namespace UtilitiesLib;

public static class Utilities
{
    private const string defDblFmt = "F7";
    private const char defValuesSeparator = ',';

    public static void PrintMatrix(
        string prefix,
        double[][] matrix,
        char valuesSeparator = defValuesSeparator,
        string format = defDblFmt)
    {
        Console.WriteLine(prefix);
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            PrintValues(matrix[i], valuesSeparator, format, true);
        }
    }

    public static void PrintArray(
        string prefix,
        double[] array,
        char valuesSeparator = defValuesSeparator,
        string format = defDblFmt)
    {
        Console.Write(prefix);
        PrintValues(array, valuesSeparator, format, true);
    }

    public static void PrintValues(
        double[] values,
        char valuesSeparator = defValuesSeparator,
        string format = defDblFmt,
        bool addEOL = false)
    {
        StringBuilder sb = new StringBuilder();

        for (int i = 0; i < values.Length; i++)
        {
            if (i > 0)
            {
                sb.Append($"{valuesSeparator} ");
            }
            sb.Append($"{values[i].ToString(format)}");
        }

        if (addEOL)
        {
            Console.WriteLine(sb);
        }
        else
        {
            Console.Write(sb);
        }
    }

    public static int[] GenerateSequence(
        int len,
        Random rnd = null)
    {
        var sequence = new int[len];

        for (int i = 0; i < len; i++)
        {
            sequence[i] = i;
        }

        if (rnd != null)
        {
            for (int i = 0; i < len; i++)
            {
                int ii = rnd.Next(0, len);

                var tmp = sequence[i];
                sequence[i] = sequence[ii];
                sequence[ii] = tmp;
            }
        }

        return sequence;
    }

    public static double NextDouble(
        this Random rnd,
        double minValue,
        double range)
    {
        var rndVal = rnd.NextDouble();      //  0 .. 1
        return minValue + (rndVal * range); // minValue .. maxValue
    }

    // Computes the input value's Order Of Magnitude
    // 10 >= x < 0
    // ex: value = 1.095 returns 0
    // ex: value = 17.99 returns 1
    // ex: value = 122.8 returns 2
    // ex: value = 0.006399 returns -3
    public static int OrderOfMagnitude(double value)
    {
        double absValue = Math.Abs(value);
        int orderOfMagnitude = 0;

        if (absValue >= 10d)
        {
            do
            {
                orderOfMagnitude++;
                absValue /= 10d;
            } while (absValue >= 10d);
        }
        else if (absValue < 1d)
        {
            do
            {
                orderOfMagnitude--;
                absValue *= 10d;
            } while (absValue < 1d);
        }

        return orderOfMagnitude;
    }
}
