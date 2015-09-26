using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Filters
{
    public class Median : IFilter
    {
        private readonly Color _default = Color.FromArgb(255, 0, 0, 0);
        private readonly int _radius = 2;

        public Bitmap Process(Bitmap image)
        {
            unsafe
            {
                var bitmapData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height),
                    ImageLockMode.ReadWrite, image.PixelFormat);

                var bytesPerPixel = Image.GetPixelFormatSize(image.PixelFormat) / 8;
                var heightInPixels = bitmapData.Height;
                var widthInPixels = bitmapData.Width;
                var widthInBytes = bitmapData.Width * bytesPerPixel;
                var ptrFirstPixel = (byte*)bitmapData.Scan0;

                var newRgbValues = new byte[bitmapData.Width * bitmapData.Height * bytesPerPixel];

                for (int y = 0; y < heightInPixels; y++)
                {
                    var ptrWindowBaseLines = new byte*[_radius * 2 + 1];
                    var index = 0;
                    for (var i = y - _radius; i <= y + _radius; i++)
                    {
                        if (i >= 0 && i < heightInPixels)
                        {
                            ptrWindowBaseLines[index] = ptrFirstPixel + i * bitmapData.Stride;
                        }
                        else
                        {
                            ptrWindowBaseLines[index] = null;
                        }

                        index++;
                    }

                    for (var x = 0; x < widthInPixels; x++)
                    {
                        var currentPixelWindow = new List<Color>();
                        
                        //indexX and indexY - positions in pixels relative to current point
                        for (var indexY = -_radius; indexY <= _radius; indexY++)
                        {
                            if (ptrWindowBaseLines[indexY + _radius] == null)
                            {
                                continue;
                            }

                            var currentRow = ptrWindowBaseLines[indexY + _radius];

                            for (int indexX = -_radius; indexX <= _radius; indexX++)
                            {
                                if ((x + indexX) * bytesPerPixel >= 0
                                    && Math.Abs(indexX) + Math.Abs(indexY) <= _radius //rhombus
                                    )
                                {
                                    currentPixelWindow.Add(Color.FromArgb(
                                        alpha: currentRow[(x + indexX) * bytesPerPixel + 3],
                                        red: currentRow[(x + indexX) * bytesPerPixel + 2],
                                        green: currentRow[(x + indexX) * bytesPerPixel + 1],
                                        blue: currentRow[(x + indexX) * bytesPerPixel]));
                                }
                            }
                        }

                        Color newColor = GetNewColor(currentPixelWindow);
                        newRgbValues[y * widthInBytes + x * bytesPerPixel] = newColor.B;
                        newRgbValues[y * widthInBytes + x * bytesPerPixel + 1] = newColor.G;
                        newRgbValues[y * widthInBytes + x * bytesPerPixel + 2] = newColor.R;
                        newRgbValues[y * widthInBytes + x * bytesPerPixel + 3] = newColor.A;
                    }
                }

                Marshal.Copy(newRgbValues, 0, (IntPtr)ptrFirstPixel, newRgbValues.Length);
                image.UnlockBits(bitmapData);
            }

            return image;
        }

        private Color GetNewColor(List<Color> window)
        {
            int centralIndex = window.Count / 2;

            var alphaValues = new byte[window.Count];
            var redValues = new byte[window.Count];
            var greenValues = new byte[window.Count];
            var blueValues = new byte[window.Count];

            for (int i = 0; i < window.Count; i++)
            {
                alphaValues[i] = window[i].A;
                redValues[i] = window[i].R;
                greenValues[i] = window[i].G;
                blueValues[i] = window[i].B;
            }

            Array.Sort(alphaValues, (emp1, emp2) => emp1.CompareTo(emp2));
            Array.Sort(redValues, (emp1, emp2) => emp1.CompareTo(emp2));
            Array.Sort(greenValues, (emp1, emp2) => emp1.CompareTo(emp2));
            Array.Sort(blueValues, (emp1, emp2) => emp1.CompareTo(emp2));

            var newColor = Color.FromArgb(
                alpha: alphaValues[centralIndex],
                red: redValues[centralIndex],
                green: greenValues[centralIndex],
                blue: blueValues[centralIndex]
                );

            return newColor;
        }

    }
}
