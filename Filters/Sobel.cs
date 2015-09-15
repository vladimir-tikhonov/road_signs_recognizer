using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Filters
{
    public class Sobel : IFilter
    {
        private readonly Color _default = Color.FromArgb(255, 0, 0, 0);

        public Bitmap Process(Bitmap image)
        {
            unsafe
            {
                var bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadWrite, image.PixelFormat);

                var bytesPerPixel = Image.GetPixelFormatSize(image.PixelFormat) / 8;
                var heightInPixels = bitmapData.Height;
                var widthInBytes = bitmapData.Width * bytesPerPixel;
                var ptrFirstPixel = (byte*)bitmapData.Scan0;

                var newRGBValues = new byte[bitmapData.Width * bitmapData.Height * bytesPerPixel];

                Parallel.For(0, heightInPixels, y =>
                {
                    var ptrWindowBaseLines = new byte*[3];
                    var index = 0;
                    for (var i = y - 1; i <= y + 1; i++)
                    {
                        if (i >= 0 && i < heightInPixels)
                        {
                            ptrWindowBaseLines[index] = ptrFirstPixel + i * bitmapData.Stride;
                        }

                        index++;
                    }

                    for (var x = 0; x < widthInBytes; x = x + bytesPerPixel)
                    {
                        Color[][] currentPixelWindow = new Color[3][];

                        for (var i = 0; i < ptrWindowBaseLines.Length; i++)
                        {
                            if (ptrWindowBaseLines[i] == null)
                            {
                                currentPixelWindow[i] = Enumerable.Repeat(_default, 3).ToArray();
                                continue;
                            }

                            currentPixelWindow[i] = new Color[3];

                            currentPixelWindow[i][0] = (x - bytesPerPixel >= 0) ? Color.FromArgb(ptrWindowBaseLines[i][x - bytesPerPixel + 3],
                                ptrWindowBaseLines[i][x - bytesPerPixel + 2], ptrWindowBaseLines[i][x - bytesPerPixel + 1],
                                ptrWindowBaseLines[i][x - bytesPerPixel]) : _default;

                            currentPixelWindow[i][1] = Color.FromArgb(ptrWindowBaseLines[i][x + 3], ptrWindowBaseLines[i][x + 2],
                                ptrWindowBaseLines[i][x + 1], ptrWindowBaseLines[i][x]);

                            currentPixelWindow[i][2] = (x + bytesPerPixel < widthInBytes) ? Color.FromArgb(ptrWindowBaseLines[i][x + bytesPerPixel + 3],
                                ptrWindowBaseLines[i][x + bytesPerPixel + 2], ptrWindowBaseLines[i][x + bytesPerPixel + 1],
                                ptrWindowBaseLines[i][x + bytesPerPixel]) : _default;
                        }

                        Color newColor = GetNewColor(currentPixelWindow);
                        newRGBValues[y * widthInBytes + x] = newColor.B;
                        newRGBValues[y * widthInBytes + x + 1] = newColor.G;
                        newRGBValues[y * widthInBytes + x + 2] = newColor.R;
                        newRGBValues[y * widthInBytes + x + 3] = newColor.A;
                    }
                });

                Marshal.Copy(newRGBValues, 0, (IntPtr)ptrFirstPixel, newRGBValues.Length);
                image.UnlockBits(bitmapData);
            }

            return image;
        }

        private Color GetNewColor(Color[][] window)
        {
            var greenGx = (window[2][0].G + 2 * window[2][1].G + window[2][2].G) - (window[0][0].G + 2 * window[0][1].G + window[0][2].G);
            var greenGy = (window[0][2].G + 2 * window[1][2].G + window[2][2].G) - (window[0][0].G + 2 * window[1][0].G + window[2][0].G);
            var greenG = (int)Math.Sqrt(greenGx * greenGx + greenGy * greenGy);

            var redGx = (window[2][0].R + 2 * window[2][1].R + window[2][2].R) - (window[0][0].R + 2 * window[0][1].R + window[0][2].R);
            var redGy = (window[0][2].R + 2 * window[1][2].R + window[2][2].R) - (window[0][0].R + 2 * window[1][0].R + window[2][0].R);
            var redG = (int)Math.Sqrt(redGx * redGx + redGy * redGy);

            var blueGx = (window[2][0].B + 2 * window[2][1].B + window[2][2].B) - (window[0][0].B + 2 * window[0][1].B + window[0][2].B);
            var blueGy = (window[0][2].B + 2 * window[1][2].B + window[2][2].B) - (window[0][0].B + 2 * window[1][0].B + window[2][0].B);
            var blueG = (int)Math.Sqrt(blueGx * blueGx + blueGy * blueGy);

            if (greenG < 0) { greenG = 0; }
            if (greenG > 255) { greenG = 255; }

            if (redG < 0) { redG = 0; }
            if (redG > 255) { redG = 255; }

            if (blueG < 0) { blueG = 0; }
            if (blueG > 255) { blueG = 255; }

            return Color.FromArgb(window[1][1].A, redG, greenG, blueG);
        }
    }
}
