using UUBacktesting.View;
using UUBacktesting.ViewModel;
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

namespace UUBacktesting
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

        private void PlayerStatusControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender == null || !((sender is AnalyseProjectSummaryControl)|| (sender is BacktestingProjectSummaryControl)))
                return;

            var control = sender as FrameworkElement;

            var d = control.DataContext as ProjectSummaryViewModelBase;
            if(d!=null)
                foreach (var p in MainViewModel.ProjectList)
                {
                    if (p == d)
                    {
                        p.Selected();

                        MainViewModel.CurrentProject = p;
                    }
                    else p.DeSelected();
                }
        }

        private void Window_Closing(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm exit? Press OK to close all windows and exit program.", "Confirm",MessageBoxButton.OKCancel)== MessageBoxResult.OK)
            {
                foreach (var w in Application.Current.Windows)
                {
                    if (w != this)
                        (w as Window).Close();
                }
            }

        }
    }

    public class ProjectItemTemplateSelector : DataTemplateSelector
    {

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            Window win = Application.Current.MainWindow;

            if (item is BacktestingProjectSummaryViewModel)
            {
                return win.FindResource("backtestingProjectTemplate") as DataTemplate;
                
            }
            return win.FindResource("analyseProjectTemplate") as DataTemplate;
        }
    }
}
