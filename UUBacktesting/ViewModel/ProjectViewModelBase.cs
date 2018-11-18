
using UUBacktesting.View;
using BackTestingInterface;
using CommonLibForWPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace UUBacktesting.ViewModel
{
    public abstract class ProjectViewModelBase:ViewModelBase, IObersverSupport
    {
        public void Filter(string filter)
        {
            InstrumentList.Clear();
            if (!string.IsNullOrEmpty(filter))
            {
                GetTargetProject().InstrumentList.ForEach(v =>
                {
                    if (v.Ticker.IndexOf(filter, StringComparison.OrdinalIgnoreCase)>=0
                    || v.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase)>=0
                    || v.PYName.IndexOf(filter, StringComparison.OrdinalIgnoreCase)>=0)
                        InstrumentList.Add(v);
                });
            }
            else
            {
                GetTargetProject().InstrumentList.ForEach(v =>
                {
                    InstrumentList.Add(v);
                });
            }
        }

        List<INotifiedViewModel> _OpenedViewModel = new List<INotifiedViewModel>();
        public List<INotifiedViewModel> OpenedViewModel {
            get { return _OpenedViewModel; }
        }

        EntityObserver observer = new EntityObserver() { };
        public bool NeedRefresh()
        {
            if (GetTargetProject() != null && GetTargetProject().Status == ProjectStatus.Running) return true;
            return false;
        }
        public void StartObserveViewModel()
        {
            observer.ObersverObject = this;
            observer.IsRunning = true;
        }
        public void StopObserveViewModel()
        {
            observer.IsRunning = false;
        }
        public virtual void Refresh()
        {
            OpenedViewModel.ForEach(v =>
            {
                if(v is IObersverSupport)
                if ((v as IObersverSupport).NeedRefresh())
                    (v as IObersverSupport).Refresh();
            });
            OnPropertyChanged("ProcessInfo");
            OnPropertyChanged("FinishPercent");
        }
        public Action<string,FrameworkElement,bool> OpenView { get; set; }

        public abstract IProject GetTargetProject();
        public int TestStepDelayMS
        {
            get { return GetTargetProject().TestStepDelayMS; }
            set { GetTargetProject().TestStepDelayMS = value;OnPropertyChanged("TestStepDelayMS"); }
        }
        public abstract void LoadInfo();
        public abstract void SaveInfo();
        public abstract void SaveToFile();
        public abstract void LoadFromFile();
        public ProjectSummaryViewModelBase TargetSummaryVM { get; set; }

        List<MarketDataGrade> _GradeList = new List<MarketDataGrade>() { MarketDataGrade.FiveMinutes, MarketDataGrade.FifteenMinutes, MarketDataGrade.HalfHour, MarketDataGrade.Hour, MarketDataGrade.HalfDay, MarketDataGrade.Day, MarketDataGrade.ThreeDays, MarketDataGrade.Week, MarketDataGrade.HalfMonth, MarketDataGrade.Month, MarketDataGrade.Season, MarketDataGrade.HalfYear, MarketDataGrade.Year };
        public List<MarketDataGrade> GradeList { get { return _GradeList; } }
        public MarketDataGrade Grade { get { return GetTargetProject().Grade; } set { GetTargetProject().Grade = value; OnPropertyChanged("Grade"); } }
        ObservableCollection<IInstrument> _InstrumentList = new ObservableCollection<IInstrument>();
        public ObservableCollection<IInstrument> InstrumentList { get { return _InstrumentList; } }
        ObservableCollection<IMarketData> _MarketDataList = new ObservableCollection<IMarketData>();
        public ObservableCollection<IMarketData> MarketDataList { get { return _MarketDataList; } }
        public virtual IInstrument CurrentInstrument { get; set; }
        ObservableCollection<ConditionViewModel> _ConditionList = new ObservableCollection<ConditionViewModel>();
        public ObservableCollection<ConditionViewModel> ConditionList { get { return _ConditionList; } }
        public ConditionViewModel CurrentCondition { get; set; }

        List<CommonCommand> _ConditionMenuList = new List<CommonCommand>();
        public List<CommonCommand> ConditionMenuList { get { return _ConditionMenuList; } }
        public virtual void InitContextMenu()
        {
            ConditionMenuList.Clear();
            MainViewModel.Resource.ConditionPrototypeList.ForEach(v =>
            {
                var command = new CommonCommand((obj) =>
                {
                    var o = v.CreateInstance() as ICondition;
                    o.GetInstrumentList = () => {
                        var l = new List<IInstrument>();
                        foreach (var inst in InstrumentList)
                            l.Add(inst);
                        return l;
                    };

                    o.AnalyseDataSource = CurrentDataSource;
                    var vm = new ConditionViewModel() { TargetObject = o };
                    ConditionList.Add(vm);
                    GetTargetProject().ConditionList.Add(o);
                });
                command.Name = "New " + v.Name;
                command.Memo = v.Memo;
                command.ForegroundBrush = new SolidColorBrush(Colors.Blue);
                ConditionMenuList.Add(command);
            });
            var dcommand = new CommonCommand((obj) =>
            {
                if (CurrentCondition != null && ConditionList.Contains(CurrentCondition))
                    ConditionList.Remove(CurrentCondition);
            });
            dcommand.Name = "Delete Condition";
            dcommand.Memo = "Delete Current Condition";
            dcommand.ForegroundBrush = new SolidColorBrush(Colors.Orange);
            ConditionMenuList.Add(dcommand);
        }
        public void LoadCurrentInstrumentMarketData( DateTime start,DateTime end)
        {
            if (CurrentInstrument == null) return;
            else
            {
                MarketDataList.Clear();
                var ml = CurrentDataSource.GetDataList(new List<IInstrument>() { CurrentInstrument }, start, end,Grade);
                ml.ForEach(v => MarketDataList.Add(v));
            }
        }
        public IDataSource CurrentDataSource
        {
            get
            {
                if (GetTargetProject() != null) return GetTargetProject().CurrentDataSource;
                return null;
            }
            set { if (GetTargetProject() != null) GetTargetProject().CurrentDataSource = value.Clone() as IDataSource; OnPropertyChanged("CurrentDataSource"); }
        }
        public List<IDataSource> ValidDataSourceList
        {
            get { return MainViewModel.Resource.DataSourcePrototypeList; }
        }

        public string Name
        {
            get { if (GetTargetProject() == null) return null; return GetTargetProject().Name; }
            set
            {
                if (GetTargetProject() != null)
                {
                    GetTargetProject().Name = value;
                    OnPropertyChanged("Name");
                    IsChanged = true;
                }
            }
        }
        public string Memo
        {
            get { if (GetTargetProject() == null) return null; return GetTargetProject().Memo; }
            set
            {
                if (GetTargetProject() != null)
                {
                    GetTargetProject().Memo = value;
                    OnPropertyChanged("Memo");
                    IsChanged = true;
                }
            }
        }

        public double FinishPercent
        {
            get
            {
                if (GetTargetProject() != null) return GetTargetProject().FinishPercent;
                return 0;
            }
        }
        public string ProcessInfo
        {
            get
            {
                if (FinishPercent < CommonLib.CommonProc.EPSILON)
                    return "0%";
                var spendtime = (DateTime.Now-GetTargetProject().ProjectStartTime);
                var remiantime = TimeSpan.FromSeconds(spendtime.TotalSeconds / FinishPercent) - spendtime;
                return FinishPercent.ToString("p") + ",spend:" + spendtime.ToString().Substring(0,8) + ",remain:" + remiantime.ToString().Substring(0, 8);
            }
        }
        bool isRunning = false;
        public bool CanStart
        {
            get { return !isRunning; }
        }
        public bool CanPause
        {
            get { if (isRunning&&GetTargetProject().Status == ProjectStatus.Running) return true; return false; }
        }
        public bool CanStop
        {
            get { if (GetTargetProject().Status != ProjectStatus.Stopped) return true; return false; }
        }
        protected void CheckRunningStatus()
        {
            if (GetTargetProject().Status != ProjectStatus.Running)
            {
                isRunning = false;
                StopObserveViewModel();
            }
        }
        public CommonCommand SaveCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    SaveInfo();
                });
            }
        }

        public CommonCommand LoadCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    LoadInfo();
                });
            }
        }
        public void OpenInstrument(IInstrument instrument)
        {
            if (instrument != null)
            {
                var c = new InstrumentControl() { DataContext = new InstrumentViewModel() { TargetObject = instrument.Clone() as IInstrument, GetCurrentDataSource = () => { return CurrentDataSource; } } };
                if (OpenView != null)
                    OpenView(instrument.Name, c,false);
            }
        }

        public CommonCommand AddInstrumentCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    var l = AllInstrumentWindowViewModel.GetInstrumentList();
                    if (l != null && l.Count > 0)
                    {
                        l.ForEach(v =>
                        {
                            if (!InstrumentList.Any(i => i.Ticker == v.Ticker))
                            {
                                InstrumentList.Add(v);

                            }
                            if (!GetTargetProject().InstrumentList.Any(i => i.Ticker == v.Ticker))
                            {
                                GetTargetProject().InstrumentList.Add(v.Clone() as IInstrument);

                            }
                        });
                    }
                });
            }
        }
        public abstract bool CanRemove(IInstrument instrument);
        public CommonCommand RemoveInstrumentCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (CurrentInstrument == null) return;
                    if (!CanRemove(CurrentInstrument))
                        MessageBox.Show("Please delete instrument reference first!");
                    else
                        InstrumentList.Remove(CurrentInstrument);
                });
            }
        }

        public CommonCommand RefreshCommand
        {
            get
            {
                return new CommonCommand((o) => Refresh());
            }
        }

        public CommonCommand ShowAllInstrumentWindowCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    var w = new AllInstrumentWindow() { DataContext = new AllInstrumentWindowViewModel() };
                    w.Show();
                });
            }
        }

        public CommonCommand StartCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (!GetTargetProject().CanRun())
                    {
                        MessageBox.Show("Project can not run, please supplement information");
                        return;
                    }
                    isRunning = true;
                    OnPropertyChanged("CanStart");

                    TargetSummaryVM.StartObserveViewModel();
                    try {
                        Task.Factory.StartNew(() => GetTargetProject().Start());
                        StartObserveViewModel();
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show("Run error:" + ex.Message);
                        isRunning = false;

                    }
                    
                    OnPropertyChanged("CanPause");
                    OnPropertyChanged("CanStop");

                });
            }
        }
        public CommonCommand StopCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {

                    GetTargetProject().Stop();
                    isRunning = false;
                    StopObserveViewModel();
                    OnPropertyChanged("CanStart");
                    OnPropertyChanged("CanPause");
                    OnPropertyChanged("CanStop");
                    OnPropertyChanged("FinishPercent");
                    TargetSummaryVM.StopObserveViewModel();
                    TargetSummaryVM.Refresh();
                });
            }
        }
        public CommonCommand PauseCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {

                    GetTargetProject().Pause();
                    isRunning = false;
                    StopObserveViewModel();
                    OnPropertyChanged("CanStart");
                    OnPropertyChanged("CanPause");
                    OnPropertyChanged("CanStop");
                    TargetSummaryVM.StopObserveViewModel();
                    TargetSummaryVM.Refresh();
                });
            }
        }

        public CommonCommand SaveToFileCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    SaveToFile();
                });
            }
        }
        public CommonCommand LoadFromFileCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    LoadFromFile();
                });
            }
        }
        public static string instrumentsString;
        public static string conditionsString;
        public CommonCommand CopyInstrumentListCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    instrumentsString = GetTargetProject().GetInstrumentInfo();
                });
            }
        }
        public CommonCommand PasteInstrumentListCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (string.IsNullOrEmpty(instrumentsString))
                        MessageBox.Show("Not copy instruments yet, please copy first");
                    else
                    {
                        var i = GetTargetProject().SetInstrumentInfo(instrumentsString);
                        MessageBox.Show("Copy " + i.ToString() + " instruments complete");
                        InstrumentList.Clear();
                        GetTargetProject().InstrumentList.ForEach(v => InstrumentList.Add(v));
                    }
                });
            }
        }
        public CommonCommand CopyConditionListCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    conditionsString = GetTargetProject().GetConditionInfo();
                });
            }
        }
        public CommonCommand PasteConditionListCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (string.IsNullOrEmpty(conditionsString))
                        MessageBox.Show("Not copy conditions yet, please copy first");
                    else
                    {
                        var i = GetTargetProject().SetConditionInfo(conditionsString);
                        MessageBox.Show("Copy " + i.ToString() + " conditions complete");
                        ConditionList.Clear();
                        GetTargetProject().ConditionList.ForEach(v => ConditionList.Add(new ConditionViewModel() { TargetObject = v }));
                    }
                });
            }
        }
    }
}
