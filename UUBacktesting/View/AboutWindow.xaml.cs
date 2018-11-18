using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace UUBacktesting.View
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
            DataContext = new AboutViewModel();
        }
    }

    class AboutViewModel
    {
        public string WorkPath
        {
            get { return System.AppDomain.CurrentDomain.BaseDirectory; }
        }

        public DateTime LastBuildTime
        {
            get
            {
                return System.IO.File.GetLastWriteTime(GetType().Assembly.Location);
            }
        }
    }
}
