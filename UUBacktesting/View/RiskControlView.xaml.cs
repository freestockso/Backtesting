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
    /// Interaction logic for RiskControlView.xaml
    /// </summary>
    public partial class RiskControlView : UserControl
    {
        public RiskControlView()
        {
            InitializeComponent();
        }

        private void RiskControlDrop(object sender, DragEventArgs e)
        {
            DragDropSupport.OnDrop<IInstrument>(e, RiskControlOnDrop);
        }
        void RiskControlOnDrop(List<IInstrument> dl)
        {
            var dc = DataContext as RiskControlViewModel;
            var i = dl.FirstOrDefault();
            if (i != null)
            {
                var tg = new PositionControl() { TargetInstrumentTicker = i.Ticker };
                dc.PositionControlList.Add(tg);
                dc.TargetObject.PositionControlList.Add(tg);
            }

        }
    }
}
