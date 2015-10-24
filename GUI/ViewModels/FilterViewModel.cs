using System.Collections.ObjectModel;
using Filters;
using GUI.Models;

namespace GUI.ViewModels
{
    class FilterViewModel
    {
        public FilterViewModel()
        {
            Filters = new ObservableCollection<FilterModel>
            {
                new FilterModel(new Median(), "Медианный фильтр", false),
                new FilterModel(new Sobel(), "Собеля"),
                new FilterModel(new Binarization(), "Бинаризация"),
            };
        }

        public ObservableCollection<FilterModel> Filters { get; set; }
    }
}
