using System;
using System.Collections.Generic;
using System.Drawing;
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

        private void ListBox_PreviewMouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
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

            ExtractParts(bitmap, originalBitmap);
            FilteredImage.Source = BitmapConverter.GetBitmapSource(bitmap);
        }

        private void ExtractParts(Bitmap bitmap, Bitmap originalBitmap)
        {
            var binarizedImage = BitmapBinarizer.Process(bitmap);
            var lines = HoughTransform.GetLines(binarizedImage);
            var circles = HoughTransform.GetCircles(binarizedImage);
            var strip = DoPerformStrip.IsChecked;

            var viewModel = (ImagesGrid.DataContext) as ImageViewModel;
            if (viewModel == null)
            {
                return;
            }

            var circleBitmaps = CirclesExtractor.Extract(bitmap, originalBitmap, circles, strip);
            viewModel.Circles.Clear();
            foreach (var circlesBitmap in circleBitmaps[0])
            {
                viewModel.Circles.Add(new ImageModel(BitmapConverter.GetBitmapSource(circlesBitmap)));
            }

            var trianglesBitmaps = TrianglesExtractor.Extract(bitmap, originalBitmap, binarizedImage, lines, strip);
            viewModel.Triangles.Clear();
            foreach (var triangleBitmap in trianglesBitmaps[0])
            {
                viewModel.Triangles.Add(new ImageModel(BitmapConverter.GetBitmapSource(triangleBitmap)));
            }

            var rectanglesBitmaps = RectanglesExtractor.Extract(bitmap, originalBitmap, binarizedImage, lines, strip);
            viewModel.Rectangles.Clear();
            foreach (var recangleBitmap in rectanglesBitmaps[0])
            {
                viewModel.Rectangles.Add(new ImageModel(BitmapConverter.GetBitmapSource(recangleBitmap)));
            }

            PerformClassification(circleBitmaps[1], trianglesBitmaps[1], rectanglesBitmaps[1]);
        }

        private void PerformClassification(List<Bitmap> circles, List<Bitmap> triangles, List<Bitmap> rectangles)
        {
            foreach (var circle in circles)
            {
                var tmp = ColorInfo.Extract(circle);
            }
            foreach (var triangle in triangles)
            {
                var tmp = ColorInfo.Extract(triangle);
            }
            foreach (var rectangle in rectangles)
            {
                var tmp = ColorInfo.Extract(rectangle);
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
