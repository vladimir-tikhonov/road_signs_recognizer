using System;
using System.Collections.Generic;
using System.Linq;

namespace Lib
{
    public static class HoughTransform
    {
        private const double GradientTreshold = 3.0;
        private const int DistQuintizeTreshold = 4;

        public static List<int[]> GetLines(byte[,] image)
        {
            var result = new List<int[]>();
            var a = new int[image.Length / 2, 360];

            for (var r = 2; r < image.GetLength(0) - 2; r++)
            {
                for (var c = 2; c < image.GetLength(1) - 2; c++)
                {
                    var dr = RowGradient(image, r, c);
                    var dc = ColGradient(image, r, c);
                    var gmag = Gradient(dr, dc);

                    if (gmag > GradientTreshold)
                    {
                        var theta = Math.Atan2(dr, dc);
                        if (theta < 0)
                        {
                            theta += 2 * Math.PI;
                        }
                        var thetaq = (int) RadianToDegree(theta);
                        var d = (int) Math.Abs(c*Math.Cos(theta) + r*Math.Sin(theta));
                        var dq = d;
                        a[dq, thetaq]++;
                    }
                }
            }
            for (var r = 0; r < a.GetLength(0); r++)
            {
                for (var c = 0; c < a.GetLength(1); c++)
                {
                    if (a[r, c] > 10)
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

        public static List<int[]> GetCircles(byte[,] image)
        {
            var result = new List<int[]>();
            var points = new int[image.GetLength(0), image.GetLength(1)];
            var radiuses = new List<int>[image.GetLength(0), image.GetLength(1)];

            for (var r = 0; r < image.GetLength(0); r++)
            {
                for (var c = 0; c < image.GetLength(1); c++)
                {
                    var dr = RowGradient(image, r, c);
                    var dc = ColGradient(image, r, c);
                    var gmag = Gradient(dr, dc);

                    if (gmag > GradientTreshold)
                    {
                        var theta = Math.Atan2(dc, dr);
                        if (theta < 0)
                        {
                            theta += 2 * Math.PI;
                        }
                        var tan = Math.Tan(theta);
                        if (tan > 1000 || Math.Abs(tan) < 0.5)
                        {
                            continue; // whatever
                        }

                        for (var a = 0; a < image.GetLength(0); a++)
                        {
                            var b = (int) (a*tan - r*tan + c);
                            b = b % 2 == 0 ? b : b - 1;
                            if (b < 0 || b >= image.GetLength(1))
                            {
                                continue;
                            }
                            points[a, b]++;
                            if (radiuses[a, b] == null)
                            {
                                radiuses[a, b] = new List<int>();
                            }
                            radiuses[a, b].Add((int)Math.Sqrt(Math.Pow(r - a, 2) + Math.Pow(c - b, 2)));
                        }
                    }
                }
            }
            var pointsCount = new List<int>();
            for (var r = 0; r < points.GetLength(0); r++)
            {
                for (var c = 0; c < points.GetLength(1); c++)
                {                   
                    if (points[r, c] > 10)
                    {
                        pointsCount.Add(points[r, c]);
                    }
                }
            }

            var distinctPoints = pointsCount.Distinct().ToList();
            var treshold = 0;
            if (distinctPoints.Count > 0)
            {
                treshold = distinctPoints.OrderByDescending(p => p).ElementAt(distinctPoints.Count / 2);
            }          
            for (var r = 0; r < points.GetLength(0); r++)
            {
                for (var c = 0; c < points.GetLength(1); c++)
                {
                    if (points[r, c] > treshold)
                    {
                        radiuses[r, c] = GetPossibleRadiuses(image, radiuses[r, c], r, c);
                    }
                    else
                    {
                        radiuses[r, c] = new List<int>();
                        points[r, c] = 0;
                    }
                }
            }

            for (var r = 0; r < points.GetLength(0); r++)
            {
                for (var c = 0; c < points.GetLength(1); c++)
                {
                    if (points[r, c] > 0)
                    {
                        if (radiuses[r, c].Count > 0)
                        {
                            var radius = radiuses[r, c].Max();
                            if (IsCircleLocalMaximum(radiuses, radius, result, r, c))
                            {
                                result.Add(new[] { r, c, radius });
                            }                           
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
            for (var i = 1; i < 2 * DistQuintizeTreshold; i++)
            {
                var upperTestedDistance = r - i;
                if (upperTestedDistance >= 0)
                {
                    testedDistances.Add(upperTestedDistance);
                }

                var lowerTestedDistance = r + i;
                if (lowerTestedDistance <= a.GetLength(0))
                {
                    testedDistances.Add(lowerTestedDistance);
                }
            }
            foreach (var testedDistance in testedDistances)
            {
                if (a[testedDistance, c] > value)
                {
                    return false;
                }
            }
            return true;
        }

        private static List<int> GetPossibleRadiuses(byte[,] image, List<int> radiuses , int r, int c)
        {
            radiuses = radiuses.Distinct().ToList();
            var result = new List<int>();
            foreach (var radius in radiuses)
            {
                if (radius < 30)
                {
                    continue;
                }
                var flag = true;
                for (double i = 0; i < Math.PI * 2; i += 0.1)
                {
                    var dr = (int)(radius * Math.Sin(i));
                    var dc = (int)(radius * Math.Cos(i));
                    if (!IsSomethingNearPoint(image, r + dr, c + dc))
                    {
                        flag = false;
                        break;
                    }
                }
                if (flag)
                {
                    result.Add(radius);
                }         
            }
            return result;
        }

        private static bool IsSomethingNearPoint(byte[,] image, int r, int c)
        {
            var sum = 0;
            for (var i = 0; i < 5; i++)
            {
                if (r >= image.GetLength(0) - 1 - i || c >= image.GetLength(1) - 1 - i || r < 0 || c < 0)
                {
                    continue;
                }
                sum += image[r, c] + image[r, c + i] + image[r + i, c] + image[r + i, c + i];
            }
            return sum > 0;
        }

        private static bool IsCircleLocalMaximum(List<int>[,] radiuses, int radius, List<int[]> currentCenters, int r, int c)
        {
            if (currentCenters.Any(center => (int)Math.Sqrt(Math.Pow(r - center[0], 2) + Math.Pow(c - center[1], 2)) < radius))
            {
                return false;
            }
            for (var i = 0; i < radius; i++)
            {
                if (r >= radiuses.GetLength(0) - 1 - i || c >= radiuses.GetLength(1) - 1 - i || r < 0 || c < 0)
                {
                    continue;
                }
                if (radiuses[r, c + i].Concat(new[] {int.MinValue}).Max() > radiuses[r, c].Max() ||
                    radiuses[r + i, c].Concat(new[] { int.MinValue }).Max() > radiuses[r, c].Max() ||
                    radiuses[r + i, c + i].Concat(new[] { int.MinValue }).Max() > radiuses[r, c].Max())
                {
                    return false;
                }
            }
            return true;
        }
    }
}
