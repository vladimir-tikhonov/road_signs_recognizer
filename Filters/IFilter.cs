using System.Drawing;

namespace Filters
{
    public interface IFilter
    {
        Bitmap Process(Bitmap image);
    }
}
