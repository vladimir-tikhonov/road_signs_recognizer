using System.Collections.ObjectModel;
using Filters;
using GUI.Models;

namespace GUI.ViewModels
{
    class FilterViewModel
    {
        public FilterViewModel()
        {
            Filters = new ObservableCollection<FilterModel> {new FilterModel(new Trinarization(), "Тринаризация")};
        }

        public ObservableCollection<FilterModel> Filters { get; set; }
    }
}
