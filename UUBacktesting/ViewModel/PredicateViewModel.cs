using BackTestingInterface;
using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Backtesting7.ViewModel
{
    class PredicateViewModel:ViewModelBase
    {
        IPredicate _TargetObject;
        public IPredicate TargetObject
        {
            get { return _TargetObject; }
            set { _TargetObject = value; OnPropertyChanged("TargetObject"); }
        }

        public virtual string Name
        {
            get { if (TargetObject != null) return TargetObject.Name; return ""; }
            set { if (TargetObject != null) { TargetObject.Name = value; OnPropertyChanged("Name"); } }
        }

        public virtual string Memo
        {
            get { if (TargetObject != null) return TargetObject.Memo; return ""; }
            set { if (TargetObject != null) { TargetObject.Memo = value; OnPropertyChanged("Memo"); } }
        }
        void Load()
        {
            TargetObject.SaveToParameterList();
        }

        void Save()
        {
            TargetObject.LoadFromParameterList();
        }

        public CommonCommand LoadCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    Load();
                });
            }
        }
        public CommonCommand SaveCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    Save();
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
                        w.Title = Name;
                        w.ShowDialog();
                    }
                });
            }
        }
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
    }

}
