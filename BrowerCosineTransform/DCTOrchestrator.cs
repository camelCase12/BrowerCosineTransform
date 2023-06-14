using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace BrowerCosineTransform;

internal class DCTOrchestrator
{
    /// <summary>
    /// Represents the width/height of each block being encoded/decoded
    /// </summary>
    static int BLOCK_SIZE = 8;

    /// <summary>
    /// Compresses a bitmap image into a tuple of lists of blocks of run-length encoded DCT coefficients
    /// </summary>
    /// <param name="Image">The image to compress</param>
    /// <returns>The run-length encoded DCT coefficients</returns>
    public static EncodedImage CompressImage(Bitmap image)
    {
        (double[][], double[][], double[][]) rgbChannels = BitmapHelper.BitmapToChannels(image);

        int rawByteage = rgbChannels.Item1.Length * rgbChannels.Item1.Length * 3;
        Console.WriteLine($"Raw image data byteage: {rawByteage}");

        List<double[][]> redBlocks = BitmapHelper.GetBlocks(rgbChannels.Item1, image.Width, image.Height);
        List<double[][]> greenBlocks = BitmapHelper.GetBlocks(rgbChannels.Item2, image.Width, image.Height);
        List<double[][]> blueBlocks = BitmapHelper.GetBlocks(rgbChannels.Item3, image.Width, image.Height);

        List<List<(byte, short)>> encodedRedBlocks = redBlocks.Select(x => DCTOrchestrator.Run2DDCTPipeline(x)).ToList();
        List<List<(byte, short)>> encodedGreenBlocks = greenBlocks.Select(x => DCTOrchestrator.Run2DDCTPipeline(x)).ToList();
        List<List<(byte, short)>> encodedBlueBlocks = blueBlocks.Select(x => DCTOrchestrator.Run2DDCTPipeline(x)).ToList();

        int redSize = encodedRedBlocks.SelectMany(list => list).Aggregate(0, (sum, tuple) => sum + sizeof(byte) + sizeof(short) * 2);
        int greenSize = encodedGreenBlocks.SelectMany(list => list).Aggregate(0, (sum, tuple) => sum + sizeof(byte) + sizeof(short) * 2);
        int blueSize = encodedBlueBlocks.SelectMany(list => list).Aggregate(0, (sum, tuple) => sum + sizeof(byte) + sizeof(short) * 2);

        int encodedByteage = redSize + greenSize + blueSize;
        Console.WriteLine($"Encoded channel byteage: {encodedByteage}");

        Console.WriteLine($"Approximate compression ratio: {(1 - (float)encodedByteage / (float)rawByteage) * 100}%");

        return new EncodedImage() { RedDCTCoefficientBlocks = encodedRedBlocks, GreenDCTCoefficientBlocks = encodedGreenBlocks, BlueDCTCoefficientBlocks = encodedBlueBlocks };
    }

    /// <summary>
    /// Recovers an image from the encoded DCT coefficient block tuple
    /// </summary>
    /// <param name="encodedBlockListTuple">The data source to recover image from</param>
    /// <returns>The recovered image</returns>
    public static Bitmap RecoverImage(EncodedImage encodedBlockListTuple, int width, int height)
    {
        List<double[][]> recoveredRedBlocks = encodedBlockListTuple.RedDCTCoefficientBlocks.Select(x => DCTOrchestrator.RecoverEncoded2DData(x)).ToList();
        List<double[][]> recoveredGreenBlocks = encodedBlockListTuple.GreenDCTCoefficientBlocks.Select(x => DCTOrchestrator.RecoverEncoded2DData(x)).ToList();
        List<double[][]> recoveredBlueBlocks = encodedBlockListTuple.BlueDCTCoefficientBlocks.Select(x => DCTOrchestrator.RecoverEncoded2DData(x)).ToList();

        (double[][], double[][], double[][]) recoveredChannels = new(
        BitmapHelper.CombineBlocks(recoveredRedBlocks, width, height),
        BitmapHelper.CombineBlocks(recoveredGreenBlocks, width, height),
        BitmapHelper.CombineBlocks(recoveredBlueBlocks, width, height));

        Bitmap result = BitmapHelper.ChannelsToBitmap(recoveredChannels.Item1, recoveredChannels.Item2, recoveredChannels.Item3);

        return result;
    }
    
    /// <summary>
    /// Runs the pipeline to compress data using 2D DCT
    /// </summary>
    /// <param name="data">The data to compress</param>
    /// <returns>Compressed data</returns>
    public static List<(byte, short)> Run2DDCTPipeline(double[][] data)
    {
        double[][] dctCoefficients = DiscreteCosineTransform.DiscreteTransform2D(data);
        int[][] quantizedData = DiscreteCosineTransform.Quantize(dctCoefficients);
        int[] flattenedData = DiscreteCosineTransform.Flatten(quantizedData);
        List<(byte, short)> runLengthEncodedData = DiscreteCosineTransform.RunLengthEncode(flattenedData);
        return runLengthEncodedData;
    }

    /// <summary>
    /// Recovers encoded/compressed 2D DCT data
    /// </summary>
    /// <param name="data">The compressed data to recover from</param>
    /// <returns>The recovered data</returns>
    public static double[][] RecoverEncoded2DData(List<(byte, short)> data)
    {
        short[] runLengthDecodedData = DiscreteCosineTransform.RunLengthDecode(data, BLOCK_SIZE*BLOCK_SIZE);
        int[][] reshapedData = DiscreteCosineTransform.Reshape(runLengthDecodedData, 8);
        double[][] dequantizedData = DiscreteCosineTransform.Dequantize(reshapedData);
        double[][] recoveredData = DiscreteCosineTransform.GetInverseDiscreteCosineTransform2D(dequantizedData);
        return recoveredData;
    }
}
