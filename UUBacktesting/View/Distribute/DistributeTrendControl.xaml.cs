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
using UUBacktesting.ViewModel.Distribute;

namespace UUBacktesting.View.Distribute
{
    /// <summary>
    /// Interaction logic for DistributeTrendControl.xaml
    /// </summary>
    public partial class DistributeTrendControl : UserControl
    {
        public DistributeTrendControl()
        {
            InitializeComponent();
            DataContext = new DistributeTrendViewModel() { Chart=MainChart};
        }
    }
}
