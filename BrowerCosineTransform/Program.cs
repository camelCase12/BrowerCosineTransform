using System.Drawing;
using System.Drawing.Imaging;

namespace BrowerCosineTransform;

internal class Program
{
    static void Main(string[] args)
    {
        Bitmap image = new Bitmap(Image.FromFile("./flower.png"));

        EncodedImage encodedChannels = DCTOrchestrator.CompressImage(image);

        Bitmap result = DCTOrchestrator.RecoverImage(encodedChannels, image.Width, image.Height);

        result.Save("output.jpg", ImageFormat.Jpeg);
    }
}