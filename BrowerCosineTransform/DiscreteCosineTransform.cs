using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace BrowerCosineTransform;

/// <summary>
/// Helper methods for calculating discrete cosine transform.
/// </summary>
internal class DiscreteCosineTransform
{
    /// <summary>
    /// Luminance quantization matrix for JPEG standard DCT2D compression
    /// </summary>
    private static int[][] quantizationMatrix = new int[][]
    {
        new int[] { 16, 11, 10, 16, 24, 40, 51, 61 },
        new int[] { 12, 12, 14, 19, 26, 58, 60, 55 },
        new int[] { 14, 13, 16, 24, 40, 57, 69, 56 },
        new int[] { 14, 17, 22, 29, 51, 87, 80, 62 },
        new int[] { 18, 22, 37, 56, 68, 109, 103, 77 },
        new int[] { 24, 35, 55, 64, 81, 104, 113, 92 },
        new int[] { 49, 64, 78, 87, 103, 121, 120, 101 },
        new int[] { 72, 92, 95, 98, 112, 100, 103, 99 }
    };

    /// <summary>
    /// Calculate a 2d discrete cosine transform based on two-direction DCT
    /// </summary>
    /// <param name="input">The input to calculate a 2d DCT for</param>
    /// <returns>The DCT coefficients</returns>
    public static double[][] DiscreteTransform2D(double[][] input)
    {
        int N = input.Length; // assuming input is a square block

        // Apply 1D DCT to each row
        double[][] intermediate = new double[N][];
        for (int i = 0; i < N; i++)
        {
            intermediate[i] = GetDiscreteCosineTransform(input[i]);
        }

        // Apply 1D DCT to each column
        double[][] output = new double[N][];
        for (int i = 0; i < N; i++)
        {
            double[] column = new double[N];
            for (int j = 0; j < N; j++)
            {
                column[j] = intermediate[j][i];
            }

            double[] dctColumn = GetDiscreteCosineTransform(column);

            for (int j = 0; j < N; j++)
            {
                intermediate[j][i] = dctColumn[j];
            }
        }

        return intermediate;
    }

    /// <summary>
    /// Calculate the discrete cosine transform coefficients for a given input
    /// </summary>
    /// <param name="input">The inputs data to calculate for</param>
    /// <returns>The discrete cosine transform coefficients</returns>
    public static double[] GetDiscreteCosineTransform(double[] input)
    {
        int N = input.Length;
        double[] output = new double[N];
        double c = Math.PI / (2.0 * N);

        for (int k = 0; k < N; k++)
        {
            output[k] = 0.0;
            for (int n = 0; n < N; n++)
            {
                output[k] += input[n] * Math.Cos((c * (2 * n + 1) * k));
            }

            if (k == 0)
            {
                output[k] *= 1.0 / Math.Sqrt(N);  // sqrt(1/N)
            }
            else
            {
                output[k] *= Math.Sqrt(2.0 / N);   // sqrt(2/N)
            }
        }

        return output;
    }

    /// <summary>
    /// Quantize 2D cosine transform coefficients.
    /// </summary>
    /// <param name="input">The coefficients to quantize</param>
    /// <returns>Quantized coefficient data</returns>
    public static int[][] Quantize(double[][] input)
    {
        int N = input.Length; // assuming input is a square block
        int[][] output = new int[N][];

        for (int i = 0; i < N; i++)
        {
            output[i] = new int[N];
            for (int j = 0; j < N; j++)
            {
                output[i][j] = (int)(input[i][j] / quantizationMatrix[i][j]);
            }
        }

        return output;
    }

    /// <summary>
    /// Quantizes double data into integers
    /// </summary>
    /// <param name="input">The data to quantize</param>
    /// <returns>The quantized result</returns>
    public static int[] Quantize(double[] input)
    {
        int N = input.Length;
        int[] output = new int[N];

        for (int i = 0; i < N; i++)
        {
            output[i] = (int)Math.Round(input[i]);
        }

        return output;
    }

    /// <summary>
    /// Recovers data from discrete cosine transform coefficients
    /// </summary>
    /// <param name="input">The coefficients to get data from</param>
    /// <returns>The recovered data</returns>
    public static double[][] GetInverseDiscreteCosineTransform2D(double[][] input)
    {
        int N = input.Length; // assuming input is a square block

        // Apply 1D IDCT to each column
        double[][] intermediate = new double[N][];
        for (int i = 0; i < N; i++)
        {
            double[] column = new double[N];
            for (int j = 0; j < N; j++)
            {
                column[j] = input[j][i];
            }

            double[] idctColumn = GetInverseDiscreteCosineTransform(column);

            for (int j = 0; j < N; j++)
            {
                if (intermediate[j] == null)
                {
                    intermediate[j] = new double[N];
                }

                intermediate[j][i] = idctColumn[j];
            }
        }

        // Apply 1D IDCT to each row
        double[][] output = new double[N][];
        for (int i = 0; i < N; i++)
        {
            output[i] = GetInverseDiscreteCosineTransform(intermediate[i]);
        }

        return output;
    }

    /// <summary>
    /// Calculates the inverse discrete cosine transform to recover data
    /// </summary>
    /// <param name="input">The discrete cosine transform coefficients to recover from</param>
    /// <returns>The recovered data</returns>
    public static double[] GetInverseDiscreteCosineTransform(double[] input)
    {
        int N = input.Length;
        double[] output = new double[N];
        double c = Math.PI / (2.0 * N);

        for (int n = 0; n < N; n++)
        {
            output[n] = input[0] / Math.Sqrt(N);

            for (int k = 1; k < N; k++)
            {
                output[n] += (2.0 / Math.Sqrt(N)) * input[k] * Math.Cos(c * (2 * n + 1) * k);
            }
        }

        return output;
    }

    /// <summary>
    /// Invert quantization by casting input values for 2d quantized data
    /// </summary>
    /// <param name="input">The quantized data to dequantize</param>
    /// <returns>The dequantized data</returns>
    public static double[][] Dequantize(int[][] input)
    {
        int N = input.Length; // assuming input is a square block
        double[][] output = new double[N][];

        for (int i = 0; i < N; i++)
        {
            output[i] = new double[N];
            for (int j = 0; j < N; j++)
            {
                output[i][j] = input[i][j] * quantizationMatrix[i][j];
            }
        }

        return output;
    }

    /// <summary>
    /// Invert quantization by casting input values.
    /// </summary>
    /// <param name="input">The input values to dequantize</param>
    /// <returns>The dequantized values</returns>
    public static double[] Dequantize(int[] input)
    {
        double[] output = new double[input.Length];

        for (int i = 0; i < input.Length; i++)
        {
            output[i] = (double)input[i];
        }

        return output;
    }

    /// <summary>
    /// Flattens a 2D array into a 1D array for run-length encoding
    /// </summary>
    /// <param name="input">The 2d data to flatten</param>
    /// <returns>Flattened data</returns>
    public static int[] Flatten(int[][] input)
    {
        return input.SelectMany(x => x).ToArray();
    }

    /// <summary>
    /// Reshapes a flattened array into a 2d array
    /// </summary>
    /// <param name="input">The input to reshape</param>
    /// <param name="blocksize">The blocksize of the reshaped data</param>
    /// <returns>The reshaped data</returns>
    public static int[][] Reshape(short[] input, int blocksize)
    {
        int size = input.Length / blocksize;
        int[][] output = new int[size][];

        for (int i = 0; i < size; i++)
        {
            output[i] = new int[blocksize];
            Array.Copy(input, i * blocksize, output[i], 0, blocksize);
        }

        return output;
    }

    /// <summary>
    /// Performs a naive encoding method to compress quantized data.
    /// </summary>
    /// <param name="input">Input quantized data to compress</param>
    /// <returns>The compressed data representing count, value</returns>
    public static List<(byte, short)> RunLengthEncode(int[] input)
    {
        List<(byte, short)> output = new List<(byte, short)>();

        byte runLength = 0;

        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == 0)
            {
                runLength++;
                if (runLength == 63) // Maximum run length before we must output a zero run length pair.
                {
                    output.Add((runLength, 0));
                    runLength = 0;
                }
            }
            else
            {
                output.Add((runLength, (short)input[i]));
                runLength = 0;
            }
        }

        output.Add((0, 0)); // Add EOB symbol

        return output;
    }

    /// <summary>
    /// Decodes run-length encoding compressed data
    /// </summary>
    /// <param name="input">The encoded data to decode</param>
    /// <returns>The decoded values</returns>
    public static short[] RunLengthDecode(List<(byte, short)> input, int blockSize)
    {
        short[] output = new short[blockSize];
        int index = 0;

        foreach ((byte runLength, short value) in input)
        {
            if (runLength == 0 && value == 0) // Check for EOB symbol
            {
                break; // All remaining coefficients are zero
            }

            index += runLength;

            // Make sure the index is within the bounds of the array
            if (index >= blockSize)
            {
                //Console.WriteLine("Warning: Run length exceeded block size.");
                index = blockSize - 1;
            }

            output[index] = value;

            // Check if index is less than blockSize - 1 before incrementing
            if (index < blockSize - 1)
            {
                index++;
            }
        }

        return output;
    }
}
