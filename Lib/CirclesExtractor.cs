using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace Lib
{
    public static class CirclesExtractor
    {
        public static List<Bitmap> Extract(Bitmap image, List<int[]> circles, bool strip = true)
        {
            var result = new List<Bitmap>();
            foreach (var circleData in circles)
            {
                var x = Math.Max(circleData[1] - circleData[2], 0);
                var y = Math.Max(circleData[0] - circleData[2], 0);
                var width = circleData[2] * 2;
                var rect = new Rectangle(x, y, width, width);
                var croppedImage = image.Clone(rect, image.PixelFormat);
                if (strip)
                {
                    croppedImage = StripImage(croppedImage);
                }
                result.Add(croppedImage);
            }
            return result;
        }

        private static Bitmap StripImage(Bitmap bitmap)
        {
            var result = bitmap.Clone() as Bitmap;
            var g = Graphics.FromImage(result);
            using (Brush br = new SolidBrush(Color.Black))
            {
                g.FillRectangle(br, 0, 0, bitmap.Width, bitmap.Height);
            }
            var path = new GraphicsPath();
            var offsetX = (float)(bitmap.Width * 0.85);
            var offsetY = (float)(bitmap.Height * 0.85);
            path.AddEllipse(offsetX, offsetY, bitmap.Width - 2 * offsetX, bitmap.Width - 2 * offsetY);
            g.SetClip(path);
            g.DrawImage(bitmap, 0, 0);
            return result;
        }
    }
}
