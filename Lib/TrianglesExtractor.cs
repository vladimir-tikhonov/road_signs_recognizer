using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;

namespace Lib
{
    public static class TrianglesExtractor
    {
        private const int AngleTolerance = 10;

        public static async Task<List<Bitmap>[]> Extract(Bitmap processedImage, Bitmap originalImage, byte[,] binarizedImage, List<int[]> lines, bool strip = true)
        {
            var extractedFromProcessed = new List<Bitmap>();
            var extractedFromOriginal = new List<Bitmap>();

            await Task.Run(() =>
            {
                var combinations = new List<int[]>();
                var bottomLines = GetLinesByAngle(90, lines);
                var leftLines = GetLinesByAngle(30, lines);
                var rightLines = GetLinesByAngle(150, lines).Concat(GetLinesByAngle(330, lines)).ToList();

                foreach (var bottomLine in bottomLines)
                {
                    foreach (var leftLine in leftLines)
                    {
                        combinations.AddRange(rightLines.Select(rightLine => new[] { bottomLine, leftLine, rightLine }));
                    }
                }

                var rectangles = new List<Rectangle>();

                foreach (var combination in combinations)
                {
                    var firstLine = lines[combination[0]];
                    var secondtLine = lines[combination[1]];
                    var thirdLine = lines[combination[2]];

                    var firstCrossing = GetPointOfCrossing(firstLine, secondtLine);
                    var secondCrossing = GetPointOfCrossing(firstLine, thirdLine);
                    var thirdCrossing = GetPointOfCrossing(secondtLine, thirdLine);

                    if (!IsValidLine(firstLine, firstCrossing, secondCrossing, binarizedImage, 0.98, 2) ||
                        !IsValidLine(secondtLine, firstCrossing, thirdCrossing, binarizedImage, 0.7, 5) ||
                        !IsValidLine(thirdLine, secondCrossing, thirdCrossing, binarizedImage, 0.7, 5))
                    {
                        continue;
                    }

                    var minX = new[] { firstCrossing[0], secondCrossing[0], thirdCrossing[0] }.Min();
                    var minY = new[] { firstCrossing[1], secondCrossing[1], thirdCrossing[1] }.Min();

                    var maxX = new[] { firstCrossing[0], secondCrossing[0], thirdCrossing[0] }.Max();
                    var maxY = new[] { firstCrossing[1], secondCrossing[1], thirdCrossing[1] }.Max();

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
                    var croppedImage = processedImage.Clone(rectangle, processedImage.PixelFormat);
                    var croppedOriginalImage = originalImage.Clone(rectangle, originalImage.PixelFormat);
                    if (strip)
                    {
                        croppedImage = StripImage(croppedImage, Color.Black);
                        croppedOriginalImage = StripImage(croppedOriginalImage, Color.Black, false);
                    }
                    extractedFromProcessed.Add(croppedImage);
                    extractedFromOriginal.Add(croppedOriginalImage);
                }
            });

            return new[] { extractedFromProcessed, extractedFromOriginal };
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
                var offsetX = (int)(bitmap.Width * 0.10);
                var offsetY = (int)(bitmap.Height * 0.10);
                path.AddLines(new[] { new Point(offsetX, bitmap.Height - offsetY), new Point(bitmap.Width / 2, offsetY), new Point(bitmap.Width - offsetX, bitmap.Height - offsetY) });
            }
            else
            {
                path.AddLines(new[] { new Point(0, bitmap.Height), new Point(bitmap.Width / 2, 0), new Point(bitmap.Width, bitmap.Height) });
            }
            
            path.CloseAllFigures();
            g.SetClip(path);
            g.DrawImage(bitmap, 0, 0);
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

        private static int[] GetPointOfCrossing(int[] firstLine, int[] secondLine)
        {
            var theta0 = GradusesToRadian(firstLine[1]);
            var theta1 = GradusesToRadian(secondLine[1]);

            var d0 = firstLine[0];
            var d1 = secondLine[0];

            var x = (d1 * Math.Sin(theta0) - d0 * Math.Sin(theta1)) /
                (Math.Cos(theta1) * Math.Sin(theta0) - Math.Cos(theta0) * Math.Sin(theta1));
            var y = (d0 - x*Math.Cos(theta0))/Math.Sin(theta0);

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
            for (int x = Math.Min(firstPoint[0], secondPoint[0]); x < Math.Max(firstPoint[0], secondPoint[0]); x++)
            {
                triesCount++;
                var y = (int)((line[0] - x * Math.Cos(theta)) / Math.Sin(theta));
                if (IsSomethingNearPoint(binarizedImage, y, x, radius))
                {
                    successCount++;
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
