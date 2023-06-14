using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BrowerCosineTransform;

internal class BitmapHelper
{
    /// <summary>
    /// Converts a bitmap into a tuple consisting of constitutent RGB channels.
    /// </summary>
    /// <param name="bitmap">The bitmap to convert</param>
    /// <returns>The constituent RGB channels</returns>
    public static (double[][], double[][], double[][]) BitmapToChannels(Bitmap bitmap)
    {
        int width = bitmap.Width;
        int height = bitmap.Height;

        double[][] red = new double[height][];
        double[][] green = new double[height][];
        double[][] blue = new double[height][];

        for (int i = 0; i < height; i++)
        {
            red[i] = new double[width];
            green[i] = new double[width];
            blue[i] = new double[width];

            for (int j = 0; j < width; j++)
            {
                Color pixel = bitmap.GetPixel(j, i);

                red[i][j] = pixel.R;
                green[i][j] = pixel.G;
                blue[i][j] = pixel.B;
            }
        }

        return (red, green, blue);
    }

    /// <summary>
    /// Converts RGB channel data into a bitmap
    /// </summary>
    /// <param name="red">The red channel</param>
    /// <param name="green">The blue channel</param>
    /// <param name="blue">The green channel</param>
    /// <returns>The constructed bitmap</returns>
    public static Bitmap ChannelsToBitmap(double[][] red, double[][] green, double[][] blue)
    {
        int height = red.Length;
        int width = red[0].Length;

        Bitmap bitmap = new Bitmap(width, height);

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                int r = (int)Math.Round(red[i][j]);
                int g = (int)Math.Round(green[i][j]);
                int b = (int)Math.Round(blue[i][j]);

                // Clamp the values to the range [0, 255] 
                r = Math.Max(0, Math.Min(255, r));
                g = Math.Max(0, Math.Min(255, g));
                b = Math.Max(0, Math.Min(255, b));

                Color pixel = Color.FromArgb(r, g, b);

                bitmap.SetPixel(j, i, pixel);
            }
        }

        return bitmap;
    }

    /// <summary>
    /// Gets 8x8 blocks for a given image based on width and height
    /// </summary>
    /// <param name="data">The data to deconstruct</param>
    /// <param name="width">The width of the image</param>
    /// <param name="height">The height of the image</param>
    /// <returns>The deconstructed blocks</returns>
    public static List<double[][]> GetBlocks(double[][] data, int width, int height)
    {
        var blocks = new List<double[][]>();

        for (int i = 0; i < height; i += 8)
        {
            for (int j = 0; j < width; j += 8)
            {
                var block = new double[8][];
                for (int x = 0; x < 8; x++)
                {
                    block[x] = new double[8];
                    for (int y = 0; y < 8; y++)
                    {
                        // If the coordinates are outside the image boundaries, pad with 0
                        block[x][y] = (i + x < height && j + y < width) ? data[i + x][j + y] : 0.0;
                    }
                }
                blocks.Add(block);
            }
        }

        return blocks;
    }

    /// <summary>
    /// Recombines blocks into a single image based on width and height
    /// </summary>
    /// <param name="blocks">The blocks to recombine</param>
    /// <param name="width">The width of the image to reconstruct</param>
    /// <param name="height">The height of the image to reconstruct</param>
    /// <returns>The reconstructed image data</returns>
    public static double[][] CombineBlocks(List<double[][]> blocks, int width, int height)
    {
        var data = new double[height][];
        for (int i = 0; i < height; i++)
        {
            data[i] = new double[width];
        }

        int blockIndex = 0;
        for (int i = 0; i < height; i += 8)
        {
            for (int j = 0; j < width; j += 8)
            {
                var block = blocks[blockIndex++];
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        if (i + x < height && j + y < width)
                        {
                            data[i + x][j + y] = block[x][y];
                        }
                    }
                }
            }
        }

        return data;
    }
}
