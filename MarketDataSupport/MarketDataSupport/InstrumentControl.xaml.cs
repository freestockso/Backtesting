using BackTestingInterface;
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

namespace MarketDataSupport
{
    /// <summary>
    /// Interaction logic for InstrumentControl.xaml
    /// </summary>
    public partial class InstrumentControl : UserControl
    {
        public InstrumentControl()
        {
            InitializeComponent();
        }
        public static void ShowInstrumentInfo(string title, List<IMarketData> dataList)
        {
            var w = new Window();
            var vm = new InstrumentViewModel() { IsEnableLoad = false,StartTime=dataList.Min(v=>v.Time), EndTime=dataList.Max(v=>v.Time)};
            dataList.ForEach(v => vm.MarketDataList.Add(v));
            var c = new InstrumentControl() { DataContext = vm };
            w.Content = c;
            w.Title = title;

            w.Show();
        }

    }
}
