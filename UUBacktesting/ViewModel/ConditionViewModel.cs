using UUBacktesting.View;
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
using Telerik.Windows.Controls;
using CommonLib;

namespace UUBacktesting.ViewModel
{
    public class ConditionViewModel:ParameterObjectViewModelBase<ICondition>, IObersverSupport
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
        IInstrument _AnalyseInstrument;
        public IInstrument AnalyseInstrument
        {
            get { return _AnalyseInstrument; }
            set { _AnalyseInstrument = value; OnPropertyChanged("AnalyseInstrumentName"); }
        }
        public string AnalyseInstrumentName
        {
            get { if (_AnalyseInstrument != null) return _AnalyseInstrument.Name;return "No analyse instrument"; }
        }
        List<MarketDataGrade> _GradeList = new List<MarketDataGrade>() { MarketDataGrade.FiveMinutes, MarketDataGrade.FifteenMinutes, MarketDataGrade.HalfHour, MarketDataGrade.Hour, MarketDataGrade.HalfDay, MarketDataGrade.Day, MarketDataGrade.ThreeDays, MarketDataGrade.Week, MarketDataGrade.HalfMonth, MarketDataGrade.Month, MarketDataGrade.Season, MarketDataGrade.HalfYear, MarketDataGrade.Year };
        public List<MarketDataGrade> GradeList { get { return _GradeList; } }
        public DateTime AnalyseStartTime { get; set; }
        public DateTime AnalyseEndTime { get; set; }
        public MarketDataGrade AnalyseGrade { get; set; }
        public IInstrument CurrentInstrument { get; set; }
        public CalculateItem CurrentCalculateItem { get; set; }
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
        public override CommonCommand LoadCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    Load();
                    Refresh();
                });
            }
        }
        TelerikChartViewHelper helper = new TelerikChartViewHelper();
        public RadCartesianChart Chart { get; set; }
        bool _ShowAllAnalyseResult = true;
        public bool ShowAllAnalyseResult
        {
            get { return _ShowAllAnalyseResult; }
            set { _ShowAllAnalyseResult = value; OnPropertyChanged("ShowAllAnalyseResult"); RefreshAdditionalSeries(); }
        }
        ObservableCollection<LendItem> _LendList = new ObservableCollection<LendItem>();
        public ObservableCollection<LendItem> LendList { get { return _LendList; } }

        //Dictionary<string, List<TimeValueObject>> GetReference()
        //{
        //    var r = TargetObject.GetResult();
        //    if (r == null) return null;
        //    var d = new Dictionary<string, List<TimeValueObject>>();
        //    r = r.Where(v => v.ResultType == SignalType.Analyse).ToList();
        //    var kl = r.Select(v => v.Name).Distinct().ToList();
        //    kl.ForEach(k =>
        //    {
        //        var vl = r.Where(v => v.Name == k).ToList();
        //        var tl = new List<TimeValueObject>();
        //        vl.ForEach(v =>
        //        {
        //            var obj = new TimeValueObject() { Value = v.Value, Name = v.Name, TargetName = v.InstrumentName, Time = v.Time };
        //            tl.Add(obj);
        //        });
        //        d.Add(k, tl);
        //    });
        //    return d;
        //}
        void RefreshAdditionalSeries()
        {
            try
            {
                var sl = Chart.Series.Where(v => v.Name.StartsWith("Analyse")).ToList();
                Chart.Series.Clear();
                sl.ForEach(v => Chart.Series.Add(v));
                if (ShowAllAnalyseResult)
                {

                    if (ShowAllAnalyseResult && Chart != null)
                    {
                        bool needAxis = false;
                        if (TargetObject.IsReferenceIndependence)
                            needAxis = true;

                        var l=helper.AddSeriesToChart(TargetObject.ReferenceValueList, Chart,needAxis);
                        LendList.Clear();
                        foreach (var v in l)
                        {
                            LendList.Add(v);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

            }
        }
        public CommonCommand AnalyseCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (AnalyseInstrument == null)
                    {
                        MessageBox.Show("No valid instrument, please add a instrument!");
                    }
                    else if (TargetObject.AnalyseDataSource == null)
                    {
                        MessageBox.Show("No valid data source, please select data source!");
                    }
                    else{

                        TargetObject.AnalyseResult(AnalyseInstrument, AnalyseStartTime, AnalyseEndTime, AnalyseGrade);
                        AnalyseResultList.Clear();
                        if (TargetObject.ReferenceValueList.ContainsKey(TargetObject.Name))
                        {
                            var dl = TargetObject.ReferenceValueList[TargetObject.Name];


                            dl.ForEach(v =>
                            {
                                AnalyseResultList.Add(v);
                            });
                            TargetObject.ReferenceValueList.Remove(TargetObject.Name);
                        }
                        var ml = TargetObject.AnalyseDataSource.GetDataList(AnalyseInstrument, AnalyseStartTime, AnalyseEndTime, AnalyseGrade);
                        MarketDataList.Clear();
                        ml.ForEach(v => MarketDataList.Add(v));

                        
                        if(o !=null&& o is RadCartesianChart)
                        {
                            Chart = o as RadCartesianChart;
                        }
                        RefreshAdditionalSeries();
                        refreshChartAxis();

                    }
                });
            }
        }
        public CommonCommand OpenInstrumentCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if(CurrentInstrument!=null)
                        InstrumentControl.ShowInstrumentInfo(CurrentInstrument as IInstrument);
                });
            }
        }
        public CommonCommand OpenCalculateCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (CurrentCalculateItem != null)
                    {
                        var inst = new Instrument() { Ticker = CurrentCalculateItem.Ticker, Name = CurrentCalculateItem.Name };
                        InstrumentControl.ShowInstrumentInfo(inst);
                    }
                });
            }
        }
        public CommonCommand TestCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    ResultList.Add(new Instrument() { Ticker = "000001", Name = "PYbank" });
                    ResultList.Add(new Instrument() { Ticker = "000002", Name = "pwefwe" });
                    ResultList.Add(new Instrument() { Ticker = "000003", Name = "xxxx" });
                });
            }
        }
        public CommonCommand RefreshCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    Refresh();
                });
            }
        }
        public CommonCommand ClearCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    ResultList.Clear();
                    CalculateList.Clear();
                });
            }
        }
        public CommonCommand SetAnalyseCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (CurrentInstrument != null)
                    {
                        AnalyseInstrument = CurrentInstrument;
                    }
                });
            }
        }
        public CommonCommand SetAnalyseCalculateCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (CurrentCalculateItem != null)
                    {
                        AnalyseInstrument = new Instrument() { Ticker = CurrentCalculateItem.Ticker, Name = CurrentCalculateItem.Name };
                    }
                });
            }
        }
        ObservableCollection<IMarketData> _MarketDataList = new ObservableCollection<IMarketData>();
        public ObservableCollection<IMarketData> MarketDataList
        {
            get { return _MarketDataList; }
        }
        ObservableCollection<TimeValueObject> _AnalyseResultList = new ObservableCollection<TimeValueObject>();
        public ObservableCollection<TimeValueObject> AnalyseResultList { get { return _AnalyseResultList; } }

        ObservableCollection<CalculateItem> _CalculateList = new ObservableCollection<CalculateItem>();
        public ObservableCollection<CalculateItem> CalculateList { get { return _CalculateList; } }
        ObservableCollection<IInstrument> _ResultList = new ObservableCollection<IInstrument>();
        public ObservableCollection<IInstrument> ResultList { get { return _ResultList; } }

        public void Refresh()
        {
            ResultList.Clear();
            var rl = TargetObject.GetResult();
            if (rl == null) return;
            var l = new List<IInstrument>();
            foreach(var r in rl)
            {
                l.Add(Instrument.AllInstrumentList.FirstOrDefault(v => v.Ticker == r.Ticker));
                
            }
            l.ForEach(v => ResultList.Add(v));
            CalculateList.Clear();
            foreach (var kv in rl)
                CalculateList.Add(new CalculateItem() { Name = kv.InstrumentName, Ticker = kv.Ticker, Value = kv.Value });

        }

        public bool NeedRefresh()
        {
            return true;
        }
    }
    public class CalculateItem
    {
        public string Name { get; set; }
        public string Ticker { get; set; }
        public double Value { get; set; }
    }
}
