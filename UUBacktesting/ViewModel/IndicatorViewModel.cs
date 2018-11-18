using BackTestingInterface;
using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Backtesting7.ViewModel
{
    class IndicatorViewModel:ParameterObjectViewModelBase<IIndicator>,IObersverSupport
    {
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
        public  IInstrument InterestedInstrument
        {
            get { if (TargetObject != null) return TargetObject.InterestedInstrument;return null; }
            set { if (TargetObject != null) TargetObject.InterestedInstrument = value; OnPropertyChanged("InterestedInstrument"); }
        }

        ObservableCollection<IIndicatorValue> _DataList = new ObservableCollection<IIndicatorValue>();
        public ObservableCollection<IIndicatorValue> DataList { get { return _DataList; } }

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

        public override void Load()
        {
            Refresh();

            base.Load();
        }

        public void Refresh()
        {
            if (TargetObject == null) return;
            DataList.Clear();
            TargetObject.DataList.ForEach(v =>
            {
                if (!DataList.Any(x => x == v))
                    DataList.Add(v);
            });
        }

        public bool NeedRefresh()
        {
            return true;
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
