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
    /// Interaction logic for ConditionControl.xaml
    /// </summary>
    public partial class ConditionControl : UserControl
    {
        public ConditionControl()
        {
            InitializeComponent();
        }

        private void Instrument_Drop(object sender, DragEventArgs e)
        {
            DragDropSupport.OnDrop<IInstrument>(e, onDrop);
        }

        void onDrop(List<IInstrument> dl)
        {
            var dc = DataContext as ConditionViewModel;
            var i = dl.FirstOrDefault();
            if (i != null)
                dc.AnalyseInstrument = i;

        }
    }
}
