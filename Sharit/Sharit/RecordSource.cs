using System.ComponentModel;
using System.IO;
using Xamarin.Forms;

namespace Sharit
{
    public class RecordSource: INotifyPropertyChanged
    {
        public string Name { get; set; }
        public int Id { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public ImageSource Image { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetImage(MemoryStream stream)
        {
            Image = ImageSource.FromStream(() =>
            {
                return stream;
            });
        }
    }
}
