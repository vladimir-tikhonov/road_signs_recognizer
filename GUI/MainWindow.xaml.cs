using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using GUI.Extensions;
using GUI.Models;
using GUI.ViewModels;
using Lib;
using Application = System.Windows.Application;
using Image = System.Windows.Controls.Image;
using Classifier;

namespace GUI
{
    public partial class MainWindow
    {
        private readonly string[] _imageFileExtensions = {"jpg", "png", "bmp", "jpeg"};

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AppExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void OpenDialog_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog
            {
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };
            var result = dialog.ShowDialog(this.GetIWin32Window());
            var viewModel = new ImageViewModel();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var model in Directory.EnumerateFiles(dialog.SelectedPath, "*.*")
                    .Where(file => _imageFileExtensions.Any(x => file.EndsWith(x, StringComparison.OrdinalIgnoreCase)))
                    .Select(filePath => new ImageModel(new BitmapImage(new Uri(filePath)))))
                {
                    viewModel.Images.Add(model);
                }
                ImagesGrid.DataContext = viewModel;
            }
        }

        private async void ProcessFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog
            {
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };
            var result = dialog.ShowDialog(this.GetIWin32Window());

            var storage = new Dictionary<string, Bitmap>();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var path in Directory.EnumerateFiles(dialog.SelectedPath, "*.*", SearchOption.AllDirectories)
                    .Where(file => _imageFileExtensions.Any(x => file.EndsWith(x, StringComparison.OrdinalIgnoreCase))))
                {
                    var bitmap = BitmapConverter.GetBitmap(new BitmapImage(new Uri(path)));
                    bitmap = (from object filterObject in FiltersMenu.ItemsSource
                              select (filterObject as FilterModel) into filterModel
                              where filterModel.Enabled
                              select filterModel.Filter)
                    .Aggregate(bitmap, (current, filter) => filter.Process(current));

                    var binarizedImage = BitmapBinarizer.Process(bitmap);
                    var linesTask = HoughTransform.GetLines(binarizedImage);
                    var circlesTask = HoughTransform.GetCircles(binarizedImage);

                    var rectanglesBitmaps = await RectanglesExtractor.Extract(bitmap, bitmap, binarizedImage, await linesTask, true);
                    var trianglesBitmaps = await TrianglesExtractor.Extract(bitmap, bitmap, binarizedImage, await linesTask, true);
                    var circleBitmaps = await CirclesExtractor.Extract(bitmap, bitmap, await circlesTask, true);

                    bitmap =
                        rectanglesBitmaps[0].Concat(trianglesBitmaps[0])
                            .Concat(circleBitmaps[0])
                            .OrderBy(b => b.Width*b.Height).Last();
                    storage.Add(path, bitmap);
                }
            }

            foreach (var element in storage)
            {
                BilinearInterpolation.Resize(element.Value, 150, 150)
                    .Save(element.Key + ".processed.bmp", ImageFormat.Bmp);
            }
        }

        private void ListBox_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            pbStatus.Value = 0;
            var image = e.OriginalSource as Image;
            if (image == null)
            {
                return;
            }

            var bitmap = BitmapConverter.GetBitmap(image.Source as BitmapSource);
            var originalBitmap = bitmap.Clone() as Bitmap;
            bitmap = (from object filterObject in FiltersMenu.ItemsSource
                select (filterObject as FilterModel) into filterModel
                where filterModel.Enabled select filterModel.Filter)
                .Aggregate(bitmap, (current, filter) => filter.Process(current));

            pbStatus.Value = 10;
            ExtractParts(bitmap, originalBitmap);
            FilteredImage.Source = BitmapConverter.GetBitmapSource(bitmap);
        }

        private async void ExtractParts(Bitmap bitmap, Bitmap originalBitmap)
        {
            var binarizedImage = BitmapBinarizer.Process(bitmap);
            var linesTask = HoughTransform.GetLines(binarizedImage);
            var circlesTask = HoughTransform.GetCircles(binarizedImage);
            var strip = DoPerformStrip.IsChecked;

            var viewModel = (ImagesGrid.DataContext) as ImageViewModel;
            if (viewModel == null)
            {
                return;
            }
            viewModel.Clear();

            var rectanglesBitmapsTask = RectanglesExtractor.Extract(bitmap, originalBitmap, binarizedImage, await linesTask, strip);
            pbStatus.Value = 40;
            var trianglesBitmapsTask = TrianglesExtractor.Extract(bitmap, originalBitmap, binarizedImage, await linesTask, strip);
            pbStatus.Value = 60;
            var circleBitmapsTask = CirclesExtractor.Extract(bitmap, originalBitmap, await circlesTask, strip);
            pbStatus.Value = 80;

            var rectanglesBitmaps = await rectanglesBitmapsTask;
            foreach (var rectangleBitmap in rectanglesBitmaps[0])
            {
                viewModel.Rectangles.Add(new ImageModel(BitmapConverter.GetBitmapSource(rectangleBitmap)));
            }

            var trianglesBitmaps = await trianglesBitmapsTask;
            foreach (var triangleBitmap in trianglesBitmaps[0])
            {
                viewModel.Triangles.Add(new ImageModel(BitmapConverter.GetBitmapSource(triangleBitmap)));
            }

            var circleBitmaps = await circleBitmapsTask;
            foreach (var circlesBitmap in circleBitmaps[0])
            {
                viewModel.Circles.Add(new ImageModel(BitmapConverter.GetBitmapSource(circlesBitmap)));
            }
            PerformClassification(circleBitmaps[1], trianglesBitmaps[1], rectanglesBitmaps[1], viewModel);
            pbStatus.Value = 100;
        }

        private void PerformClassification(List<Bitmap> circles, List<Bitmap> triangles, List<Bitmap> rectangles, ImageViewModel viewModel)
        {
            var signData = new Dictionary<Bitmap, Sign>();

            foreach (var circle in circles)
            {
                var colorInfo = ColorInfo.Extract(circle);
                signData.Add(circle, new Sign(100, colorInfo[0], colorInfo[1], colorInfo[2], colorInfo[3]));
            }
            foreach (var triangle in triangles)
            {
                var colorInfo = ColorInfo.Extract(triangle);
                signData.Add(triangle, new Sign(50, colorInfo[0], colorInfo[1], colorInfo[2], colorInfo[3]));
            }
            foreach (var rectangle in rectangles)
            {
                var colorInfo = ColorInfo.Extract(rectangle);
                signData.Add(rectangle, new Sign(150, colorInfo[0], colorInfo[1], colorInfo[2], colorInfo[3]));
            }

            var containers = new[]
            {
                null, viewModel.WarningSigns, viewModel.ProhibitingSigns, viewModel.RegulatorySigns, viewModel.InformationSigns,
                viewModel.TemporarySigns
            };

            var tc = new PrebuildSignsClassifier();
            tc.Teach();
            foreach (var sign in signData)
            {
                var classIndex = tc.FindClass(sign.Value);
                var resizedImage = BilinearInterpolation.Resize(sign.Key, 150, 150);
                containers[classIndex].Add(new ImageModel(BitmapConverter.GetBitmapSource(resizedImage)));
            }
        }

        // TODO: Remove this
        private void DrawLinesOnBitmap(Bitmap bitmap, List<int[]> lines)
        {
            using (var graphics = Graphics.FromImage(bitmap))
            {
                var pen = new Pen(Color.Yellow, 1);
                foreach (var line in lines)
                {
                    if (line[1] <= 90)
                    {
                        var xStart = line[0] / (float)Math.Cos(line[1] * (Math.PI / 180.0));
                        float yStart = 0;
                        float xEnd = 0;
                        var yEnd = line[0] / (float)Math.Cos((90 - line[1]) * (Math.PI / 180.0));
                        if (line[1] == 0)
                        {
                            yEnd = bitmap.Height;
                            xEnd = xStart;
                        }
                        if (line[1] == 90)
                        {
                            xStart = 0;
                            yStart = yEnd;
                            xEnd = bitmap.Width;
                        }
                        graphics.DrawLine(pen, xStart, yStart, xEnd, yEnd);
                    }
                }
            }
        }
    }
}
