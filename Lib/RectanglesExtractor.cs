using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Lib
{
    public static class RectanglesExtractor
    {
        private const int AngleTolerance = 10;

        public static List<Bitmap> Extract(Bitmap image, byte[,] binarizedImage, List<int[]> lines)
        {
            var result = new List<Bitmap>();
            var horizontalLines = GetLinesByAngle(90, lines);
            var verticalLines = GetLinesByAngle(0, lines);

            var combinations = new List<int[]>();
            foreach (var topLine in horizontalLines)
            {
                foreach (var bottomLine in horizontalLines)
                {
                    if (topLine == bottomLine ||
                        lines[topLine][0] >= (lines[bottomLine][0] - 20))
                    {
                        continue;
                    }

                    foreach (var leftLine in verticalLines)
                    {
                        foreach (var rightLine in verticalLines)
                        {
                            if (leftLine == rightLine ||
                                lines[leftLine][0] >= (lines[rightLine][0] - 20))
                            {
                                continue;
                            }
                            combinations.Add(new [] {bottomLine, leftLine, topLine, rightLine} );
                        }
                    }
                }
            }

            var rectangles = new List<Rectangle>();
            foreach (var combination in combinations)
            {
                var bottomLine = lines[combination[0]];
                var leftLine = lines[combination[1]];
                var topLine = lines[combination[2]];
                var rightLine = lines[combination[3]];

                var bottomLeftCrossing = GetPointOfCrossing(bottomLine, leftLine, binarizedImage);
                var leftTopCrossing = GetPointOfCrossing(leftLine, topLine, binarizedImage);
                var topRightCrossing = GetPointOfCrossing(topLine, rightLine, binarizedImage);
                var rightBottomCrossing = GetPointOfCrossing(rightLine, bottomLine, binarizedImage);

                if (!IsValidLine(bottomLine, rightBottomCrossing, bottomLeftCrossing, binarizedImage, 0.95, 4) ||
                    !IsValidLine(leftLine, bottomLeftCrossing, leftTopCrossing, binarizedImage, 0.95, 4) ||
                    !IsValidLine(topLine, leftTopCrossing, topRightCrossing, binarizedImage, 0.95, 4) ||
                    !IsValidLine(rightLine, topRightCrossing, rightBottomCrossing, binarizedImage, 0.95, 4))
                {
                    continue;
                }

                var minX = new int[] { leftTopCrossing[0], bottomLeftCrossing[0] }.Min();
                minX = Math.Max(minX - 10, 0);
                var minY = new int[] { leftTopCrossing[1], bottomLeftCrossing[1] }.Min();
                minY = Math.Max(minY - 10, 0);

                var maxX = new int[] { rightBottomCrossing[0], topRightCrossing[0] }.Max();
                maxX = Math.Min(maxX + 10, binarizedImage.GetLength(1));
                var maxY = new int[] { rightBottomCrossing[1], topRightCrossing[1] }.Max();
                maxY = Math.Min(maxY + 10, binarizedImage.GetLength(0));

                rectangles.Add(new Rectangle(minX, minY, maxX - minX, maxY - minY));
            }

            var filteredRectangles = new List<Rectangle>();
            foreach (var rectangle in rectangles)
            {
                if (rectangles.Any(r => r != rectangle && r.Contains(rectangle)))
                {
                    continue;
                }
                filteredRectangles.Add(rectangle);
            }

            foreach (var rectangle in filteredRectangles)
            {
                result.Add(image.Clone(rectangle, image.PixelFormat));
            }

            return result;
        }

        private static List<int> GetLinesByAngle(int angle, List<int[]> lines)
        {
            var result = new List<int>();
            for (int i = 0; i < lines.Count; i++)
            {
                if (Math.Abs(lines[i][1] - angle) < AngleTolerance)
                {
                    result.Add(i);
                }
            }
            return result;
        }

        private static int[] GetPointOfCrossing(int[] firstLine, int[] secondLine, byte[,] binarizedImage)
        {
            var theta0 = GradusesToRadian(firstLine[1]);
            var theta1 = GradusesToRadian(secondLine[1]);

            var d0 = firstLine[0];
            var d1 = secondLine[0];

            var x = (d1 * Math.Sin(theta0) - d0 * Math.Sin(theta1)) /
                (Math.Cos(theta1) * Math.Sin(theta0) - Math.Cos(theta0) * Math.Sin(theta1));
            var y = (d0 - x*Math.Cos(theta0))/Math.Sin(theta0);

            if (Math.Abs(theta0) < 0.1)
            {
                y = d1;
            }

            return new []{ (int)x, (int)y };
        }

        private static double GradusesToRadian(int graduses)
        {
            return (graduses/180.0)*Math.PI;
        }

        private static bool IsValidLine(int[] line, int[] firstPoint, int[] secondPoint, byte[,] binarizedImage, double treshold, int radius)
        {
            if (firstPoint[0] < 0 ||
                firstPoint[1] < 0 ||
                secondPoint[0] < 0 ||
                secondPoint[1] < 0)
            {
                return false;
            }
            if (Math.Sqrt(Math.Pow(firstPoint[0] - secondPoint[0], 2) + Math.Pow(firstPoint[1] - secondPoint[1], 2)) <
                Math.Min(binarizedImage.GetLength(0), binarizedImage.GetLength(1)) / 10.0)
            {
                return false;
            }
            var theta = GradusesToRadian(line[1]);
            var triesCount = 0;
            var successCount = 0;
            if (Math.Abs(firstPoint[0] - secondPoint[0]) > 10)
            {
                for (var x = Math.Min(firstPoint[0], secondPoint[0]); x < Math.Max(firstPoint[0], secondPoint[0]); x++)
                {
                    triesCount++;
                    var y = (int) ((line[0] - x*Math.Cos(theta))/Math.Sin(theta));
                    if (IsSomethingNearPoint(binarizedImage, y, x, radius))
                    {
                        successCount++;
                    }
                }
            }
            else
            {
                for (var y = Math.Min(firstPoint[1], secondPoint[1]); y < Math.Max(firstPoint[1], secondPoint[1]); y++)
                {
                    triesCount++;
                    var x = (int)((line[0] - y * Math.Sin(theta)) / Math.Cos(theta));
                    if (IsSomethingNearPoint(binarizedImage, y, x, radius))
                    {
                        successCount++;
                    }
                }
            }

            return (successCount / (double) triesCount) >= treshold;
        }

        private static bool IsSomethingNearPoint(byte[,] image, int r, int c, int radius)
        {
            var sum = 0;
            for (var i = 0; i < radius; i++)
            {
                if (r >= image.GetLength(0) - 1 - i || c >= image.GetLength(1) - 1 - i || r < 0 || c < 0)
                {
                    continue;
                }
                sum += image[r, c] + image[r, c + i] + image[r + i, c] + image[r + i, c + i];
            }
            return sum > 0;
        }
    }
}
