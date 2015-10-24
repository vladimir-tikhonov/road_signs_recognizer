using System;
using System.Collections.Generic;

namespace Lib
{
    public static class HoughTransform
    {
        private const double GradientTreshold = 3.5;
        private const int DistQuintizeTreshold = 5;

        public static List<int[]> GetLines(byte[,] image)
        {
            var result = new List<int[]>();
            var a = new int[image.Length / 2, 360];

            for (var r = 0; r < image.GetLength(0); r++)
            {
                for (var c = 0; c < image.GetLength(1); c++)
                {
                    var dr = RowGradient(image, r, c);
                    var dc = ColGradient(image, r, c);
                    var gmag = Gradient(dr, dc);

                    if (gmag > GradientTreshold)
                    {
                        var theta = Math.Atan2(dr, dc);
                        if (theta < 0)
                        {
                            theta += Math.PI;
                        }
                        var thetaq = (int) RadianToDegree(theta);
                        var d = (int) Math.Abs(c*Math.Cos(theta) + r*Math.Sin(theta));
                        var dq = QuantizeDistance(d);
                        a[dq, thetaq]++;
                    }
                }
            }
            for (var r = 0; r < a.GetLength(0); r++)
            {
                for (var c = 0; c < a.GetLength(1); c++)
                {
                    if (a[r, c] > 50)
                    {
                        if (IsLocalMaximum(a, r, c))
                        {
                            result.Add(new[] {r, c});
                        }  
                    }
                }
            }
            return result;
        }

        private static int RowGradient(byte[,] image, int r, int c)
        {
            if (r == 0 || r == image.GetLength(0) - 1 || c == 0 || c == image.GetLength(1) - 1)
            {
                return 0;
            }
            return -1*image[r - 1, c - 1] + -2*image[r - 1, c] + -1*image[r - 1, c + 1] +
                    1*image[r + 1, c - 1] +  2*image[r + 1, c] +  1*image[r + 1, c + 1];
        }

        private static int ColGradient(byte[,] image, int r, int c)
        {
            if (r == 0 || r == image.GetLength(0) - 1 || c == 0 || c == image.GetLength(1) - 1)
            {
                return 0;
            }
            return -1 * image[r - 1, c - 1] + 1 * image[r - 1, c + 1] +
                   -2 * image[r,     c - 1] + 2 * image[r    , c + 1] +
                   -1 * image[r + 1, c - 1] + 1 * image[r + 1, c + 1];
        }

        private static double Gradient(int rowGradient, int colGradient)
        {
            return Math.Sqrt(rowGradient*rowGradient + colGradient*colGradient);
        }

        private static double RadianToDegree(double angle)
        {
            if (angle < 0)
            {
                angle = -angle;
            }
            return angle * (180.0 / Math.PI);
        }

        private static int QuantizeDistance(int distance)
        {
            var rest = distance % DistQuintizeTreshold;
            if (rest > DistQuintizeTreshold / 2 + 1)
            {
                return distance + DistQuintizeTreshold - rest;
            }
            return distance - rest;
        }

        private static bool IsLocalMaximum(int[,] a, int r, int c)
        {
            var value = a[r, c];
            var testedDistances = new List<int>();
            for (var i = 1; i <= 3; i++)
            {
                var upperTestedDistance = r - i*DistQuintizeTreshold;
                if (upperTestedDistance >= 0)
                {
                    testedDistances.Add(upperTestedDistance);
                }

                var lowerTestedDistance = r + i * DistQuintizeTreshold;
                if (lowerTestedDistance <= a.GetLength(0))
                {
                    testedDistances.Add(lowerTestedDistance);
                }
            }
            foreach (var testedDistance in testedDistances)
            {
                if (a[testedDistance, c] > value && testedDistance > r)
                {
                    return false;
                }
                if (a[testedDistance, c] >= value && testedDistance <= r)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
