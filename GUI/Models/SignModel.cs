using System.Windows.Media.Imaging;

namespace GUI.Models
{
    class SignModel
    {
        public SignModel(BitmapSource bitmap, string description)
        {
            Picture = bitmap;
            Description = description;
        }

        public BitmapSource Picture { get; set; }
        public string Description { get; set; }
    }
}
