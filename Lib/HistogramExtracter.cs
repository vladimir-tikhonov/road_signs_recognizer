using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Lib
{
    public static class HistogramExtracter
    {
        public static double[] Process(Bitmap image)
        {
            var horizontal = new int[image.Width];
            var vertical = new int[image.Height];

            unsafe
            {
                var bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);

                var bytesPerPixel = Image.GetPixelFormatSize(image.PixelFormat) / 8;
                var heightInPixels = bitmapData.Height;
                var widthInBytes = bitmapData.Width * bytesPerPixel;
                var ptrFirstPixel = (byte*)bitmapData.Scan0;

                Parallel.For((long)0, heightInPixels, y =>
                {
                    var currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                    for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        if (currentLine[x] != 0)
                        {
                            vertical[y]++;
                            horizontal[x/bytesPerPixel]++;
                        }
                    }
                });
                image.UnlockBits(bitmapData);
            }

            var horizontalRelative = horizontal.ToList().Select(x => (double) x/image.Height).ToList();
            var verticalRelative = vertical.ToList().Select(x => (double)x / image.Width).ToList();
            return horizontalRelative.Concat(verticalRelative).ToArray();
        }
    }
}
