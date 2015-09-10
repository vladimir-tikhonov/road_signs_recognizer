using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;
using Lib;
using Color = System.Windows.Media.Color;

namespace Filters
{
    public class Trinarization : IFilter
    {
        private readonly LabColor _yellow = LabColor.FromRgb(255, 255, 0);
        private readonly LabColor _lightBlue = LabColor.FromRgb(22, 111, 193);
        private readonly LabColor _blue = LabColor.FromRgb(0, 0, 255);
        private readonly LabColor _red = LabColor.FromRgb(255, 0, 0);
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
            if (_yellow.DistanceTo(labColor) < ToleranceDistance ||
                _lightBlue.DistanceTo(labColor) < ToleranceDistance ||
                _blue.DistanceTo(labColor) < ToleranceDistance)
            {
                return Color.FromRgb(128, 128, 128);
            }
            else if (_red.DistanceTo(labColor) < ToleranceDistance)
            {
                return Color.FromRgb(0, 0, 0);
            }
            else if (labColor.L < 50)
            {
                return Color.FromRgb(0, 0, 0);
            }
            else
            {
                return Color.FromRgb(255, 255, 255);
            }
        }
    }
}
