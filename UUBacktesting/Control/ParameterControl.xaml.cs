using CommonLib;
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
using Telerik.Windows.Controls;

namespace UUBacktesting.Control
{
    /// <summary>
    /// Interaction logic for ParameterControl.xaml
    /// </summary>
    public partial class ParameterControl : UserControl
    {
        public ParameterControl()
        {
            InitializeComponent();
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null) return;
            var data = e.NewValue as Parameter;
            if (data == null) return;
            var control = ParameterTemplateSelector.GetControl(data);
            if (control != null)
                EditControl.Child = control;
        }

    }

    public class ParameterTemplateSelector : DataTemplateSelector
    {
        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            FrameworkElement element = container as FrameworkElement;

            var parameter = item as Parameter;
            if (parameter != null)
            {
                if(parameter.EditMode== ParameterEditMode.boolean)
                    return  element.FindResource("booleanControl") as DataTemplate;
                if (parameter.EditMode == ParameterEditMode.dec|| parameter.EditMode == ParameterEditMode.dec32|| parameter.EditMode == ParameterEditMode.dec64)
                    return element.FindResource("decControl") as DataTemplate;
                if (parameter.EditMode == ParameterEditMode.int8|| parameter.EditMode == ParameterEditMode.int16|| parameter.EditMode == ParameterEditMode.int32
                    ||parameter.EditMode == ParameterEditMode.int64|| parameter.EditMode == ParameterEditMode.usint8|| parameter.EditMode == ParameterEditMode.usint16
                    || parameter.EditMode == ParameterEditMode.usint32|| parameter.EditMode == ParameterEditMode.usint64)
                    return element.FindResource("numControl") as DataTemplate;
                if (parameter.EditMode == ParameterEditMode.selection)
                    return element.FindResource("selectionControl") as DataTemplate;
                if (parameter.EditMode == ParameterEditMode.str|| parameter.EditMode == ParameterEditMode.cha)
                    return element.FindResource("strControl") as DataTemplate;
                if (parameter.EditMode == ParameterEditMode.time)
                    return element.FindResource("timeControl") as DataTemplate;
                

            }

            return base.SelectTemplate(item, container);
        }

        public static FrameworkElement GetControl(Parameter p)
        {
            var t = p.EditMode;
            Binding binding = new Binding()
            {
                Source = p,
                Path = new PropertyPath("Value"),
                Mode = BindingMode.TwoWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };
            if (t == ParameterEditMode.int8 || t == ParameterEditMode.int16 || t == ParameterEditMode.int32
                    || t == ParameterEditMode.int64 || t == ParameterEditMode.usint8 || t == ParameterEditMode.usint16
                    || t == ParameterEditMode.usint32 || t == ParameterEditMode.usint64)
            {
                var c = new Telerik.Windows.Controls.RadNumericUpDown();
                c.NumberDecimalDigits = 0;
                c.SetBinding(RadNumericUpDown.ValueProperty, binding);
                return c;
            }
            if (t == ParameterEditMode.dec || t == ParameterEditMode.dec32 || t== ParameterEditMode.dec64)
            {
                var c = new Telerik.Windows.Controls.RadNumericUpDown();
                c.NumberDecimalDigits = 2;
                c.SetBinding(RadNumericUpDown.ValueProperty, binding);
                return c;
            }
            if (t == ParameterEditMode.boolean)
            {
                var c = new CheckBox();
                c.SetBinding(CheckBox.IsCheckedProperty, binding);
                return c;
            }
            if (t == ParameterEditMode.selection)
            {
                var c = new ComboBox();
                c.SetBinding(ComboBox.SelectedItemProperty, binding);
                var seltypelist = Enum.GetValues(p.Value.GetType());
                foreach (var i in seltypelist)
                {
                    c.Items.Add(i);
                }
                return c;
            }
            if (t == ParameterEditMode.str||t== ParameterEditMode.cha)
            {
                var c = new TextBox();
                c.SetBinding(TextBox.TextProperty, binding);
                return c;
            }
            if (t == ParameterEditMode.time)
            {
                var c = new Telerik.Windows.Controls.RadDateTimePicker();
                c.SetBinding(RadDateTimePicker.SelectedValueProperty, binding);
                return c;
            }
            return null;
        }

    }
}
