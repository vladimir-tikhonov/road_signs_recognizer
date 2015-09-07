using System.Windows.Media.Imaging;

namespace GUI.Models
{
    class ImageModel
    {
        public ImageModel(BitmapSource bitmap)
        {
            Picture = bitmap;
        }

        public BitmapSource Picture { get; set; }
    }
}
