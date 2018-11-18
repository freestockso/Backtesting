using UUBacktesting.ViewModel;
using BackTestingCommonLib;
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
    /// Interaction logic for StandardStrategyControl.xaml
    /// </summary>
    public partial class StandardStrategyControl : UserControl
    {
        public StandardStrategyControl()
        {
            InitializeComponent();
        }

        private void TradeAlgorithmDrop(object sender, DragEventArgs e)
        {
            DragDropSupport.OnDrop<IInstrument>(e, TradeAlgorithmOnDrop);
            DragDropSupport.OnDrop<ConditionViewModel>(e, TradeAlgorithmOnDrop);
        }
        void TradeAlgorithmOnDrop(List<IInstrument> dl)
        {
            var dc = DataContext as StandardStrategyViewModel;
            var i = dl.FirstOrDefault();
            if (i != null)
            {
                var tg = new TradeAlgorithm() { TargetInstrumentTicker=i.Ticker};
                var vm = new TradeAlgorithmViewModel(tg);
                
                dc.TradeAlgorithmList.Add(vm);
                dc.TargetObject.TradeAlgorithmList.Add(tg);
            }
                

        }
        void TradeAlgorithmOnDrop(List<ConditionViewModel> dl)
        {
            var dc = DataContext as StandardStrategyViewModel;
            var i = dl.FirstOrDefault();
            if (i != null)
            {
                var tg = new TradeAlgorithm() { TargetSignalName = i.Name };
                var vm = new TradeAlgorithmViewModel(tg);

                dc.TradeAlgorithmList.Add(vm);
                dc.TargetObject.TradeAlgorithmList.Add(tg);
            }


        }
    }
}
