using BackTestingInterface;
using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UUBacktesting.ViewModel
{
    class StrategyViewModel: ParameterObjectViewModelBase<IStrategy>,IObersverSupport
    {
        public CommonCommand SaveOrigenalCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (TargetObject == null) return;
                    Save();
                    TargetObject.SaveOriginalStatus();
                });
            }
        }
        public CommonCommand LoadOrigenalCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (TargetObject == null) return;

                    TargetObject.LoadOriginalStatus();
                    Load();
                });
            }
        }

        public CommonCommand EditCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    var s = TargetObject as IEditControlSupport;
                    if (s != null)
                    {
                        var c = s.GetEditControl();
                        var w = new Window();
                        w.Content = c;
                        w.Title = TargetObject.Name;
                        w.ShowDialog();
                    }
                });
            }
        }

        public virtual void Refresh()
        {
            
        }

        public virtual bool NeedRefresh()
        {
            return false;
        }
    }
}
