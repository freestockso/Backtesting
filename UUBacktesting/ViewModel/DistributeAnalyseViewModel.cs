using BackTestingCommonLib;
using BackTestingInterface;
using CommonLib;
using CommonLibForWPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using UUBacktesting.View;
using UUBacktesting.View.Distribute;
using UUBacktesting.ViewModel.Distribute;

namespace UUBacktesting.ViewModel
{
    class DistributeAnalyseViewModel:ViewModelBase
    {
        DistributeAnalyse _TargetObject = new DistributeAnalyse();
        public DistributeAnalyse TargetObject
        {
            get { return _TargetObject; }
            set { _TargetObject = value;
                OnPropertyChanged("StartTime");
                OnPropertyChanged("EndTime");
                OnPropertyChanged("Grade");
                OnPropertyChanged("DistributeStep");
                OnPropertyChanged("IsIncludeZero");
                OnPropertyChanged("AnalyseType");
                OnPropertyChanged("ProcessMode");
            }
        }
        void Load(DistributeAnalyse obj)
        {
            TargetObject = obj;
            InstrumentList.Clear();
            TargetObject.InstrumentList.ForEach(v => InstrumentList.Add(v));
            SectorList.Clear();
            TargetObject.SectorList.ForEach(v => SectorList.Add(v));
            CurrentDataSource = TargetObject.CurrentDataSource;
        }
        void Save()
        {
            TargetObject.InstrumentList.Clear();
            foreach (var inst in InstrumentList)
                TargetObject.InstrumentList.Add(inst);
            TargetObject.SectorList.Clear();
            foreach (var v in SectorList)
                TargetObject.SectorList.Add(v);
            TargetObject.CurrentDataSource = CurrentDataSource.Clone() as IDataSource;

        }
        public int DistributeStep
        {
            get { return TargetObject.StatisticStep; }
            set { TargetObject.StatisticStep = value; OnPropertyChanged("DistributeStep"); }
        }

        public bool IsIncludeZero
        {
            get { return TargetObject.IsIncludeZero; }
            set { TargetObject.IsIncludeZero = value; OnPropertyChanged("IsIncludeZero"); }
        }

        public DateTime StartTime
        {
            get { return TargetObject.StartTime; }
            set { TargetObject.StartTime = value; OnPropertyChanged("StartTime"); }
        }

        public DateTime EndTime
        {
            get { return TargetObject.EndTime; }
            set { TargetObject.EndTime = value; OnPropertyChanged("EndTime"); }
        }

        public MarketDataGrade Grade { get { return TargetObject.Grade; } set { TargetObject.Grade = value; OnPropertyChanged("Grade"); } }

        public DistributeAnalyseType AnalyseType
        {
            get { return TargetObject.AnalyseType; }
            set
            {
                TargetObject.AnalyseType = value;
                OnPropertyChanged("AnalyseType");
                OnPropertyChanged("InstrumentListBrush");
                OnPropertyChanged("SectorListBrush");
                
                if (TargetObject.AnalyseType== DistributeAnalyseType.Instrument)
                {
                    _ProcessModeList.Clear();
                       var l = DistributeAnalyse.GetProcessModeList();
                    l.ForEach(v => _ProcessModeList.Add(v));
                }
                else
                {
                    if (ProcessMode != DistributeValueProcessMode.VolumeChangePercent && ProcessMode != DistributeValueProcessMode.VolumeChange && ProcessMode != DistributeValueProcessMode.Volume)
                        ProcessMode = DistributeValueProcessMode.VolumeChange;
                    _ProcessModeList.Clear();
                    _ProcessModeList.Add(DistributeValueProcessMode.Volume);
                    _ProcessModeList.Add(DistributeValueProcessMode.VolumeChange);
                    _ProcessModeList.Add( DistributeValueProcessMode.VolumeChangePercent);
                }
            }
        }

        public DistributeValueProcessMode ProcessMode
        {
            get { return TargetObject.ProcessMode; }
            set { TargetObject.ProcessMode = value; OnPropertyChanged("ProcessMode"); }
        }

        public List<MarketDataGrade> AllGradeList { get { return MarketData.GetMarketDataGradeList(); } }
        public List<DistributeAnalyseType> AnalyseTypeList { get { return DistributeAnalyse.GetDistributeAnalyseTypeList(); } }

        ObservableCollection<DistributeValueProcessMode> _ProcessModeList = new ObservableCollection<DistributeValueProcessMode>();
        public ObservableCollection<DistributeValueProcessMode> ProcessModeList
        {
            get
            {
                if (_ProcessModeList.Count == 0)
                {
                    var l= DistributeAnalyse.GetProcessModeList();
                    l.ForEach(v => _ProcessModeList.Add(v));
                }
                return _ProcessModeList;
            }
        }

        ObservableCollection<IInstrument> _InstrumentList = new ObservableCollection<IInstrument>();
        public ObservableCollection<IInstrument> InstrumentList { get { return _InstrumentList; } }
        ObservableCollection<string> _SectorList = new ObservableCollection<string>();
        public ObservableCollection<string> SectorList { get { return _SectorList; } }

        ObservableCollection<IInstrument> _AllInstrumentList = new ObservableCollection<IInstrument>();
        public ObservableCollection<IInstrument> AllInstrumentList
        {
            get
            {
                _AllInstrumentList.Clear();
                Instrument.AllInstrumentList.ForEach(v =>
                {
                    _AllInstrumentList.Add(v);
                });
                return _AllInstrumentList;
            }
        }

        ObservableCollection<string> _IndustoryList = new ObservableCollection<string>();
        public ObservableCollection<string> IndustoryList
        {
            get
            {
                _IndustoryList.Clear();
                Instrument.AllIndustoryList.ForEach(v =>
                {
                    _IndustoryList.Add(v);
                });
                return _IndustoryList;
            }
        }

        ObservableCollection<string> _RegionList = new ObservableCollection<string>();
        public ObservableCollection<string> RegionList
        {
            get
            {
                _RegionList.Clear();
                Instrument.AllRegionList.ForEach(v =>
                {
                    _RegionList.Add(v);
                });
                return _RegionList;
            }
        }

        public IInstrument CurrentInstrument { get; set; }
        public string CurrentSector { get; set; }

        Brush activeBrush = new SolidColorBrush(Colors.Orange);
        Brush deactiveBrush= new SolidColorBrush(Colors.LightGray);

        public Brush InstrumentListBrush
        {
            get { if (AnalyseType == DistributeAnalyseType.Instrument) return activeBrush; return deactiveBrush; }
        }
        public Brush SectorListBrush
        {
            get { if (AnalyseType == DistributeAnalyseType.Instrument) return deactiveBrush; return activeBrush; }
        }

        IDataSource _CurrentDataSource;
        public IDataSource CurrentDataSource
        {
            get
            {
                if (_CurrentDataSource == null)
                {
                    _CurrentDataSource = MainViewModel.Resource.DataSourcePrototypeList.FirstOrDefault().Clone() as IDataSource;
                }
                return _CurrentDataSource;
            }
            set { _CurrentDataSource = value.Clone() as IDataSource; OnPropertyChanged("CurrentDataSource"); }
        }
        public List<IDataSource> ValidDataSourceList
        {
            get { return MainViewModel.Resource.DataSourcePrototypeList; }
        }
        public Action<string, FrameworkElement> OpenView { get; set; }
        public void OpenInstrument(IInstrument instrument)
        {
            if (instrument != null)
            {
                var c = new InstrumentControl() { DataContext = 
                    new InstrumentViewModel()
                    { TargetObject = instrument.Clone() as IInstrument, GetCurrentDataSource = () => { return CurrentDataSource; }
                    ,StartTime=StartTime,EndTime=EndTime}
                };
                if (OpenView != null)
                    OpenView(instrument.Name, c);
            }
        }
        string GetHeader()
        {
            var s = "";
            if(AnalyseType== DistributeAnalyseType.Instrument)
            {
                foreach (var i in InstrumentList)
                    s += i.Ticker + ",";
            }
            else
            {
                foreach (var sec in SectorList)
                    s += sec + ",";
            }
            return s.Substring(0,s.Length-1);
        }

        bool CanStart()
        {
            if (CurrentDataSource == null
                || (InstrumentList.Count == 0 && AnalyseType == DistributeAnalyseType.Instrument)
                || (SectorList.Count == 0 && AnalyseType != DistributeAnalyseType.Instrument))
            {
                MessageBox.Show("Please select data source or instrument or sector");
                return false; 
            }
            return true;
        }
        void TimeDistribute()
        {
            if (!CanStart())
            {
                return;
            }
            var control = new DistributeControl();
            var vm = new DistributeViewModel() { IsBusy=true};
            control.DataContext = vm;
            OpenView("Distribute Time("+ProcessMode.ToString()+"):" + GetHeader(), control);
            Task.Factory.StartNew(() =>
            {
                try {
                    Save();
                    var ml = TargetObject.GetMarketDataList();

                    var dl = TargetObject.GetDistributeByTime(ml);
                    var tl = TargetObject.GetDistributeTrendByTime(ml);
                    App.Current.Dispatcher.Invoke(() => { vm.LoadData(dl, tl); });
                }
                catch (Exception ex)
                {
                    LogSupport.Error(ex);
                }
            }).ContinueWith((t) =>
            {
                vm.IsBusy = false;
            });
        }
        void MarketDistribute()
        {
            if (!CanStart())
            {
                return;
            }
            var control = new DistributeControl();
            var vm = new DistributeViewModel() { IsBusy = true };
            control.DataContext = vm;
            OpenView("Distribute Market(" + ProcessMode.ToString() + "):" + GetHeader(), control);
            Task.Factory.StartNew(() =>
            {
                try {
                    Save();
                    var ml = TargetObject.GetMarketDataList();

                    var dl = TargetObject.GetDistributeByMarket(ml);
                    var tl = TargetObject.GetDistributeTrendByMarket(ml);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        vm.LoadData(dl, tl);
                    }); }
                catch (Exception ex)
                {
                    LogSupport.Error(ex);
                }
            }).ContinueWith((t) =>
            {
                vm.IsBusy = false;
            });
        }

        void TimeTrend()
        {
            if (!CanStart())
            {
                return;
            }
            var control = new DistributeTrendControl();
            var vm = control.DataContext as DistributeTrendViewModel;
            vm.IsTimeMode = true;
            OpenView("Trend Time(" + ProcessMode.ToString() + "):" + GetHeader(), control);
            Task.Factory.StartNew(() =>
            {
                Save();
                var ml = TargetObject.GetMarketDataList();
                var tl = TargetObject.GetDistributeTrendByTime(ml);
                App.Current.Dispatcher.Invoke(() =>
                {
                    vm.LoadData(tl);
                });

            }).ContinueWith((t) =>
            {
                vm.IsBusy = false;
            });
        }
        void MarketTrend()
        {
            if (!CanStart())
            {
                return;
            }
            var control = new DistributeTrendControl();
            var vm = control.DataContext as DistributeTrendViewModel;
            vm.IsTimeMode = false;
            OpenView("Trend Market(" + ProcessMode.ToString() + "):" + GetHeader(), control);
            Task.Factory.StartNew(() =>
            {
                try {
                    Save();
                    var ml = TargetObject.GetMarketDataList();
                    var tl = TargetObject.GetDistributeTrendByMarket(ml);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        vm.LoadData(tl);
                    });
                }
                catch (Exception ex)
                {
                    LogSupport.Error(ex);
                }
            }).ContinueWith((t) =>
            {
                vm.IsBusy = false;
            });
        }
        void DimentionAnalyse()
        {
            var control = new DimentionAnalyseControl();
            var vm = new DimentionAnalyseControlViewModel() { IsBusy = true };
            control.DataContext = vm;
            OpenView("Dimention Analyse(" + ProcessMode.ToString() + "):" + GetHeader(), control);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Save();
                    var rl = TargetObject.GetDimentionAnalyse();


                    App.Current.Dispatcher.Invoke(() =>
                    {
                        vm.LoadData(rl);
                    });
                }
                catch (Exception ex)
                {
                    LogSupport.Error(ex);
                }
            }).ContinueWith((t) =>
            {
                vm.IsBusy = false;
            });

        }

        void Spot()
        {
            var control = new SpotControl();
            var vm = new SpotControlViewModel() { IsBusy = true };
            control.DataContext = vm;
            OpenView("2 Matrix Analyse(" + ProcessMode.ToString() + "):" + GetHeader(), control);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Save();
                    Dictionary<string, List<Tuple<double, double>>> rl = TargetObject.Get2DCurve();


                    App.Current.Dispatcher.Invoke(() =>
                    {
                        vm.LoadData(rl);
                    });
                }
                catch (Exception ex)
                {
                    LogSupport.Error(ex);
                }
            }).ContinueWith((t) =>
            {
                vm.IsBusy = false;
            });

        }
        void Surface()
        {
            var control = new SurfaceControl();
            var vm = new SurfaceControlViewModel() { IsBusy = true };
            control.DataContext = vm;
            OpenView("3 Matrix Analyse(" + ProcessMode.ToString() + "):" + GetHeader(), control);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Save();
                    Dictionary<string, List<Tuple<double, double,double>>> rl = TargetObject.Get3DCurve();


                    App.Current.Dispatcher.Invoke(() =>
                    {
                        vm.LoadData(rl);
                    });
                }
                catch (Exception ex)
                {
                    LogSupport.Error(ex);
                }
            }).ContinueWith((t) =>
            {
                vm.IsBusy = false;
            });
        }

        void TimeSurface()
        {
            if (!CanStart())
            {
                return;
            }
            var control = new DistributeSurfaceControl();
            var vm = new DistributeSurfaceViewModel() { IsBusy = true };
            control.DataContext = vm;
            OpenView("Trend Time(" + ProcessMode.ToString() + "):" + GetHeader(), control);
            Task.Factory.StartNew(() =>
            {
                try {
                    Save();
                    var ml = TargetObject.GetMarketDataList();
                    var tl = TargetObject.GetDistributeTrendByTime(ml);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        vm.LoadData(tl);
                    }); }
                catch (Exception ex)
                {
                    LogSupport.Error(ex);
                }
            }).ContinueWith((t) =>
            {
                vm.IsBusy = false;
            });
        }
        void MarketSurface()
        {
            if (!CanStart())
            {
                return;
            }
            var control = new DistributeSurfaceControl();
            var vm = new DistributeSurfaceViewModel() { IsBusy = true };
            control.DataContext = vm;
            OpenView("Trend Market(" + ProcessMode.ToString() + "):" + GetHeader(), control);
            Task.Factory.StartNew(() =>
            {
                try {
                    Save();
                    var ml = TargetObject.GetMarketDataList();
                    var tl = TargetObject.GetDistributeTrendByMarket(ml);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        vm.LoadData(tl);
                    }); }
                catch (Exception ex)
                {
                    LogSupport.Error(ex);
                }
            }).ContinueWith((t) =>
            {
                vm.IsBusy = false;
            });
        }

        public CommonCommand TimeDistributeCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    TimeDistribute();
                });
            }
        }
        public CommonCommand TimeTrendCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    TimeTrend();
                });
            }
        }
        public CommonCommand MarketDistributeCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    MarketDistribute();
                });
            }
        }
        public CommonCommand MarketTrendCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    MarketTrend();
                });
            }
        }
        public CommonCommand TimeSurfaceCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    TimeSurface();
                });
            }
        }
        public CommonCommand MarketSurfaceCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    MarketSurface();
                });
            }
        }

        public CommonCommand SurfaceCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    Surface();
                });
            }
        }

        public CommonCommand SpotCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    Spot();
                });
            }
        }

        public CommonCommand DimentionAnalyseCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    DimentionAnalyse();
                });
            }
        }

        public CommonCommand LoadCommand
        {
            get
            {
                return new CommonCommand((o) => { Load(TargetObject); });
            }
        }
        public CommonCommand SaveCommand
        {
            get { return new CommonCommand((o) => { Save(); }); }
        }

        public CommonCommand LoadFromFileCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    var dlg = new OpenFileDialog() { Filter = "distribute project|*.distribute|(*.*)|*.*" };
                    if (dlg.ShowDialog() == true)
                    {
                        var p = CommonLib.CommonProc.LoadObjFromFile<DistributeAnalyse>(dlg.FileName);
                        if (p != null)
                        {
                            
                            Load(p);
                        }
                    }
                    Load(TargetObject);
                });
            }
        }
        public CommonCommand SaveToFileCommand
        {
            get
            {
                return new CommonCommand((o) => 
                {
                    var dlg = new SaveFileDialog() { Filter = "distribute project|*.distribute|(*.*)|*.*" };
                    if (dlg.ShowDialog() == true)
                    {
                        Save();
                        CommonLib.CommonProc.SaveObjToFile(TargetObject, dlg.FileName);
                    }
                });
            }
        }

        public CommonCommand RemoveInstrumentCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (CurrentInstrument != null && InstrumentList.Contains(CurrentInstrument))
                        InstrumentList.Remove(CurrentInstrument);
                });
            }
        }
        public CommonCommand RemoveSectorCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (CurrentSector != null && SectorList.Contains(CurrentSector))
                        SectorList.Remove(CurrentSector);
                });
            }
        }
    }
}
