using BackTestingCommonLib;
using BackTestingInterface;
using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UUBacktesting.ViewModel
{
    class RiskControlViewModel : ParameterObjectViewModelBase<IRiskControl>, IObersverSupport
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
        public bool NeedRefresh()
        {
            return false;
        }

        public void Refresh()
        {
            PositionControlList.Clear();
            TargetObject.PositionControlList.ForEach(v =>
            {
                PositionControlList.Add(v);
            });
        }
        public override void Load()
        {
            base.Load();
            Refresh();
        }
        ObservableCollection<IPositionControl> _PositionControlList = new ObservableCollection<IPositionControl>();
        public ObservableCollection<IPositionControl> PositionControlList { get { return _PositionControlList; } }
        public IPositionControl CurrentPositionControl { get; set; }
        public double MinPositionPercent
        {
            get { return TargetObject.MinPositionPercent; }
            set { TargetObject.MinPositionPercent = value; OnPropertyChanged("MinPositionPercent"); }
        }
        public double MaxPositionPercent
        {
            get { return TargetObject.MaxPositionPercent; }
            set { TargetObject.MaxPositionPercent = value; OnPropertyChanged("MaxPositionPercent"); }
        }

        public CommonCommand AddPositionControlCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (TargetObject == null) return;
                    var control = new PositionControl();
                    PositionControlList.Add(control);
                    TargetObject.PositionControlList.Add(control);
                });
            }
        }
        public CommonCommand DeletePositionControlCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (TargetObject == null||CurrentPositionControl==null) return;
                    TargetObject.PositionControlList.Remove(CurrentPositionControl);
                    PositionControlList.Remove(CurrentPositionControl);
                    
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
