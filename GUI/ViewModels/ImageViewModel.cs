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

            WarningSigns = new ObservableCollection<ImageModel>();
            ProhibitingSigns = new ObservableCollection<ImageModel>();
            RegulatorySigns = new ObservableCollection<ImageModel>();
            InformationSigns = new ObservableCollection<ImageModel>();
            TemporarySigns = new ObservableCollection<ImageModel>();
        }

        public ObservableCollection<ImageModel> Images { get; set; }

        public ObservableCollection<ImageModel> Circles { get; set; }
        public ObservableCollection<ImageModel> Rectangles { get; set; }
        public ObservableCollection<ImageModel> Triangles { get; set; }

        public ObservableCollection<ImageModel> WarningSigns { get; set; }
        public ObservableCollection<ImageModel> ProhibitingSigns { get; set; }
        public ObservableCollection<ImageModel> RegulatorySigns { get; set; }
        public ObservableCollection<ImageModel> InformationSigns { get; set; }
        public ObservableCollection<ImageModel> TemporarySigns { get; set; }
    }
}
