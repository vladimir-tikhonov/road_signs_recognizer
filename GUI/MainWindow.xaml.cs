using System;
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
        private readonly string[] _imageFileExtensions = {"jpg", "png", "bmp"};

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
            if (image != null)
            {
                var bitmap = BitmapConverter.GetBitmap(image.Source as BitmapSource);
                bitmap = (from object filterObject in FiltersMenu.ItemsSource
                          select (filterObject as FilterModel) into filterModel
                          where filterModel.Enabled select filterModel.Filter)
                          .Aggregate(bitmap, (current, filter) => filter.Process(current));
                FilteredImage.Source = BitmapConverter.GetBitmapSource(bitmap);
            }
        }
    }
}
