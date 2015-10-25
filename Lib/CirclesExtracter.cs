using System;
using System.Collections.Generic;
using System.Drawing;

namespace Lib
{
    public static class CirclesExtracter
    {
        public static List<Bitmap> Extract(Bitmap image, List<int[]> circles)
        {
            var result = new List<Bitmap>();
            foreach (var circleData in circles)
            {
                var x = Math.Max(circleData[1] - circleData[2], 0);
                var y = Math.Max(circleData[0] - circleData[2], 0);
                var width = circleData[2] * 2;
                var rect = new Rectangle(x, y, width, width);
                result.Add(image.Clone(rect, image.PixelFormat));
            }
            return result;
        }
    }
}
