
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
using System.Globalization;

namespace UUBacktesting.View
{
    /// <summary>
    /// Interaction logic for ProjectWindow.xaml
    /// </summary>
    public partial class BacktestingProjectWindow : Window
    {
        public BacktestingProjectWindow()
        {
            InitializeComponent();
            helper.MainGroup = MainGroup;
            
        }
        public TelerikDockingHelper helper = new TelerikDockingHelper();

        public void OpenDocument(string header,FrameworkElement control,bool isBinding)
        {
            helper.OpenDocument(header, control,isBinding);
        }

        //void OpenDocument(object sender,FrameworkElement control)
        //{
        //    if (sender == null||!(sender is FrameworkElement)|| control==null) return;
        //    var item = sender as FrameworkElement;
        //    var data = item.DataContext;
        //    control.DataContext = data;
        //    var dc = DataContext as BacktestingProjectViewModel;
        //    if (data is IObersverSupport && !dc.OpenedViewModel.Contains(data as IObersverSupport))
        //        dc.OpenedViewModel.Add(data as IObersverSupport);
        //    if (data.GetType().GetProperty("Name") != null)
        //    {
        //        var pn = data.GetType().GetProperty("Name");
        //        var title=pn.GetValue(data).ToString();

        //        OpenDocument(title, control);
        //    }
        //    else
        //    {
        //        OpenDocument("UnNamed", control);
        //    }
        //}

        private void Condition_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender == null || !(sender is FrameworkElement)) return;
            var item = sender as FrameworkElement;
            var data = item.DataContext;
            if(data is ConditionViewModel)
            {
                (data as ConditionViewModel).Refresh();
                var dc = DataContext as BacktestingProjectViewModel;
                dc.OpenCondition(data as ConditionViewModel);
            }

        }
        private void Instrument_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var dc = DataContext as BacktestingProjectViewModel;
            var item = sender as FrameworkElement;
            var data = item.DataContext;
            var inst = data as IInstrument;
            dc.OpenInstrument(inst);

        }

        private void Instrument_Drop(object sender, DragEventArgs e)
        {
            DragDropSupport.OnDrop<IInstrument>(e, onDrop);
        }
        void onDrop(List<IInstrument> dl)
        {
            var dc = DataContext as BacktestingProjectViewModel;
            dl.ForEach(v =>
            {
                if (!dc.InstrumentList.Any(i => i.Ticker == v.Ticker))
                {
                    dc.InstrumentList.Add(v);
                }
                if (!dc.TargetProject.InstrumentList.Any(i => i.Ticker == v.Ticker))
                    dc.TargetProject.InstrumentList.Add(v.Clone() as IInstrument);
            });
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

        private void RadMenuItemClose_Click(object sender, Telerik.Windows.RadRoutedEventArgs e)
        {
            Close();
        }

        private void ConditionList_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var dc = DataContext as ProjectViewModelBase;
            var t = sender as TextBox;
            dc.Filter(t.Text);
        }
    }

    
}
