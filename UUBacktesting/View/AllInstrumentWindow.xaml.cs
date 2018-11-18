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
using UUBacktesting.ViewModel;

namespace UUBacktesting.View
{
    /// <summary>
    /// Interaction logic for AllInstrumentWindow.xaml
    /// </summary>
    public partial class AllInstrumentWindow : Window
    {
        public AllInstrumentWindow()
        {
            InitializeComponent();
        }
        public List<IInstrument> GetSelectItem()
        {
            var l = new List<IInstrument>();
            
            var il = InstrumentList.SelectedItems;
            foreach (var i in il)
            {
                l.Add(i as IInstrument);
            }
            return l;
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

        private void ButtonSelectAll_Click(object sender, RoutedEventArgs e)
        {
            InstrumentList.SelectAll();
        }

        private void InstrumentList_KeyDown(object sender, KeyEventArgs e)
        {
            //if(e.Key== Key.co)
        }

        private void ButtonOpen_Click(object sender, RoutedEventArgs e)
        {
            var item = sender as FrameworkElement;
            var dc = item.DataContext;
            InstrumentControl.ShowInstrumentInfo(dc as IInstrument);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var dc = DataContext as AllInstrumentWindowViewModel;
            var t = sender as TextBox;
            dc.Filter(t.Text);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
