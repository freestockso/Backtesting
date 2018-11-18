
using UUBacktesting.View;
using BackTestingCommonLib;
using BackTestingInterface;
using CommonLibForWPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace UUBacktesting.ViewModel
{
    class AnalyseProjectViewModel:ProjectViewModelBase
    {
        public AnalyseProjectViewModel(IAnalyseProject project)
        {
            if (project == null)
                throw new Exception("no valid project object");
            TargetProject = project;
            InitContextMenu();
        }
        IAnalyseProject _TargetProject = null;
        public IAnalyseProject TargetProject
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

        public DateTime AnalyseStartTime
        {
            get { if (TargetProject != null) return TargetProject.AnalyseStartTime; return new DateTime(); }
            set { if (TargetProject != null) { TargetProject.AnalyseStartTime = value; OnPropertyChanged("AnalyseStartTime"); IsChanged = true; } }
        }
        public DateTime AnalyseEndTime
        {
            get { if (TargetProject != null) return TargetProject.AnalyseEndTime; return new DateTime(); }
            set { if (TargetProject != null) { TargetProject.AnalyseEndTime = value; OnPropertyChanged("AnalyseEndTime"); IsChanged = true; } }
        }



        ObservableCollection<ConditionViewModel> _PredicateList = new ObservableCollection<ConditionViewModel>();
        public ObservableCollection<ConditionViewModel> PredicateList { get { return _PredicateList; } }
        public ConditionViewModel CurrentPredicate { get; set; }

        ObservableCollection<InstrumentViewModel> _ResultList = new ObservableCollection<InstrumentViewModel>();
        public ObservableCollection<InstrumentViewModel> ResultList { get { return _ResultList; } }
        public InstrumentViewModel CurrentResult { get; set; }

        ObservableCollection<InstrumentViewModel> _BlockList = new ObservableCollection<InstrumentViewModel>();
        public ObservableCollection<InstrumentViewModel> BlockList { get { return _BlockList; } }
        public InstrumentViewModel CurrentBlock { get; set; }

        List<CommonCommand> _PredicateMenuList = new List<CommonCommand>();
        public List<CommonCommand> PredicateMenuList { get { return _PredicateMenuList; } }

        public override void SaveToFile()
        {
            var dlg = new SaveFileDialog() { Filter = "analyse project|*.alyproject|(*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                SaveInfo();
                TargetProject.SerialObject();
                CommonLib.CommonProc.SaveObjToFile(TargetProject, dlg.FileName);
            }
        }
        public override void LoadFromFile()
        {
            var dlg = new OpenFileDialog() { Filter = "analyse project|*.alyproject|(*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                var p = CommonLib.CommonProc.LoadObjFromFile<AnalyseProject>(dlg.FileName);
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
                if (obj.TargetObject != null)
                {
                    TargetProject.ConditionList.Add(obj.TargetObject);
                }
            }
            TargetProject.PredicateList.Clear();
            foreach(var obj in PredicateList)
            {
                if (obj.TargetObject != null)
                {
                    TargetProject.PredicateList.Add(obj.TargetObject);
                }
            }
            TargetProject.InstrumentList.Clear();
            foreach (var obj in InstrumentList)
            {
                TargetProject.InstrumentList.Add(obj);
            }
            TargetProject.ResultList.Clear();
            foreach (var obj in ResultList)
            {
                TargetProject.ResultList.Add(obj.TargetObject);
            }
            TargetProject.BlockList.Clear();
            foreach (var v in BlockList)
                TargetProject.BlockList.Add(v.TargetObject.Clone() as IInstrument);
            IsChanged = false;

            TargetSummaryVM.Refresh();

        }
        public override void LoadInfo()
        {
            if (TargetProject == null) return;
            ConditionList.Clear();
            TargetProject.ConditionList.ForEach(v => ConditionList.Add( new ConditionViewModel() { TargetObject = v }));
            PredicateList.Clear();
            TargetProject.PredicateList.ForEach(v => PredicateList.Add( new ConditionViewModel() { TargetObject = v }));

            InstrumentList.Clear();
            TargetProject.InstrumentList.ForEach(v => InstrumentList.Add(v));
            ResultList.Clear();
            TargetProject.ResultList.ForEach(v => ResultList.Add(new InstrumentViewModel() { TargetObject = v, GetCurrentDataSource = () => { return CurrentDataSource; } }));
            TargetProject.BlockList.ForEach(v => BlockList.Add(new InstrumentViewModel() { TargetObject = v, GetCurrentDataSource = () => { return CurrentDataSource; } }));
            CurrentDataSource = TargetProject.CurrentDataSource;

            IsChanged = false;
            if(TargetSummaryVM!=null)
                TargetSummaryVM.Refresh();
        }


        public override IProject GetTargetProject()
        {
            return TargetProject;
        }

        public override void Refresh()
        {
            CheckRunningStatus();
            ResultList.Clear();
            TargetProject.ResultList.ForEach(v => ResultList.Add(new InstrumentViewModel() { TargetObject = v, GetCurrentDataSource = () => { return CurrentDataSource; } }));
            BlockList.Clear();
            TargetProject.BlockList.ForEach(v => BlockList.Add(new InstrumentViewModel() { TargetObject = v, GetCurrentDataSource = () => { return CurrentDataSource; } }));
            OnPropertyChanged("FinishPercent");
            OnPropertyChanged("IsRunning");
            OnPropertyChanged("ProcessInfo");
            OnPropertyChanged("CanStart");
            OnPropertyChanged("CanPause");
            OnPropertyChanged("CanStop");
            base.Refresh();
        }

        public override void InitContextMenu()
        {
            base.InitContextMenu();

            PredicateMenuList.Clear();
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
                    PredicateList.Add(vm);
                    TargetProject.PredicateList.Add(o);
                });
                command.Name = "New " + v.Name;
                command.Memo = v.Memo;
                command.ForegroundBrush = new SolidColorBrush(Colors.Blue);
                PredicateMenuList.Add(command);
            });

            var pcommand = new CommonCommand((obj) =>
            {
                if (CurrentPredicate != null && PredicateList.Contains(CurrentPredicate))
                    PredicateList.Remove(CurrentPredicate);
            });
            pcommand.Name = "Delete Predicate";
            pcommand.Memo = "Delete Current Predicate";
            pcommand.ForegroundBrush = new SolidColorBrush(Colors.Orange);
            PredicateMenuList.Add(pcommand);
        }

        public CommonCommand ClearBlockCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (MessageBox.Show("Confirm clear? Press OK to clear all items in block list.", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        BlockList.Clear();
                        TargetProject.BlockList.Clear();
                    }
                });
            }
        }
        public CommonCommand ClearCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (MessageBox.Show("Confirm clear? Press OK to clear all items in result list.", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        ResultList.Clear();
                        TargetProject.ResultList.Clear();
                    }
                });
            }
        }

        public CommonCommand AddToBlockCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (CurrentResult != null&& CurrentResult.TargetObject!=null && !BlockList.Contains(CurrentResult))
                    {
                        
                        BlockList.Add(CurrentResult);

                        TargetProject.ResultList.RemoveAll(v => v.Ticker == CurrentResult.TargetObject.Ticker);
                        if (!TargetProject.BlockList.Any(v => v.Ticker == CurrentResult.TargetObject.Ticker))
                            TargetProject.BlockList.Add( CurrentResult.TargetObject);

                        ResultList.Remove(CurrentResult);
                    }

                });
            }
        }

        public CommonCommand DeleteBlockCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (CurrentBlock != null && CurrentBlock.TargetObject != null && BlockList.Contains(CurrentBlock))
                    {
                        TargetProject.BlockList.RemoveAll(v => v.Ticker == CurrentBlock.TargetObject.Ticker);
                        BlockList.Remove(CurrentBlock);
                    }
                });
            }
        }
        public CommonCommand AddBlockCommand
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
                            if (!BlockList.Any(i => i.Ticker == v.Ticker))
                            {
                                BlockList.Add(new InstrumentViewModel() { TargetObject=v.Clone() as IInstrument, GetCurrentDataSource = () => { return CurrentDataSource; } });
                            }
                            if (!TargetProject.BlockList.Any(i => i.Ticker == v.Ticker))
                                TargetProject.BlockList.Add(v.Clone() as IInstrument);
                        });
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
                    if (CurrentInstrument != null)
                    {
                        var c = new InstrumentControl() { DataContext = new InstrumentViewModel() { TargetObject = CurrentInstrument, GetCurrentDataSource = () => { return CurrentDataSource; } } };
                        if (OpenView != null)
                            OpenView(CurrentInstrument.Name, c,false);

                    }

                });
            }
        }
        public void OpenCondition(ConditionViewModel condition)
        {
            if (condition != null&&condition.TargetObject!=null)
            {
                condition.AnalyseStartTime = AnalyseStartTime;
                condition.AnalyseEndTime = AnalyseEndTime;
                condition.AnalyseGrade = Grade;
                //condition.TargetObject.GetMarketData = (instrument, start, end, grade) => { return CurrentDataSource.GetDataList(instrument, start, end, grade); };
                condition.TargetObject.AnalyseDataSource = CurrentDataSource;
                var c = new ConditionControl() { DataContext = condition };
                if (OpenView != null)
                    OpenView("Name", c,true);
            }
        }

        public override bool CanRemove(IInstrument instrument)
        {
            if (ResultList.Any(v => v.Ticker == instrument.Ticker))
                return false;
            if (BlockList.Any(v => v.Ticker == instrument.Ticker))
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
        public bool IsRunning
        {
            get
            {
                if (TargetProject == null) return false;
                if (TargetProject.Status == ProjectStatus.Running) return true;
                return false;
            }
        }



        TelerikGridViewHelper exportHelper = new TelerikGridViewHelper();
        List<string> _ExportFormatList;
        public List<string> ExportFormatList
        {
            get
            {
                if(_ExportFormatList==null)
                {
                    _ExportFormatList = new List<string>();
                    _ExportFormatList.AddRange(exportHelper.ExportFormats);

                }
                return _ExportFormatList;
            }
        }

        string _ExportFormat = "Csv";
        public string ExportFormat { get { return _ExportFormat; } set { _ExportFormat = value; OnPropertyChanged("ExportFormat"); } }
        public CommonCommand ExportCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    exportHelper.SelectedExportFormat = ExportFormat;
                    exportHelper.Export(o);
                });
            }
        }
    }
}
