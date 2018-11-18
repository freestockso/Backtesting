
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
    /// Interaction logic for PortfolioControl.xaml
    /// </summary>
    public partial class PortfolioControl : UserControl
    {
        public PortfolioControl()
        {
            InitializeComponent();
        }
        private void RadGridView_Drop(object sender, DragEventArgs e)
        {

            DragDropSupport.OnDrop<IInstrument>(e, onDrop);
        }

        void onDrop(List<IInstrument> dl)
        {
            var dc = DataContext as PortfolioViewModel;
            dl.ForEach(v =>
            {
                if (!dc.PositionList.Any(i => i.InstrumentTicker == v.Ticker))
                    dc.PositionList.Add(new Position() {Shares=100, InstrumentTicker = v.Ticker,InstrumentName=v.Name,CurrentPrice=v.CurrentPrice });
            });
            dc.Save();
        }

        private void RadGridView_RowActivated(object sender, Telerik.Windows.Controls.GridView.RowEventArgs e)
        {
            var p = e.Row.DataContext as IPosition;
            if (p.TradeTrace.Count > 0)
            {
                PositionTable.RowDetailsVisibilityMode = Telerik.Windows.Controls.GridView.GridViewRowDetailsVisibilityMode.VisibleWhenSelected;
            }
            else
                PositionTable.RowDetailsVisibilityMode = Telerik.Windows.Controls.GridView.GridViewRowDetailsVisibilityMode.Collapsed;
        }
    }
}
