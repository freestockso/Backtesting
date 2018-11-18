using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Media;

namespace CommonLibForWPF
{
    public class CommonCommand:ICommand
    {
        public string Name { get; set; }
        public string Memo { get; set; }
        public SolidColorBrush BackgroundBrush { get; set; }
        public SolidColorBrush ForegroundBrush { get; set; }
        public CommonCommand(Action<object> action)
        {
            TargetAction = action;
        }
        public CommonCommand(Action<object> action,Predicate<object> predicate)
        {
            TargetAction = action;
            TargetPredicate = predicate;
        }
        public Action<object> TargetAction { get; set; }
        public Predicate<object> TargetPredicate { get; set; }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (TargetPredicate != null)
                return TargetPredicate(parameter);
            return true;
        }

        public void Execute(object parameter)
        {
            if (CanExecute(parameter))
                TargetAction(parameter);
        }
    }
}
