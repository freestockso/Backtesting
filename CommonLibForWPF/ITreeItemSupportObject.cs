using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace CommonLibForWPF
{
    public interface ITreeItemSupportObject
    {
        string Name { get; set; }

        string Memo { get; set; }

        BitmapImage Icon { get; set; }

        IObservable<ITreeItemSupportObject> SubItems { get; }
    }
}
