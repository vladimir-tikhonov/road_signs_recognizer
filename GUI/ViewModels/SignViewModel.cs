using System.Collections.ObjectModel;
using GUI.Models;

namespace GUI.ViewModels
{
    class SignViewModel
    {
        public SignViewModel()
        {
            Signs = new ObservableCollection<SignModel>();
        }

        public ObservableCollection<SignModel> Signs { get; set; }
    }
}
