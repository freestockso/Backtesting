
using UUBacktesting.View;
using BackTestingCommonLib;
using BackTestingInterface;
using CommonLib;
using CommonLibForWPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Globalization;
using System.Windows.Media;
using BackTestingCommonLib.RiskControl;
using BackTestingCommonLib.Strategy;

namespace UUBacktesting.ViewModel
{
    class BacktestingProjectViewModel:ProjectViewModelBase
    {

        public override IProject GetTargetProject()
        {
            return TargetProject;
        }
        public BacktestingProjectViewModel(IBacktestingProject project)
        {
            if (project == null)
                throw new Exception("no valid project object");
            TargetProject = project;
            InitContextMenu();
        }

        public bool IsUnlimited { get { return TargetProject.IsUnlimited; } set { TargetProject.IsUnlimited = value; OnPropertyChanged("IsUnlimited"); } }

        public bool UseFirstMarketDataInit
        {
            get { return TargetProject.UseFirstMarketDataInit; }
            set { TargetProject.UseFirstMarketDataInit = value; OnPropertyChanged("UseFirstMarketDataInit"); }
        }
        #region Observableconllection

        ObservableCollection<IOrder> _OrderList = new ObservableCollection<IOrder>();
        
        ObservableCollection<TimeValueObject> _CurrentValueList = new ObservableCollection<TimeValueObject>();
        ObservableCollection<TimeValueObject> _StandardValueList = new ObservableCollection<TimeValueObject>();

        public ObservableCollection<IOrder> OrderList { get { return _OrderList; } }

        public ObservableCollection<TimeValueObject> CurrentValueList
        {
            get { return _CurrentValueList; }
        }
        public ObservableCollection<TimeValueObject> StandardValueList 
        {
            get { return _StandardValueList; }
        }

        ObservableCollection<IOrder> _CurrentInstrumentOrderList = new ObservableCollection<IOrder>();
        public ObservableCollection<IOrder> CurrentInstrumentOrderList { get { return _CurrentInstrumentOrderList; } }
        ObservableCollection<IMarketData> _CurrentInstrumentMarketDataList = new ObservableCollection<IMarketData>();
        public ObservableCollection<IMarketData> CurrentInstrumentMarketDataList { get { return _CurrentInstrumentMarketDataList; } }

        #endregion

        IBacktestingProject _TargetProject = null;
        public IBacktestingProject TargetProject
        {
            get { return _TargetProject; }
            set
            {
                if (value != null)
                {
                    _TargetProject = value;

                    LoadInfo();
                    OnPropertyChanged("TargetProject");
                    IsChanged = false;
                }
            }
        }

        public double MaxLost
        {
            get { return TargetProject.MaxLost.Number; }
        }
        public double Efficiency
        {
            get { return TargetProject.Efficiency; }
        }
        public double CurrentValue
        {
            get { return TargetProject.CurrentValue.Number; }
        }


        public MarketDataGrade AnalyseGrade { get { return TargetProject.AnalyseGrade; } set { TargetProject.AnalyseGrade = value; ; OnPropertyChanged("AnalyseGrade"); } }

        public DateTime AnalyseStartTime { get { return TargetProject.AnalyseStartTime; } set { TargetProject.AnalyseStartTime = value;OnPropertyChanged("AnalyseStartTime"); OnPropertyChanged("TestStartTime"); } }
        public override void SaveToFile()
        {
            var dlg = new SaveFileDialog() { Filter = "back testing project|*.btproject|(*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                SaveInfo();
                TargetProject.SerialObject();
                CommonLib.CommonProc.SaveObjToFile(TargetProject, dlg.FileName);
            }
        }
        public override void LoadFromFile()
        {
            var dlg = new OpenFileDialog() { Filter = "back testing project|*.btproject|(*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                var p = CommonLib.CommonProc.LoadObjFromFile<BacktestingProject>(dlg.FileName);
                if (p != null)
                {
                    p.RecoverySerialObject();
                    TargetProject = p;
                    LoadInfo();
                }
            }
        }

        public override void SaveInfo()
        {
            if (TargetProject == null) return;
            OpenedViewModel.ForEach(v =>
            {
                if (v is IEditableViewModel)
                    if ((v as IEditableViewModel).IsChanged)
                        (v as IEditableViewModel).Save();
            });

            TargetProject.ConditionList.Clear();
            foreach (var obj in ConditionList)
            {
                if (obj.TargetObject != null && obj.TargetObject is ICondition)
                {
                    var c = obj.TargetObject as ICondition;
                    TargetProject.ConditionList.Add(c);
                    c.SaveToParameterList();
                }
            }

            TargetProject.InstrumentList.Clear();
            foreach(var obj in InstrumentList)
            {
                TargetProject.InstrumentList.Add(obj);
            }

            IsChanged = false;

            TargetSummaryVM.Refresh();

        }

        public override void LoadInfo()
        {
            if (TargetProject == null) return;

            ConditionList.Clear();
            TargetProject.ConditionList.ForEach(v => ConditionList.Add(new ConditionViewModel() { TargetObject = v }));
            InstrumentList.Clear();
            TargetProject.InstrumentList.ForEach(v => InstrumentList.Add(v));
            CurrentStrategy = TargetProject.TestStrategy;
            CurrentTradeGate = TargetProject.CurrentTradeGate;
            CurrentDataSource = TargetProject.CurrentDataSource;
            IsChanged = false;
            if(TargetSummaryVM!=null)
                TargetSummaryVM.Refresh();
        }

        public string TestCurrentTimeInfo
        {
            get
            {
                if (TargetProject.Status == ProjectStatus.Running)
                    return ProcessInfo;
                return TestStartTime.ToString() + "-" + TestEndTime.ToString();
            }
        }
        public DateTime TestStartTime
        {
            get { if (TargetProject != null) return TargetProject.TestStartTime; return new DateTime(); }
            set { if (TargetProject != null) { TargetProject.TestStartTime = value; OnPropertyChanged("TestStartTime"); OnPropertyChanged("TestCurrentTimeInfo"); OnPropertyChanged("AnalyseStartTime"); IsChanged = true; } }
        }
        public DateTime TestEndTime
        {
            get { if (TargetProject != null) return TargetProject.TestEndTime; return new DateTime(); }
            set { if (TargetProject != null) { TargetProject.TestEndTime = value; OnPropertyChanged("TestEndTime"); OnPropertyChanged("TestCurrentTimeInfo"); IsChanged = true; } }
        }
        public DateTime TestCurrentTime
        {
            get { if (TargetProject != null) return TargetProject.TestCurrentTime; return new DateTime(); }
            set { if (TargetProject != null) { TargetProject.TestCurrentTime = value; OnPropertyChanged("TestCurrentTime"); IsChanged = true; } }

        }

        void refreshChartAxis()
        {
            OnPropertyChanged("MaxPrice");
            OnPropertyChanged("MinPrice");
            OnPropertyChanged("MaxVolumeAxisValue");
            OnPropertyChanged("MinVolumeAxisValue");
        }
        double GetPriceDis()
        {
            return CurrentInstrumentMarketDataList.Max(v => v.High) - CurrentInstrumentMarketDataList.Min(v => v.Low);
        }
        double GetVolumeDis()
        {
            return CurrentInstrumentMarketDataList.Max(v => v.Volume) - CurrentInstrumentMarketDataList.Min(v => v.Volume);
        }
        public double MaxPrice
        {
            get {
                if (CurrentInstrumentMarketDataList.Count == 0) return 100;
                return CurrentInstrumentMarketDataList.Max(v => v.High)+ (GetPriceDis()* MarginPercent);
            }
        }
        public double MinPrice
        {
            get
            {
                if (CurrentInstrumentMarketDataList.Count == 0) return 0;
                var h = GetPriceDis() * (MarginPercent * 2 + 1);
                return MaxPrice - h /MainChartPercent;
            }
        }

        public double MaxVolumeAxisValue
        {
            get {
                if (CurrentInstrumentMarketDataList.Count == 0) return 1000;
                var h = GetVolumeDis() * (MarginPercent * 2 + 1);
                return MinVolumeAxisValue + h / (1 - MainChartPercent);
            }
            
        }
        public double MinVolumeAxisValue
        {
            get
            {
                if (CurrentInstrumentMarketDataList.Count == 0) return 0;
                var min = CurrentInstrumentMarketDataList.Min(v => v.Volume);
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
        public ITradeGate CurrentTradeGate
        { get
            {
                if (TargetProject != null)
                    return TargetProject.CurrentTradeGate; return null;
            } set { if (TargetProject != null) TargetProject.CurrentTradeGate = value.Clone() as ITradeGate; OnPropertyChanged("CurrentTradeGate"); } }
        public List<ITradeGate> ValidTradeGateList
        {
            get { return MainViewModel.Resource.TradeGatePrototypeList; }
        }

        public IStrategy CurrentStrategy { get { if (TargetProject != null) return TargetProject.TestStrategy; return null; }
            set { if (TargetProject != null) TargetProject.TestStrategy = value.Clone() as IStrategy; OnPropertyChanged("CurrentStrategy"); } }
        public IRiskControl RiskPolicy
        {
            get { if (TargetProject != null) return TargetProject.RiskPolicy; return null; }
            set { if (TargetProject != null) TargetProject.RiskPolicy = value.Clone() as IRiskControl; OnPropertyChanged("RiskPolicy"); }
        }
        public List<IRiskControl> ValidRiskControlList
        {
            get { return MainViewModel.Resource.RiskControlPrototypeList; }
        }
        public List<IStrategy> ValidStrategyList
        {
            get { return MainViewModel.Resource.StrategyPrototypeList; }
        }

        public CommonCommand AddToPortfolioCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (TargetProject == null) return;
                    if (CurrentInstrument == null || CurrentInstrument== null) return;
                    if (!TargetProject.TargetPortfolio.PositionList.Any(p => p.InstrumentTicker == CurrentInstrument.Ticker))
                        TargetProject.TargetPortfolio.PositionList.Add(new Position() { InstrumentName= CurrentInstrument.Name,InstrumentTicker=CurrentInstrument.Ticker,CurrentPrice=CurrentInstrument.CurrentPrice });
                });
            }
        }

        IInstrument _CurrentInstrument = null;
        public override IInstrument CurrentInstrument
        {
            get { return _CurrentInstrument; }
            set
            {
                if (_CurrentInstrument == value || value == null) return;
                if (_CurrentInstrument == null || _CurrentInstrument.Ticker != value.Ticker)
                {
                    _CurrentInstrument = value;
                    OnPropertyChanged("CurrentInstrument");
                    CurrentInstrumentMarketDataList.Clear();
                    MarketDataList.Where(v => v.InstrumentTicker == value.Ticker).ToList().ForEach(v => CurrentInstrumentMarketDataList.Add(v));
                    CurrentInstrumentOrderList.Clear();
                    OrderList.Where(v => v.Ticker == value.Ticker).ToList().ForEach(v => CurrentInstrumentOrderList.Add(v));
                    refreshChartAxis();
                }
            }
        }

        public override void Refresh()
        {
            CheckRunningStatus();
            CommonProc.SynchroniseList<IMarketData>(TargetProject.MarketDataList, MarketDataList);
            //CommonProc.SynchroniseList<IIndicatorValue>(TargetProject.IndicatorDataList, IndicatorDataList);
            CommonProc.SynchroniseList<IOrder>(TargetProject.OrderList, OrderList);
            CommonProc.SynchroniseList<TimeValueObject>(TargetProject.CurrentValueList, CurrentValueList);
            CommonProc.SynchroniseList<TimeValueObject>(TargetProject.StandardValueList, StandardValueList);
            if (CurrentInstrument == null && InstrumentList.Count > 0)
                CurrentInstrument = InstrumentList.FirstOrDefault();
            if (CurrentInstrument != null)
            {
                var l = MarketDataList.Where(v => v.InstrumentTicker == CurrentInstrument.Ticker);
                CommonProc.SynchroniseList<IMarketData>(l, CurrentInstrumentMarketDataList);
                var ol = OrderList.Where(v => v.Ticker == CurrentInstrument.Ticker);
                CommonProc.SynchroniseList<IOrder>(ol, CurrentInstrumentOrderList);
            }

            TestCurrentTime=TargetProject.TestCurrentTime;
            OnPropertyChanged("CurrentValue");
            OnPropertyChanged("Pnl");
            OnPropertyChanged("StatusPnl");
            OnPropertyChanged("Efficiency");
            OnPropertyChanged("MaxLost");
            refreshChartAxis();
            OnPropertyChanged("CanStart");
            OnPropertyChanged("CanPause");
            OnPropertyChanged("CanStop");
            OnPropertyChanged("FinishPercent");
            if (TargetSummaryVM != null)
                TargetSummaryVM.Refresh();
            OnPropertyChanged("CanStart");
            OnPropertyChanged("CanPause");
            OnPropertyChanged("CanStop");
            OnPropertyChanged("TestCurrentTimeInfo");
            base.Refresh();
        }

        public CommonCommand OpenControlCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    var c = new BacktestingProjectControl() { DataContext = this };
                    if(OpenView!=null)
                        OpenView("Result View", c,false);
                });
            }
        }

        public CommonCommand OpenInstrumentCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    OpenInstrument(CurrentInstrument);
                });
            }
        }
        public void OpenCondition(ConditionViewModel condition)
        {
            if (condition != null&&condition.TargetObject!=null)
            {
                condition.AnalyseStartTime = AnalyseStartTime;
                condition.AnalyseEndTime = TestEndTime;
                condition.AnalyseGrade = AnalyseGrade;
                //condition.TargetObject.GetMarketData = (instrument,start,end,grade) => { return CurrentDataSource.GetDataList(instrument,start,end,grade); };
                condition.TargetObject.AnalyseDataSource = CurrentDataSource;
                var c = new ConditionControl() { DataContext = condition };
                if (OpenView != null)
                    OpenView("Name", c,true);
            }
        }

        public override bool CanRemove(IInstrument instrument)
        {
            if (TargetProject == null || TargetProject.TargetPortfolio == null) return true;
            if (TargetProject.TargetPortfolio.PositionList.Any(v => v.InstrumentTicker == instrument.Ticker))
                return false;
            return true;
        }

        public CommonCommand OpenConditionCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    OpenCondition(CurrentCondition);
                });
            }
        }
        public CommonCommand OpenPortfolioCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    var vm = new PortfolioViewModel() { TargetObject = TargetProject.TargetPortfolio };
                    var c = new View.PortfolioControl() { DataContext =  vm};
                    if (OpenView != null)
                        OpenView("Edit Portfolio", c,false);
                });
            }
        }
        public CommonCommand OpenStrategyCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    FrameworkElement control;
                    if (CurrentStrategy is StandardStrategy)
                    {
                        var s = new StandardStrategyViewModel() { TargetObject = CurrentStrategy as StandardStrategy};
                        control = new StandardStrategyControl() { DataContext = s };
                    }
                    else {
                        var s = new StrategyViewModel() { TargetObject = CurrentStrategy };
                        control = new StrategyControl() { DataContext = s };

                    }
                    if (OpenView != null)
                        OpenView("Name", control,true);
                });
            }
        }
        public CommonCommand OpenRiskControlCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    FrameworkElement control;
                    var s = new RiskControlViewModel() { TargetObject = RiskPolicy as IRiskControl };

                    control = new RiskControlView() { DataContext = s };
                    if (OpenView != null)
                        OpenView("Name", control,true);
                });
            }
        }
        public CommonCommand OpenDataSourceCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    //LoadCurrentInstrumentMarketData(TestStartTime, TestEndTime);
                    var c = new DataSourceControl() { DataContext = this };
                    if (OpenView != null)
                        OpenView(CurrentDataSource.Name, c,false);
                });
            }
        }
        public CommonCommand OpenTradeGateCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (OpenView != null)
                        OpenView(CurrentTradeGate.Name, new TradeGateControl() { DataContext = this },false);
                });
            }
        }

        public int DelayMs
        {
            get
            {
                return TargetProject.CurrentTradeGate.DelayMs;
            }
            set
            {
                TargetProject.CurrentTradeGate.DelayMs = value;OnPropertyChanged("DelayMs");
            }
        }
        public int CacheSize
        {
            get
            {
                return TargetProject.CurrentDataSource.CacheSize;
            }
            set
            {
                TargetProject.CurrentDataSource.CacheSize = value; OnPropertyChanged("CacheSize");
            }
        }
    }


}
