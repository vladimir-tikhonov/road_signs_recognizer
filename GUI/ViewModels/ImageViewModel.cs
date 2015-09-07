using System.Collections.ObjectModel;
using GUI.Models;

namespace GUI.ViewModels
{
    class ImageViewModel
    {
        public ImageViewModel()
        {
            Images = new ObservableCollection<ImageModel>();
        }

        public ObservableCollection<ImageModel> Images { get; set; }
    }
}
