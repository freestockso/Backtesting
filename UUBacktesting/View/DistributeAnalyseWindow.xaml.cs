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
using System.Windows.Shapes;
using Telerik.Windows.Controls;
using UUBacktesting.ViewModel;

namespace UUBacktesting.View
{
    /// <summary>
    /// Interaction logic for DistributeAnalyseWindow.xaml
    /// </summary>
    public partial class DistributeAnalyseWindow : Window
    {
        public DistributeAnalyseWindow()
        {
            InitializeComponent();
            DataContext = new DistributeAnalyseViewModel() { OpenView=OpenDocument};
        }

        private void ExitClick(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            Close();
        }

        public void OpenDocument(string header, FrameworkElement control)
        {
            if (control == null) MessageBox.Show("No valid control to show as document window!");
            var pane = new RadPane() { Header = header };

            pane.Content = control;
            MainGroup.AddItem(pane, Telerik.Windows.Controls.Docking.DockPosition.Center);
        }

        private void Instrument_Drop(object sender, DragEventArgs e)
        {
            DragDropSupport.OnDrop<IInstrument>(e, onDrop);
        }
        void onDrop(List<IInstrument> dl)
        {
            var dc = DataContext as DistributeAnalyseViewModel;
            dl.ForEach(v =>
            {
                if (!dc.InstrumentList.Any(i => i.Ticker == v.Ticker))
                {
                    var iv = v.Clone() as IInstrument;
                    dc.InstrumentList.Add(iv);

                }
            });
        }

        private void Sector_Drop(object sender, DragEventArgs e)
        {
            DragDropSupport.OnDrop<string>(e, onDropSector);
        }
        void onDropSector(List<string> dl)
        {
            var dc = DataContext as DistributeAnalyseViewModel;
            var s = dl.FirstOrDefault();
            var rl = new List<string>();
            if(Instrument.AllIndustoryList.Contains(s))
            {
                foreach(var sector in dc.SectorList)
                {
                    if (!Instrument.AllIndustoryList.Contains(sector))
                        rl.Add(sector);
                }
            }
            if (Instrument.AllRegionList.Contains(s))
            {
                foreach (var sector in dc.SectorList)
                {
                    if (!Instrument.AllRegionList.Contains(sector))
                        rl.Add(sector);
                }
            }
            rl.ForEach(v => { dc.SectorList.Remove(v); });
            dl.ForEach(v =>
            {
                if (!dc.SectorList.Any(i => i == v))
                {
                    dc.SectorList.Add(v);
                }
            });
        }
        private void AllInstrumentList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parent = (ListBox)sender;
            var data = new List<object>();
            foreach (var item in parent.SelectedItems)
            {
                FrameworkElement element = item as FrameworkElement;
                if (element != null)
                    data.Add(element.DataContext);
                else
                    data.Add(item);
            }

            DragDropSupport.OnDrag(data, parent);

        }

        private void IndustoryList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parent = (ListBox)sender;
            var data = new List<object>();
            foreach (var item in parent.SelectedItems)
            {
                FrameworkElement element = item as FrameworkElement;
                if (element != null)
                    data.Add(element.DataContext);
                else
                    data.Add(item);
            }

            DragDropSupport.OnDrag(data, parent);
        }

        private void RegionList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var parent = (ListBox)sender;
            var data = new List<object>();
            foreach (var item in parent.SelectedItems)
            {
                FrameworkElement element = item as FrameworkElement;
                if (element != null)
                    data.Add(element.DataContext);
                else
                    data.Add(item);
            }

            DragDropSupport.OnDrag(data, parent);
        }

        private void InstrumentDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dc = DataContext as DistributeAnalyseViewModel;
            var item = e.OriginalSource as FrameworkElement;
            var data = item.DataContext;
            var inst = data as IInstrument;
            if (inst != null)
            {
                dc.OpenInstrument(inst);
            }
        }

    }
}
