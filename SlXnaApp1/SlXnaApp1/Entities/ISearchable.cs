using System.Windows.Media.Imaging;
using System.Windows;

namespace SlXnaApp1.Entities
{
    public interface ISearchable
    {
        int Uid
        {
            get;
        }

        string FullName
        {
            get;
        }
    }
}
