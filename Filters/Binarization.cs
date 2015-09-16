using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using Lib;
using Color = System.Windows.Media.Color;

namespace Filters
{
    public class Binarization : IFilter
    {
        private readonly LabColor _black = LabColor.FromRgb(0, 0, 0);
        private const int ToleranceDistance = 40;

        public Bitmap Process(Bitmap image)
        {
            unsafe
            {
                var bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadWrite, image.PixelFormat);

                var bytesPerPixel = Image.GetPixelFormatSize(image.PixelFormat) / 8;
                var heightInPixels = bitmapData.Height;
                var widthInBytes = bitmapData.Width * bytesPerPixel;
                var ptrFirstPixel = (byte*)bitmapData.Scan0;

                Parallel.For(0, heightInPixels, y =>
                {
                    var currentLine = ptrFirstPixel + (y * bitmapData.Stride);
                    for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        var newColor = GetNewColor(currentLine[x + 2], currentLine[x + 1], currentLine[x]);

                        currentLine[x] = newColor.B;
                        currentLine[x + 1] = newColor.G;
                        currentLine[x + 2] = newColor.R;
                    }
                });
                image.UnlockBits(bitmapData);
            }
            return image;
        }

        private Color GetNewColor(byte r, byte g, byte b)
        {
            var labColor = LabColor.FromRgb(r, g, b);
            return _black.DistanceTo(labColor) < ToleranceDistance ?
                Color.FromRgb(0, 0, 0) : Color.FromRgb(255, 255, 255);
        }
    }
}
