using BackTestingInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.ChartView;
using System.Globalization;

namespace UUBacktesting
{
    public class TelerikChartViewHelper
    {
        Color[] colors = new Color[] { Colors.Black, Colors.Orange, Colors.Blue, Colors.Pink, Colors.Green, Colors.Red, Colors.Brown };

        public List<LendItem> AddSeriesToChart(Dictionary<string, List<TimeValueObject>> data, RadCartesianChart chart,bool AddValueAxis=false)
        {
            chart.HorizontalAxis = new DateTimeCategoricalAxis() { LabelFormat = "MM/dd HH:mm" };
            var vl = new LinearAxis();
            vl.HorizontalAlignment = HorizontalAlignment.Right;

            int i = 0;
            var l = new List<LendItem>();
            foreach (var kv in data)
            {
                if (i > 6) i = 0;
                var line = new LineSeries();
                line.CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Time" };
                line.ValueBinding = new GenericDataPointBinding<TimeValueObject, double>() { ValueSelector = product => product.DoubleValue };
                line.ItemsSource = kv.Value;
                line.ToolTip = kv.Key;
                line.Stroke = new SolidColorBrush(colors[i]);
                line.StrokeThickness = 3;
                if(AddValueAxis)
                    line.VerticalAxis = vl;
                chart.Series.Add(line);
                l.Add(new LendItem() { Name = kv.Key, control = line, ControlColor = new SolidColorBrush(colors[i]) });
                i++;
            }

            return l;
        }

        public List<LendItem> AddSeriesToChart(Dictionary<string, List<NameValueObject>> data, RadCartesianChart chart, bool AddValueAxis = false)
        {
            chart.HorizontalAxis = new CategoricalAxis();
            var vl = new LinearAxis();
            vl.HorizontalAlignment = HorizontalAlignment.Right;
            int i = 0;
            var l = new List<LendItem>();
            foreach (var kv in data)
            {
                if (i > 6) i = 0;
                var line = new LineSeries();
                line.CategoryBinding = new PropertyNameDataPointBinding() { PropertyName = "Name" };
                line.ValueBinding = new GenericDataPointBinding<NameValueObject, double>() { ValueSelector = product => product.DoubleValue };
                line.ItemsSource = kv.Value;
                line.ToolTip = kv.Key;
                line.Stroke = new SolidColorBrush(colors[i]);
                line.StrokeThickness = 3;
                if (AddValueAxis)
                    line.VerticalAxis = vl;
                chart.Series.Add(line);
                l.Add(new LendItem() { Name = kv.Key, control = line, ControlColor = new SolidColorBrush(colors[i]) });
                i++;
            }

            return l;
        }
    }

    public class LendItem
    {
        public string Name { get; set; }
        public FrameworkElement control { get; set; }
        public SolidColorBrush ControlColor { get; set; }
        public bool IsVisible
        {
            get
            {
                if (control == null) return false;
                return control.IsVisible;
            }
            set
            {
                if (control == null) return;
                if (value)
                    control.Visibility = Visibility.Visible;
                else
                    control.Visibility = Visibility.Hidden;
            }
        }
    }

    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool && (bool)value) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is Visibility && (Visibility)value == Visibility.Visible;
        }
    }
}
