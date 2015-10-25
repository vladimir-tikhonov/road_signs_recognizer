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
            bitmap = (from object filterObject in FiltersMenu.ItemsSource
                select (filterObject as FilterModel) into filterModel
                where filterModel.Enabled select filterModel.Filter)
                .Aggregate(bitmap, (current, filter) => filter.Process(current)); 

            FilteredImage.Source = BitmapConverter.GetBitmapSource(bitmap);
            ExtractParts(bitmap);
        }

        private void ExtractParts(Bitmap bitmap)
        {
            var binarizedImage = BitmapBinarizer.Process(bitmap);
            var lines = HoughTransform.GetLines(binarizedImage);
            var circles = HoughTransform.GetCircles(binarizedImage);

            var viewModel = (ImagesGrid.DataContext) as ImageViewModel;
            if (viewModel == null)
            {
                return;
            }

            var circleBitmaps = CirclesExtracter.Extract(bitmap, circles);
            viewModel.Circles.Clear();
            foreach (var circlesBitmap in circleBitmaps)
            {
                viewModel.Circles.Add(new ImageModel(BitmapConverter.GetBitmapSource(circlesBitmap)));
            }
            // DrawLinesOnBitmap(bitmap, lines);
        }

        // TODO: Remove this
        private void DrawLinesOnBitmap(Bitmap bitmap, List<int[]> lines)
        {
            using (var graphics = Graphics.FromImage(bitmap))
            {
                var pen = new Pen(Color.Yellow, 3);
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
