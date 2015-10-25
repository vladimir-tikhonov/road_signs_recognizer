using System;
using System.Collections.Generic;
using System.Drawing;

namespace Lib
{
    public static class TrianglesExtractor
    {
        private const int AngleTolerance = 5;

        public static List<Bitmap> Extract(Bitmap image, byte[,] binarizedImage, List<int[]> lines)
        {
            var result = new List<Bitmap>();

            return result;
        }

        private static List<int> GetLinesByAngle(int index, int angle, List<int[]> lines)
        {
            var result = new List<int>();
            for (int i = 0; i < lines.Count; i++)
            {
                if (index == i)
                {
                    continue;
                }
                if (Math.Abs(lines[index][1] - lines[i][1] - angle) < AngleTolerance)
                {
                    result.Add(i);
                }
            }
            return result;
        }
    }
}
