using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Telerik.Windows.Controls;
using UUBacktesting.ViewModel;

namespace UUBacktesting
{
    public class TelerikDockingHelper
    {
        public RadPaneGroup MainGroup { get; set; }
        public ProjectViewModelBase projectViewModel { get; set; }
        bool ActiveExist(FrameworkElement control)
        {
            if (control == null) return true;
            for (int i = 0; i < MainGroup.Items.Count; i++)
            {
                var p = MainGroup.Items[i] as RadPane;
                var c = p.Content as FrameworkElement;
                if (c.DataContext == control.DataContext)
                {
                    p.IsActive = true;
                    return true;
                }
            }
            return false;
        }
        public void OpenDocument(string header, FrameworkElement control, bool isBinding)
        {
            if (control == null) { MessageBox.Show("No valid control to show as document window!"); return; }
            if (string.IsNullOrEmpty(header)) { MessageBox.Show("No valid header to show as document window!"); return; }
            if(isBinding)
                if (ActiveExist(control)) return;
            else
                for (int i = 0; i < MainGroup.Items.Count; i++)
                {
                    var p = MainGroup.Items[i] as RadPane;
                    if (p.Header!=null&&p.Header.ToString() == header)
                    {
                        p.IsActive = true;
                        return;
                    }
                }


            var pane = new RadPane() { };
            if (isBinding)
            {
                Binding binding = new Binding();
                binding.Path = new PropertyPath(header);
                binding.Source = control.DataContext;
                BindingOperations.SetBinding(pane, RadPane.HeaderProperty, binding);
            }
            else
            {
                pane.Header = header;
            }
            pane.Content = control;
            MainGroup.AddItem(pane, Telerik.Windows.Controls.Docking.DockPosition.Center);
            pane.Unloaded += Pane_Unloaded;

            if(projectViewModel!=null)
            {
                var vm = control.DataContext;
                if (vm is INotifiedViewModel)
                    projectViewModel.OpenedViewModel.Add(vm as INotifiedViewModel);
            }
        }
        
        private void Pane_Unloaded(object sender, RoutedEventArgs e)
        {
            var pane = sender as RadPane;
            if (pane == null) return;
            pane.Unloaded -= Pane_Unloaded;
            var control = pane.Content as FrameworkElement;
            if (control == null) return;
            var dc = pane.DataContext as ProjectViewModelBase;
            var vm = control.DataContext as IObersverSupport;
            if (dc != null && vm != null)
            {
                if (vm != null && dc.OpenedViewModel.Contains(vm))
                    dc.OpenedViewModel.Remove(vm);
            }
        }
    }
}
