using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
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
        private const int GroupsCount = 5;
        private const int ClassesCount = 5;
        
        private const int ResizeWidth = 150;
        private const int ResizeHeight = 150;

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
                BilinearInterpolation.Resize(element.Value, ResizeWidth, ResizeHeight)   
                    .Save(element.Key + ".processed.bmp", ImageFormat.Bmp);
            }
        }

        private void Teach_Click(object sender, RoutedEventArgs e)
        {
            var teachingData = new List<List<List<double[]>>>();
            for (var i = 0; i < GroupsCount; i++)
            {
                var groupsContainer = new List<List<double[]>>();
                for (var j = 0; j < ClassesCount; j++)
                {
                    groupsContainer.Add(new List<double[]>());
                }
                teachingData.Add(groupsContainer);
            }

            var dialog = new FolderBrowserDialog
            {
                SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures)
            };
            var result = dialog.ShowDialog(this.GetIWin32Window());
            
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                foreach (var path in Directory.EnumerateFiles(dialog.SelectedPath, "*.*", SearchOption.AllDirectories)
                    .Where(file =>  file.EndsWith(".processed.bmp", StringComparison.OrdinalIgnoreCase)))
                {
                    var bitmap = BitmapConverter.GetBitmap(new BitmapImage(new Uri(path)));
                    var histogram = HistogramExtracter.Process(bitmap);
                    var tmp = path.Split('\\').ToList();
                    tmp.Reverse();
                    var currentGroup = int.Parse(tmp[2]) - 1;
                    var currentClass = int.Parse(tmp[1]) - 1;
                    teachingData[currentGroup][currentClass].Add(histogram);
                }
            }

            var perceptrons = new List<Perceptron>();
            perceptrons.AddRange((Enumerable.Repeat(new Perceptron(), GroupsCount)));
            Parallel.ForEach(teachingData, groupData =>
            {
                var teachingSet = new Dictionary<double[], double[]>();
                for (var i = 0; i < groupData.Count; i++)
                {
                    var classData = groupData[i];
                    var expectedOutput = new double[groupData.Count];
                    expectedOutput[i] = 1;
                    foreach (var classInput in classData)
                    {
                        teachingSet.Add(classInput, expectedOutput);
                    }
                }
                var perceptron = new Perceptron();
                perceptron.Reset();
                perceptron.Teach(teachingSet);
                var index = teachingData.IndexOf(groupData);
                perceptrons[index] = perceptron;
            });
            var formatter = new BinaryFormatter();
            Stream stream = new FileStream(dialog.SelectedPath + "\\perceptrons.bin", FileMode.Create, FileAccess.Write, FileShare.None);
            formatter.Serialize(stream, perceptrons);
            stream.Close();
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
            PerformClassification(circleBitmaps, trianglesBitmaps, rectanglesBitmaps, viewModel);
            pbStatus.Value = 100;
        }

        private void PerformClassification(List<Bitmap>[] circles, List<Bitmap>[] triangles, List<Bitmap>[] rectangles, ImageViewModel viewModel)
        {
            var signData = new Dictionary<Bitmap[], Sign>();

            for (int i = 0; i < circles[1].Count; i++)
            {
                var circle = circles[1][i];
                var colorInfo = ColorInfo.Extract(circle);
                signData.Add(new[] { circles[0][i] , circles[1][i] }, new Sign(100, colorInfo[0], colorInfo[1], colorInfo[2], colorInfo[3]));
            }
            for (int i = 0; i < triangles[1].Count; i++)
            {
                var triangle = triangles[1][i];
                var colorInfo = ColorInfo.Extract(triangle);
                signData.Add(new[] { triangles[0][i], triangles[1][i] }, new Sign(50, colorInfo[0], colorInfo[1], colorInfo[2], colorInfo[3]));
            }
            for (int i = 0; i < rectangles[1].Count; i++)
            {
                var rectangle = rectangles[1][i];
                var colorInfo = ColorInfo.Extract(rectangle);
                signData.Add(new[] { rectangles[0][i], rectangles[1][i] }, new Sign(150, colorInfo[0], colorInfo[1], colorInfo[2], colorInfo[3]));
            }

            var containers = new[]
            {
                viewModel.WarningSigns, viewModel.ProhibitingSigns, viewModel.RegulatorySigns, viewModel.InformationSigns,
                viewModel.TemporarySigns
            };

            var formatter = new BinaryFormatter();
            Stream stream = new FileStream("..\\..\\..\\perceptrons.bin", FileMode.Open, FileAccess.Read, FileShare.None);
            var perceptrons = (List<Perceptron>)formatter.Deserialize(stream);
            stream.Close();

            var tc = new PrebuildSignsClassifier();
            tc.Teach();
            var signsViewModel = (SignsGrid.DataContext) as SignViewModel;
            signsViewModel.Signs.Clear();

            foreach (var sign in signData)
            {
                var groupIndex = tc.FindClass(sign.Value) - 1;
                var signsImageSource = BitmapConverter.GetBitmapSource(sign.Key[1]);
                containers[groupIndex].Add(new ImageModel(signsImageSource));
                var perceptron = perceptrons[groupIndex];
                var histogram =
                    HistogramExtracter.Process(BilinearInterpolation.Resize(sign.Key[0], ResizeWidth, ResizeHeight));
                var classificationResult = perceptron.Classify(histogram).ToList();
                var classIndex = classificationResult.IndexOf(classificationResult.Max());
                var description = SignDescription.Get(groupIndex, classIndex);
                signsViewModel.Signs.Add(new SignModel(signsImageSource, description));

            }
        }
    }
}
