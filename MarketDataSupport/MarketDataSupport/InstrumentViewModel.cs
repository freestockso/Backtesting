using BackTestingCommonLib;
using BackTestingInterface;
using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MarketDataSupport
{
    class InstrumentViewModel : ViewModelBase
    {
        IInstrument _TargetObject;
        public IInstrument TargetObject
        {
            get { return _TargetObject; }
            set { _TargetObject = value; OnPropertyChanged("TargetObject"); }
        }
        ObservableCollection<IMarketData> _MarketDataList = new ObservableCollection<IMarketData>();
        public ObservableCollection<IMarketData> MarketDataList {
            get { return _MarketDataList; }
        }
        public void LoadMarketData(DateTime start,DateTime end,IDataSource ds,IInstrument instrument)
        {

            var dl = ds.GetDataList(new List<IInstrument> { instrument }, start, end, CurrentPeriod);
            MarketDataList.Clear();
            dl.ForEach(v => MarketDataList.Add(v));
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

        DateTime _StartTime = DateTime.Now - TimeSpan.FromDays(5);
        public DateTime StartTime
        {
            get { return _StartTime; }
            set { _StartTime = value; OnPropertyChanged("StartTime"); }
        }

        DateTime _EndTime = DateTime.Now ;
        public DateTime EndTime
        {
            get { return _EndTime; }
            set { _EndTime = value; OnPropertyChanged("EndTime"); }
        }

        private Point _panOffset;
        private Size _zoom;
        public Point PanOffset
        {
            get
            {
                return _panOffset;
            }
            set
            {
                if (this._panOffset != value)
                {
                    this._panOffset = value;
                    this.OnPropertyChanged("PanOffset");
                }
            }
        }

        public Size Zoom
        {
            get
            {
                return _zoom;
            }
            set
            {
                if (this._zoom != value)
                {
                    this._zoom = value;
                    this.OnPropertyChanged("Zoom");
                }
            }
        }

        List<MarketDataGrade> _Period;
        public List<MarketDataGrade> Period
        {
            get
            {
                if (_Period == null)
                {
                    _Period = new List<MarketDataGrade>() { MarketDataGrade.Day, MarketDataGrade.FifteenMinutes, MarketDataGrade.FiveMinutes, MarketDataGrade.HalfDay, MarketDataGrade.HalfHour, MarketDataGrade.HalfYear, MarketDataGrade.Hour, MarketDataGrade.Month, MarketDataGrade.ThreeDays, MarketDataGrade.Season, MarketDataGrade.HalfMonth, MarketDataGrade.Week, MarketDataGrade.Year };
                }
                return _Period;
            }
        }

        MarketDataGrade _CurrentPeriod = MarketDataGrade.FifteenMinutes;
        public MarketDataGrade CurrentPeriod
        {
            get { return _CurrentPeriod; }
            set
            {
                if(_CurrentPeriod!=value)
                {
                    _CurrentPeriod = value;
                    var l =MarketData.SummaryMarketDataList( MarketDataList.ToList(), value);
                    MarketDataList.Clear();
                    l.ForEach(v => MarketDataList.Add(v));
                }
            }
        }

        bool _IsEnableLoad = true;
        public bool IsEnableLoad { get { return _IsEnableLoad; } set { _IsEnableLoad = value; OnPropertyChanged("IsEnableLoad"); } }
    }
}
