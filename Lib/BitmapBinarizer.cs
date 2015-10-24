using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace Lib
{
    public static class BitmapBinarizer
    {
        public static byte[,] Process(Bitmap image)
        {
            byte[,] result;
            unsafe
            {
                var bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);

                var bytesPerPixel = Image.GetPixelFormatSize(image.PixelFormat) / 8;
                var heightInPixels = bitmapData.Height;
                var widthInBytes = bitmapData.Width * bytesPerPixel;
                var ptrFirstPixel = (byte*)bitmapData.Scan0;
                result = new byte[heightInPixels, widthInBytes / bytesPerPixel];

                Parallel.For((long) 0, heightInPixels, y =>
                {
                    var currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                    for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        var b = (byte) (currentLine[x] == 0 ? 0 : 1);
                        result[y, x / bytesPerPixel] = b;

                    }
                });
                image.UnlockBits(bitmapData);
            }

            return result;
        }
    }
}
