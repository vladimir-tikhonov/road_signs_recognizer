using System.Drawing;
using System.Drawing.Imaging;
using System.Threading.Tasks;

namespace Lib
{
    public static class ColorInfo
    {
        private const int ToleranceDistance = 40;

        private static readonly LabColor Red = LabColor.FromRgb(255, 0, 0);
        private static readonly LabColor White = LabColor.FromRgb(255, 255, 255);
        private static readonly LabColor Yellow = LabColor.FromRgb(255, 204, 0);
        private static readonly LabColor Blue = LabColor.FromRgb(13, 105, 225);

        public static int[] Extract(Bitmap image)
        {
            int redPx = 0, whitePx = 0, yellowPx = 0, bluePx = 0, totalPx = 0;
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
                        var labColor = LabColor.FromRgb(currentLine[x + 2], currentLine[x + 1], currentLine[x]);
                        if (Red.DistanceTo(labColor) < ToleranceDistance)
                        {
                            redPx++;
                        }
                        else if (White.DistanceTo(labColor) < ToleranceDistance)
                        {
                            whitePx++;
                        }
                        else if (Yellow.DistanceTo(labColor) < ToleranceDistance)
                        {
                            yellowPx++;
                        }
                        else if (Blue.DistanceTo(labColor) < ToleranceDistance)
                        {
                            bluePx++;
                        }
                        totalPx++;
                    }
                });
                image.UnlockBits(bitmapData);
            }
            return new[] {redPx*100/totalPx, whitePx*100/totalPx, yellowPx*100/totalPx, bluePx*100/totalPx};
        }
    }
}
