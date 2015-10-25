using System.Collections.ObjectModel;
using GUI.Models;

namespace GUI.ViewModels
{
    class ImageViewModel
    {
        public ImageViewModel()
        {
            Images = new ObservableCollection<ImageModel>();
            Circles = new ObservableCollection<ImageModel>();
            Rectangles = new ObservableCollection<ImageModel>();
            Triangles = new ObservableCollection<ImageModel>();
        }

        public ObservableCollection<ImageModel> Images { get; set; }
        public ObservableCollection<ImageModel> Circles { get; set; }
        public ObservableCollection<ImageModel> Rectangles { get; set; }
        public ObservableCollection<ImageModel> Triangles { get; set; }
    }
}
