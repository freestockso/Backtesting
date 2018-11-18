using BackTestingCommonLib;
using BackTestingInterface;
using CommonLib;
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
    class InstrumentViewModel : ViewModelBase
    {
        IInstrument _TargetObject;
        public IInstrument TargetObject
        {
            get { return _TargetObject; }
            set { _TargetObject = value; OnPropertyChanged("TargetObject"); }
        }
        bool _IsShowDistributeTrend =true;
        public bool IsShowDistributeTrend
        {
            get { return _IsShowDistributeTrend; }
            set { _IsShowDistributeTrend = value; OnPropertyChanged("IsShowDistributeTrend"); }
        }

        List<IMarketData> _SourceDataList = new List<IMarketData>();
        public List<IMarketData> SourceList
        {
            get { return _SourceDataList; }
        }
        ObservableCollection<IMarketData> _MarketDataList = new ObservableCollection<IMarketData>();
        public ObservableCollection<IMarketData> MarketDataList {
            get { return _MarketDataList; }
        }

        ObservableCollection<TimeValueObject> _DistributeTrendList = new ObservableCollection<TimeValueObject>();
        public ObservableCollection<TimeValueObject> DistributeTrendList { get { return _DistributeTrendList; } }
        void refreshChartAxis()
        {
            OnPropertyChanged("MaxPrice");
            OnPropertyChanged("MinPrice");
            OnPropertyChanged("MaxVolumeAxisValue");
            OnPropertyChanged("MinVolumeAxisValue");
        }
        double GetPriceDis()
        {
            return MarketDataList.Max(v => v.High) - MarketDataList.Min(v => v.Low);
        }
        double GetVolumeDis()
        {
            return MarketDataList.Max(v => v.Volume) - MarketDataList.Min(v => v.Volume);
        }
        public double MaxPrice
        {
            get
            {
                if (MarketDataList.Count == 0) return 100;
                return MarketDataList.Max(v => v.High) + (GetPriceDis() * MarginPercent);
            }
        }
        public double MinPrice
        {
            get
            {
                if (MarketDataList.Count == 0) return 0;
                var h = GetPriceDis() * (MarginPercent * 2 + 1);
                return MaxPrice - h / MainChartPercent;
            }
        }

        public double MaxVolumeAxisValue
        {
            get
            {
                if (MarketDataList.Count == 0) return 1000;
                var h = GetVolumeDis() * (MarginPercent * 2 + 1);
                return MinVolumeAxisValue + h / (1 - MainChartPercent);
            }

        }
        public double MinVolumeAxisValue
        {
            get
            {
                if (MarketDataList.Count == 0) return 0;
                var min = MarketDataList.Min(v => v.Volume);
                return min - (GetVolumeDis() * MarginPercent);
            }

        }
        double _MarginPercent = 0.5;
        public double MarginPercent
        {
            get { return _MarginPercent; }
            set { _MarginPercent = value; OnPropertyChanged("MarginPercent"); refreshChartAxis(); }
        }
        double _MainChartPercent = 0.75;
        public double MainChartPercent
        {
            get { return _MainChartPercent; }
            set { _MainChartPercent = value; OnPropertyChanged("MainChartPercent"); refreshChartAxis(); }
        }
        public CommonCommand LoadCommand
        {
            get {
                return new CommonCommand((o) =>
          {
              if (TargetObject != null)
              {
                  LoadMarketData(StartTime, EndTime, GetCurrentDataSource, TargetObject);
                  refreshChartAxis();
              }
          });
            }
        }

        public void LoadMarketData(DateTime start,DateTime end,Func<IDataSource> ds,IInstrument instrument)
        {
            if (ds == null||ds()==null)
            {
                MessageBox.Show("please select valid data source");
                return;
            }

            var dl = ds().GetDataList(new List<IInstrument> { instrument }, start, end,  MarketDataGrade.FiveMinutes);
            SourceList.Clear();
            dl.ForEach(v => SourceList.Add(v));
            var ml = MarketData.SummaryMarketDataList(dl, CurrentPeriod);
            MarketDataList.Clear();
            ml.ForEach(v => MarketDataList.Add(v));
            OnPropertyChanged("IsLoaded");
        }
        public string Ticker
        {
            get { if (TargetObject != null) return TargetObject.Ticker; return ""; }
        }
        public double PE
        {
            get
            {
                if (TargetObject != null) 
                    return TargetObject.PE;
                return -1;
            }
        }

        public double PB
        {
            get
            {
                if (TargetObject != null)
                    return TargetObject.PB;
                return -1;
            }
        }

        public double MarketValue
        {
            get
            {
                if (TargetObject != null)
                    return TargetObject.MarketValue;
                return -1;
            }
        }

        public string Industory
        {
            get
            {
                if (TargetObject != null)
                    return TargetObject.Industory;
                return "";
            }
        }

        public string Region
        {
            get
            {
                if (TargetObject != null)
                    return TargetObject.Region;
                return "";
            }
        }

        public string Currency
        {
            get
            {
                if (TargetObject != null)
                    return TargetObject.Currency;
                return "";
            }
        }

        public double Margin
        {
            get
            {
                if (TargetObject != null)
                    return TargetObject.Margin;
                return -1;
            }
        }

        public double OrderFixedCost
        {
            get
            {
                if (TargetObject != null)
                    return TargetObject.OrderFixedCost;
                return -1;
            }
        }

        public double OrderPercentCost
        {
            get
            {
                if (TargetObject != null)
                    return TargetObject.OrderPercentCost;
                return -1;
            }
        }
        public double CurrentPrice
        {
            get
            {
                if (TargetObject != null)
                    return TargetObject.CurrentPrice;
                return -1;
            }
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

        public Func<IDataSource> GetCurrentDataSource
        {
            get;set;
        }
        IDataSource _CurrentDataSource;
        public IDataSource CurrentDataSource
        {
            get { if (GetCurrentDataSource != null) return GetCurrentDataSource(); return _CurrentDataSource; }
            set
            {
                _CurrentDataSource = value;
                GetCurrentDataSource = () => { return _CurrentDataSource; };
                OnPropertyChanged("CurrentDataSource");
            }
        }
        public List<IDataSource> ValidDataSourceList
        {
            get { return MainViewModel.Resource.DataSourcePrototypeList; }
        }

        public List<MarketDataGrade> Period
        {
            get
            {
                return MarketData.GetMarketDataGradeList();
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
                    try {
                        var l = MarketData.SummaryMarketDataList(SourceList, value);
                        MarketDataList.Clear();
                        l.ForEach(v => MarketDataList.Add(v)); }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }

        bool _IsEnableLoad = true;
        public bool IsEnableLoad { get { return _IsEnableLoad; } set { _IsEnableLoad = value; OnPropertyChanged("IsEnableLoad"); } }
        public bool IsLoaded
        {
            get { if (MarketDataList.Count > 0) return true;  return false; }
        }

        public List<DistributeValueProcessMode> ProcessTypeList { get { return BackTestingCommonLib. DistributeAnalyse.GetProcessModeList();  } }

        public DistributeValueProcessMode CurrentProcessType { get; set; }

        public CommonCommand DistributeCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    GenerateDistribute();
                });
            }
        }
        int _CountStep = 5;
        public int CountStep
        {
            get { return _CountStep; }
            set { _CountStep = value; OnPropertyChanged("CountStep"); }
        }

        ObservableCollection<DataPoint> _MarketDistributeList = new ObservableCollection<DataPoint>();
        public ObservableCollection<DataPoint> MarketDistributeList { get { return _MarketDistributeList; } }

        ObservableCollection<DataPoint> _NormalDistributeList = new ObservableCollection<DataPoint>();
        public ObservableCollection<DataPoint> NormalDistributeList { get { return _NormalDistributeList; } }

        public double CurrentValue { get; set; }

        public bool IncludeZero
        {
            get { return da.IsIncludeZero; }
            set { da.IsIncludeZero = value; }
        }

        List<TimeValueObject> CalculateDataList { get; set; }
        BackTestingCommonLib.DistributeAnalyse da = new BackTestingCommonLib.DistributeAnalyse();
        void GenerateDistribute()
        {

            if (MarketDataList.Count == 0)
            {
                MessageBox.Show("Please load market data first.");
                return;
            }
            MarketDistributeList.Clear();
            NormalDistributeList.Clear();
            var nl =CommonLib.Distribute.GetNormalDistribute(0,1,3.5,1d/CountStep);
            nl.ForEach(v => NormalDistributeList.Add(v));
            da.StatisticStep = CountStep;
            da.ProcessMode = CurrentProcessType;
            var l = da.GetDistributeByTime(MarketDataList.ToList());
            if (l.Count == 0) return;
            l.FirstOrDefault().Value.ForEach(v => MarketDistributeList.Add(v));
            if (CalculateDataList == null)
                CalculateDataList = new List<TimeValueObject>();
            CalculateDataList.Clear();
            foreach(var v in da.GetDistributeTrendByTime(MarketDataList.ToList()))
            {
                if (v.Name == TargetObject.Ticker)
                    CalculateDataList.Add(new TimeValueObject() { Time = v.Time, Value = v.Value, Name = v.Name });
            }
            
            CurrentTime = EndTime;
            CurrentValue =da.GetDistanceByTime(  MarketDataList.ToList(),Ticker);
            OnPropertyChanged("CurrentValue");

            DistributeTrendList.Clear();
            var trendList = da.GetDistributeTrendByTime(MarketDataList.ToList());
            trendList.ForEach(v =>
            {
                if (v.Name == TargetObject.Ticker)
                    DistributeTrendList.Add(new TimeValueObject() { Time = v.Time, Value = v.Value, Name = v.Name });
            });
        }

        IMarketData _CurrentMarketData;
        public IMarketData CurrentMarketData
        {
            get { return _CurrentMarketData; }
            set {
                _CurrentMarketData = value;
                OnPropertyChanged("CurrentMarketData");
                CurrentTime = value.Time;
            }
        }

        DateTime _CurrentTime;
        public DateTime CurrentTime
        {
            get { return _CurrentTime; }
            set {
                if (value < StartTime || value > EndTime) return;
                _CurrentTime = value;
                OnPropertyChanged("CurrentTime");
                var data = CalculateDataList.FirstOrDefault(v => v.Time == value);
                if (data == null) return;
                CurrentValue=CommonLib. Distribute.GetDistance(data.DoubleValue, CalculateDataList.Select(v => v.DoubleValue).ToList());
                OnPropertyChanged("CurrentValue");
            }
        }
    }
}
