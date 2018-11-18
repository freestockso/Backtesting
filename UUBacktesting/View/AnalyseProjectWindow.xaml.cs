
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
using System.Windows.Shapes;
using Telerik.Windows.Controls;

namespace UUBacktesting.View
{
    /// <summary>
    /// Interaction logic for AnalyseProjectWindow.xaml
    /// </summary>
    public partial class AnalyseProjectWindow : Window
    {
        public AnalyseProjectWindow()
        {
            InitializeComponent();
            helper.MainGroup = MainGroup;
        }
        public TelerikDockingHelper helper = new TelerikDockingHelper();
        public void OpenDocument(string header, FrameworkElement control,bool isBinding)
        {
            helper.OpenDocument(header, control,isBinding);
        }
        //public void OpenDocument(string header, FrameworkElement control)
        //{
        //    if (control == null) { MessageBox.Show("No valid control to show as document window!"); return; }
        //    if (string.IsNullOrEmpty(header)) { MessageBox.Show("No valid header to show as document window!");return; }
        //    for(int i=0;i< MainGroup.Items.Count;i++)
        //    {
        //        var p = MainGroup.Items[i] as RadPane;
        //        if (p.Header.ToString() == header)
        //        {
        //            p.IsActive = true;
        //            return;
        //        }
        //    }
        //    var pane = new RadPane() { Header = header };

        //    pane.Content = control;
        //    MainGroup.AddItem(pane, Telerik.Windows.Controls.Docking.DockPosition.Center);
        //    pane.Unloaded += Pane_Unloaded;
        //}
        //private void Pane_Unloaded(object sender, RoutedEventArgs e)
        //{
        //    var pane = sender as RadPane;
        //    if (pane == null) return;
        //    pane.Unloaded -= Pane_Unloaded;
        //    var control = pane.Content as FrameworkElement;
        //    if (control == null) return;
        //    var dc = DataContext as ProjectViewModelBase;
        //    var vm = control.DataContext as IObersverSupport;
        //    if (dc != null&& vm!=null)
        //    {
        //        if (vm != null && dc.OpenedViewModel.Contains(vm))
        //            dc.OpenedViewModel.Remove(vm);
        //    }
        //}
        private void CurrentDataSource_Click(object sender, RoutedEventArgs e)
        {
            var dc = DataContext as AnalyseProjectViewModel;
            //dc.LoadCurrentInstrumentMarketData(dc.AnalyseStartTime, dc.AnalyseEndTime);
            helper.OpenDocument(dc.CurrentDataSource.Name, new DataSourceControl() { DataContext = dc },false);
        }

        private void InstrumentList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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
        private void Instrument_Drop(object sender, DragEventArgs e)
        {
            DragDropSupport.OnDrop<IInstrument>(e, onDrop);
        }
        void onDrop(List<IInstrument> dl)
        {
            var dc = DataContext as ProjectViewModelBase;
            dl.ForEach(v =>
            {
                if (!dc.InstrumentList.Any(i => i.Ticker == v.Ticker))
                {
                    dc.InstrumentList.Add(v);
                }
            });
        }
        private void ValueView_Click(object sender, RoutedEventArgs e)
        {
            var c = new AnalyseProjectControl() { DataContext = DataContext };
            helper.OpenDocument("Project View", c,false);
        }
        //void OpenDocument(object sender, FrameworkElement control)
        //{
        //    var dc = DataContext as AnalyseProjectViewModel;
        //    if (sender == null || !(sender is FrameworkElement) || control == null) return;
        //    var item = sender as FrameworkElement;
        //    var data = item.DataContext;
        //    control.DataContext = data;
        //    if (data is IObersverSupport&&!dc.OpenedViewModel.Contains(data as IObersverSupport))
        //        dc.OpenedViewModel.Add(data as IObersverSupport);
        //    if (data.GetType().GetProperty("Name") != null)
        //    {
        //        var pn = data.GetType().GetProperty("Name");
        //        var title = pn.GetValue(data).ToString();

        //        OpenDocument(title, control);
        //    }
        //    else
        //    {
        //        OpenDocument("UnNamed", control);
        //    }
        //}
        private void RadListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dc = DataContext as AnalyseProjectViewModel;
            var item = sender as FrameworkElement;
            var data = item.DataContext;
            var obj = data as ConditionViewModel;
            if (obj != null)
            {
                dc.OpenCondition(obj);
            }
        }

        private void Instrument_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dc = DataContext as AnalyseProjectViewModel;
            var item = sender as FrameworkElement;
            var data = item.DataContext;
            var inst = data as IInstrument;
            if (inst != null)
            {
                dc.OpenInstrument(inst);
            }
        }

        private void RadMenuItemClose_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            Close();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var dc = DataContext as ProjectViewModelBase;
            var t = sender as TextBox;
            dc.Filter(t.Text);
        }
    }
}
