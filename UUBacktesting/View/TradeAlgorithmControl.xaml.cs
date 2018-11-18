using UUBacktesting.ViewModel;
using BackTestingInterface;
using CommonLibForWPF;
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
    /// Interaction logic for TradeAlgorithmControl.xaml
    /// </summary>
    public partial class TradeAlgorithmControl : UserControl
    {
        public TradeAlgorithmControl()
        {
            InitializeComponent();
        }

        void OnDrop(List<ConditionViewModel> dl)
        {
            var dc = DataContext as TradeAlgorithmViewModel;
            var i = dl.FirstOrDefault();
            if (i != null)
            {
                dc.TargetSignal = i.Name;
            }
        }

        private void Condition_Drop(object sender, DragEventArgs e)
        {
            DragDropSupport.OnDrop<ConditionViewModel>(e, OnDrop);
            e.Handled = true;
        }

        void OnDrop(List<IInstrument> dl)
        {
            var dc = DataContext as TradeAlgorithmViewModel;
            var i = dl.FirstOrDefault();
            if (i != null)
            {
                dc.TargetInstrumentTicker = i.Ticker;
            }
        }
        private void Instrument_Drop(object sender, DragEventArgs e)
        {
            DragDropSupport.OnDrop<IInstrument>(e, OnDrop);
            e.Handled = true;
        }

        private void Object_Drop(object sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            DragDropSupport.OnDrop<IInstrument>(e, OnDrop);
            DragDropSupport.OnDrop<ConditionViewModel>(e, OnDrop);
            e.Handled = true;
        }
    }
}
