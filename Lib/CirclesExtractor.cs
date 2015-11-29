using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;

namespace Lib
{
    public static class CirclesExtractor
    {
        public static async Task<List<Bitmap>[]> Extract(Bitmap processedImage, Bitmap originalImage, List<int[]> circles, bool strip = true)
        {
            var extractedFromProcessed = new List<Bitmap>();
            var extractedFromOriginal = new List<Bitmap>();
            await Task.Run(() =>
            {
                foreach (var circleData in circles)
                {
                    var x = Math.Max(circleData[1] - circleData[2], 0);
                    var y = Math.Max(circleData[0] - circleData[2], 0);
                    var width = circleData[2] * 2;
                    var rect = new Rectangle(x, y, width, width);
                    var croppedImage = processedImage.Clone(rect, processedImage.PixelFormat);
                    var croppedOriginalImage = originalImage.Clone(rect, originalImage.PixelFormat);
                    if (strip)
                    {
                        croppedImage = StripImage(croppedImage, Color.Black);
                        croppedOriginalImage = StripImage(croppedOriginalImage, Color.Black, false);
                    }
                    extractedFromProcessed.Add(croppedImage);
                    extractedFromOriginal.Add(croppedOriginalImage);
                }
            });

            return new []{ extractedFromProcessed, extractedFromOriginal };
        }

        private static Bitmap StripImage(Bitmap bitmap, Color color, bool cutImage = true)
        {
            var result = bitmap.Clone() as Bitmap;
            var g = Graphics.FromImage(result);
            using (Brush br = new SolidBrush(color))
            {
                g.FillRectangle(br, 0, 0, bitmap.Width, bitmap.Height);
            }
            var path = new GraphicsPath();

            if (cutImage)
            {
                var offsetX = (float)(bitmap.Width * 0.85);
                var offsetY = (float)(bitmap.Height * 0.85);
                path.AddEllipse(offsetX, offsetY, bitmap.Width - 2*offsetX, bitmap.Width - 2*offsetY);
            }
            else
            {
                path.AddEllipse(0, 0, bitmap.Width, bitmap.Width);
            }
            g.SetClip(path);
            g.DrawImage(bitmap, 0, 0);
            return result;
        }
    }
}
