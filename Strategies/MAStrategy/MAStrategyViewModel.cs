using BackTestingInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telerik.Windows.Controls;

namespace MAStrategy
{
    class MAStrategyViewModel:ViewModelBase
    {
        ObservableCollection<IPosition> _PositionList = new ObservableCollection<IPosition>();
        public ObservableCollection<IPosition> PositionList {
            get { return _PositionList; }
        }

        public int SlowUnit
        {
            get { if (TargetObject != null) return TargetObject.SlowUnit;return 0; }
            set { if (TargetObject != null) TargetObject.SlowUnit = value; OnPropertyChanged("SlowUnit"); }
        }
        public int FastUnit
        {
            get { if (TargetObject != null) return TargetObject.FastUnit; return 0; }
            set { if (TargetObject != null) TargetObject.FastUnit = value; OnPropertyChanged("FastUnit"); }
        }
        public IPosition CurrentPosition
        {
            get
            {
                if (TargetObject != null && TargetObject.CurrentPortfolio != null)
                {
                    var t = TargetObject.CurrentPortfolio.PositionList.FirstOrDefault(v => v.InstrumentTicker == TargetObject.CurrentTicker);
                    if (t != null)
                        return t;
                }
                return null;
            }
            set
            {
                if (TargetObject != null)
                    TargetObject.CurrentTicker = value.InstrumentTicker;
                OnPropertyChanged("CurrentPosition");
            }
        }
        
        public MAStrategy TargetObject { get; set; }

        public void Refresh()
        {
            if (TargetObject == null) return;
            if (TargetObject.CurrentPortfolio == null) return;

            PositionList.Clear();
            TargetObject.CurrentPortfolio.PositionList.ForEach(v => PositionList.Add(v));

            DataList.Clear();
            TargetObject.MADataList.ForEach(v => DataList.Add(v));

            if(!string.IsNullOrEmpty(TargetObject.CurrentTicker))
            {
                CurrentPosition = PositionList.FirstOrDefault(v => v.InstrumentTicker == TargetObject.CurrentTicker);
            }
        }

        public DelegateCommand RefreshCommand
        {
            get
            {
                return new DelegateCommand((o) =>
                {
                    Refresh();
                });
            }
        }
        List<MAData> _DataList = new List<MAData>();
        public List<MAData> DataList { get { return _DataList; } }

    }
}
