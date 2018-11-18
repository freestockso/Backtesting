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
    /// Interaction logic for AnalyseProjectControl.xaml
    /// </summary>
    public partial class AnalyseProjectControl : UserControl
    {
        public AnalyseProjectControl()
        {
            InitializeComponent();
        }

        private void RadGridView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dc = DataContext as AnalyseProjectViewModel;
            if (dc.CurrentResult != null&& dc.CurrentResult.TargetObject!=null)
            {
                InstrumentControl.ShowInstrumentInfo(dc.CurrentResult.TargetObject,()=> { return dc.CurrentDataSource; }, dc.AnalyseStartTime, dc.AnalyseEndTime);

            }
        }

        private void Instrument_Drop(object sender, DragEventArgs e)
        {
            DragDropSupport.OnDrop<IInstrument>(e, onDrop);
        }
        void onDrop(List<IInstrument> dl)
        {
            var dc = DataContext as AnalyseProjectViewModel;
            dl.ForEach(v =>
            {
                if (!dc.BlockList.Any(i => i.Ticker == v.Ticker))
                {
                    var iv = new InstrumentViewModel() { TargetObject = v.Clone() as IInstrument , GetCurrentDataSource = () => { return dc.CurrentDataSource; } };
                    dc.BlockList.Add(iv);

                }
            });
        }
    }
}
