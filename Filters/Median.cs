﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Filters
{
    public class Median : IFilter
    {
        private const int Radius = 2;

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
                var currentPixelWindow = new List<byte[]>(16);

                for (var y = 0; y < heightInPixels; y++)
                {
                    var ptrWindowBaseLines = new byte*[Radius * 2 + 1];
                    var index = 0;
                    for (var i = y - Radius; i <= y + Radius; i++)
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
                        currentPixelWindow.Clear();
                        //indexX and indexY - positions in pixels relative to current point
                        for (var indexY = -Radius; indexY <= Radius; indexY++)
                        {
                            if (ptrWindowBaseLines[indexY + Radius] == null)
                            {
                                continue;
                            }

                            var currentRow = ptrWindowBaseLines[indexY + Radius];

                            for (var indexX = -Radius; indexX <= Radius; indexX++)
                            {
                                if ((x + indexX) * bytesPerPixel >= 0
                                    && (x + indexX) * bytesPerPixel < widthInBytes
                                    && Math.Abs(indexX) + Math.Abs(indexY) <= Radius //rhombus
                                    )
                                {
                                    currentPixelWindow.Add(new[]
                                    {
                                        currentRow[(x + indexX) * bytesPerPixel + 2],
                                        currentRow[(x + indexX) * bytesPerPixel + 1],
                                        currentRow[(x + indexX) * bytesPerPixel]
                                    });
                                }
                            }
                        }

                        var newColor = GetNewColor(currentPixelWindow);
                        newRgbValues[y * widthInBytes + x * bytesPerPixel] = newColor[2];
                        newRgbValues[y * widthInBytes + x * bytesPerPixel + 1] = newColor[1];
                        newRgbValues[y * widthInBytes + x * bytesPerPixel + 2] = newColor[0];
                        newRgbValues[y * widthInBytes + x * bytesPerPixel + 3] = 255;
                    }
                }

                Marshal.Copy(newRgbValues, 0, (IntPtr)ptrFirstPixel, newRgbValues.Length);
                image.UnlockBits(bitmapData);
            }

            return image;
        }

        private static byte[] GetNewColor(List<byte[]> window)
        {
            var centralIndex = window.Count / 2;

            var redValues = new byte[window.Count];
            var greenValues = new byte[window.Count];
            var blueValues = new byte[window.Count];

            for (var i = 0; i < window.Count; i++)
            {
                var tmp = window[i];
                redValues[i] = tmp[0];
                greenValues[i] = tmp[1];
                blueValues[i] = tmp[2];
            }

            return new [] { GetNSmallest(redValues, centralIndex),
                GetNSmallest(greenValues, centralIndex),
                GetNSmallest(blueValues, centralIndex) };
        }

        private static byte GetNSmallest(byte[] arr, int n)
        {
            var tmp = new byte[256];
            foreach (var b in arr)
            {
                tmp[b]++;
            }

            var sum = 0;
            for (byte i = 0; i <= 255; i++)
            {
                sum += tmp[i];
                if (sum >= n)
                {
                    return i;
                }
            }
            return 0;
        }
    }
}
