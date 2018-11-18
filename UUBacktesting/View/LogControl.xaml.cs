using UUBacktesting.ViewModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace UUBacktesting.View
{
    /// <summary>
    /// Interaction logic for LogControl.xaml
    /// </summary>
    public partial class LogControl : UserControl
    {
        public LogControl()
        {
            InitializeComponent();
            var dc = new LogViewModel();
            dc.Refresh();
            dc.StartObserveViewModel();
            DataContext = dc;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            var dc = DataContext as LogViewModel;
            dc.StopObserveViewModel();
        }
    }
}
