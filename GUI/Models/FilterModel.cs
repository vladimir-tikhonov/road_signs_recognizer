using Filters;

namespace GUI.Models
{
    class FilterModel
    {
        public FilterModel(IFilter filter, string name, bool enabled = true)
        {
            Filter = filter;
            Name = name;
            Enabled = enabled;
        }

        public IFilter Filter { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; }
    }
}
