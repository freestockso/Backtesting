using UUBacktesting.View;
using UUBacktesting.ViewModel;
using BackTestingCommonLib;
using BackTestingCommonLib.OrderProcessor;
using BackTestingInterface;
using CommonDataSource;
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
using System.Windows.Media.Imaging;
using System.IO;

namespace UUBacktesting
{
    class MainViewModel : ViewModelBase
    {
        static BacktestingResource _resource = null;
        public static BacktestingResource Resource
        {
            get
            {
                if (_resource == null)
                {
                    _resource = new BacktestingResource();
                    _resource.Initialize();
                }
                return _resource;
            }
        }
        static ObservableCollection<Window> _ProjectWindowList = new ObservableCollection<Window>();
        public static ObservableCollection<Window> ProjectWindowList { get { return _ProjectWindowList; } }

        static ObservableCollection<ProjectSummaryViewModelBase> _ProjectList = new ObservableCollection<ProjectSummaryViewModelBase>();
        public static ObservableCollection<ProjectSummaryViewModelBase> ProjectList { get { return _ProjectList; } }

        public static ProjectSummaryViewModelBase CurrentProject { get; set; }

        public static void SynchroniseWindow(ProjectSummaryViewModelBase p)
        {
            foreach (var w in ProjectWindowList)
            {
                var d = w.DataContext;
                if (d != null && d is ProjectViewModelBase)
                {
                    var vm = d as ProjectViewModelBase;
                    if (vm.TargetSummaryVM == p)
                    {
                        if (p.GetTargetProject().Status== ProjectStatus.Running)
                            vm.StartObserveViewModel();
                        else
                            vm.StopObserveViewModel();
                    }
                }
            }
        }

        bool _OperateAllProject = false;
        public bool OperateAllProject
        {
            get { return _OperateAllProject; }
            set { _OperateAllProject = value; OnPropertyChanged("OperateAllProject"); }
        }
        void Open(ProjectSummaryViewModelBase project)
        {
            if (project == null) return;
            if (project is BacktestingProjectSummaryViewModel)
                OpenBacktestingProject(project as BacktestingProjectSummaryViewModel);
            if (project is AnalyseProjectSummaryViewModel)
                OpenAnalyseProject(project as AnalyseProjectSummaryViewModel);
        }
        void OpenAnalyseProject(AnalyseProjectSummaryViewModel p)
        {
            var cw = ProjectWindowList.FirstOrDefault(v => (v.DataContext as ProjectViewModelBase).TargetSummaryVM == p);
            if (cw != null)
            {
                cw.Activate();
                return;
            }
            var w = new AnalyseProjectWindow();
            var dc = new AnalyseProjectViewModel(p.TargetProject) { TargetSummaryVM = p ,OpenView=w.OpenDocument};
            w.DataContext = dc;
            w.helper.projectViewModel = dc;
            w.Closed += W_Closed;
            ProjectWindowList.Add(w);
            w.Show();
            if (p.TargetProject.Status == ProjectStatus.Running)
                dc.StartObserveViewModel();
        }
        void OpenBacktestingProject(BacktestingProjectSummaryViewModel p)
        {
            var cw = ProjectWindowList.FirstOrDefault(v => (v.DataContext as ProjectViewModelBase).TargetSummaryVM==p);
            if (cw != null)
            {
                cw.Activate();
                return;
            }
            var w = new BacktestingProjectWindow();
            var dc= new BacktestingProjectViewModel(p.TargetProject) { TargetSummaryVM = p, OpenView = w.OpenDocument };
            w.DataContext = dc;
            w.helper.projectViewModel = dc;
            w.Closed += W_Closed;
            ProjectWindowList.Add(w);
            w.Show();
            if (p.TargetProject.Status == ProjectStatus.Running)
                dc.StartObserveViewModel();
        }

        public CommonCommand CreateBacktestingProjectCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    var p = new BackTestingCommonLib.BacktestingProject() { CurrentDataSource=new HistoricalDataSource(), CurrentTradeGate=new CommonOrderProcessor()};
                    var svm = new BacktestingProjectSummaryViewModel() { TargetProject = p, Open = Open, Delete = Delete, Start = Start, Pause = Pause, Stop = Stop };
                    ProjectList.Add(svm);
                    CurrentProject = svm;
                    OpenBacktestingProject(svm);
                });
            }
        }
        public CommonCommand CreateAnalyseProjectCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    var p = new AnalyseProject() { CurrentDataSource = new HistoricalDataSource() };
                    var svm = new AnalyseProjectSummaryViewModel() { TargetProject = p, Open = Open, Delete = Delete, Start = Start, Pause = Pause, Stop = Stop };
                    ProjectList.Add(svm);
                    CurrentProject = svm;
                    OpenAnalyseProject(svm);
                });
            }
        }
        
        void Delete(ProjectSummaryViewModelBase project)
        {
            if (MessageBox.Show("Confirm delete? Press OK to delete selected item.", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                if (project != null && ProjectList.Contains(project))
                {
                    var w = ProjectWindowList.FirstOrDefault(v => (v.DataContext as ProjectViewModelBase).TargetSummaryVM == project);
                    if (w != null)
                    {
                        if (MessageBox.Show("Project still open! Press OK to close project and delete, or press cancel to active project window.", "Confirm", MessageBoxButton.OKCancel) == MessageBoxResult.Cancel)
                        {
                            w.Activate();
                            return;
                        }else
                        {
                            //ProjectWindowList.Remove(w);
                            w.Close();
                        }
                    }
                    ProjectList.Remove(project);
                }
            }
        }
        public CommonCommand CreateSimulateProjectCommand
        {

            get
            {
                return new CommonCommand((o) =>
                {
                    if (CurrentProject == null || CurrentProject.GetTargetProject() == null ) return;
                    var tp=CurrentProject.GetTargetProject().Clone() as IProject;
                    tp.Name = CurrentProject.GetTargetProject().Name + " Clone";
                    if(CurrentProject is BacktestingProjectSummaryViewModel)
                    {
                        var vm = new BacktestingProjectSummaryViewModel() { TargetProject = tp as IBacktestingProject};
                        ProjectList.Add(vm);
                            if (AutoOpenProject)
                            OpenBacktestingProject(vm);
                    }
                    if (CurrentProject is AnalyseProjectSummaryViewModel)
                    {
                        var vm = new AnalyseProjectSummaryViewModel() { TargetProject = tp as IAnalyseProject };
                        ProjectList.Add(vm);
                        if (AutoOpenProject)
                            OpenAnalyseProject(vm);
                    }
                });
            }
        }

        public CommonCommand SaveCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    SaveToFile();
                });
            }
        }
        public CommonCommand LoadCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    LoadFromFile();
                });
            }
        }
        public CommonCommand RunCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    RunProject();
                });
            }
        }
        void createResultFileAndOpen(string fileName,string result)
        {
            var fn = fileName + ".Result";
            if (File.Exists(fn))
                File.Delete(fn);
            File.WriteAllLines(fn, new string[] { result });

            if (File.Exists(fn))
                System.Diagnostics.Process.Start("notepad.exe", fn);
        }
        void RunProject()
        {
            var dlg = new OpenFileDialog() { Filter = "back testing project|*." + BacktestingResource.BacktestingProjectFileExt + "|analyse project|*." + BacktestingResource.AnalyseProjectFileExt + "| (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                if (!dlg.FileName.EndsWith(BacktestingResource.AnalyseProjectFileExt))
                {
                    try
                    {
                        var bp = CommonLib.CommonProc.LoadObjFromFile<BacktestingProject>(dlg.FileName);
                        if (bp != null)
                        {
                            bp.RecoverySerialObject();
                            var svm = new BacktestingProjectSummaryViewModel() { TargetProject = bp, Open = Open, Delete = Delete, Start = Start, Pause = Pause, Stop = Stop };
                            ProjectList.Add(svm);
                            Start(svm,()=>
                            {
                                createResultFileAndOpen(dlg.FileName, bp.GetResult());
                            });

                        }
                        return;
                    }
                    catch (Exception ex)
                    {
                        LogSupport.Error(ex);
                    }
                }
                if (!dlg.FileName.EndsWith(BacktestingResource.BacktestingProjectFileExt))
                {
                    try
                    {
                        var ap = CommonLib.CommonProc.LoadObjFromFile<AnalyseProject>(dlg.FileName);
                        if (ap != null)
                        {
                            ap.RecoverySerialObject();
                            var svm = new AnalyseProjectSummaryViewModel() { TargetProject = ap, Open = Open, Delete = Delete, Start = Start, Pause = Pause, Stop = Stop };
                            ProjectList.Add(svm);
                            Start(svm, () =>
                            {
                                createResultFileAndOpen(dlg.FileName, ap.GetResult());
                            });
                        }
                        return;
                    }
                    catch (Exception ex)
                    {
                        LogSupport.Error(ex);
                    }
                }
            }

        }
        public void SaveToFile()
        {
            if (CurrentProject != null && ProjectList.Contains(CurrentProject))
            {
                
                var dlg = new SaveFileDialog() { Filter= "back testing project|*.btproject|analyse project | *.alyproject |(*.*)|*.*" };
                if (dlg.ShowDialog() == true)
                {
                    CurrentProject.GetTargetProject().SerialObject();
                    CommonLib.CommonProc.SaveObjToFile(CurrentProject.GetTargetProject(), dlg.FileName);
                }
            }
            else
                MessageBox.Show("Please select project");
        }
        public void LoadFromFile()
        {
            var dlg = new OpenFileDialog() { Filter = "back testing project|*."+BacktestingResource.BacktestingProjectFileExt+"|analyse project|*."+BacktestingResource.AnalyseProjectFileExt+"| (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                if (!dlg.FileName.EndsWith(BacktestingResource.AnalyseProjectFileExt))
                {
                    try {
                        var bp = CommonLib.CommonProc.LoadObjFromFile<BacktestingProject>(dlg.FileName);
                        if (bp != null)
                        {
                            bp.RecoverySerialObject();
                            var svm = new BacktestingProjectSummaryViewModel() { TargetProject = bp, Open = Open, Delete = Delete, Start = Start, Pause = Pause, Stop = Stop };
                            ProjectList.Add(svm);
                            OpenBacktestingProject(svm);

                        }
                        return;
                    }
                    catch(Exception ex)
                    {
                        LogSupport.Error(ex);
                    }
                }
                if (!dlg.FileName.EndsWith(BacktestingResource.BacktestingProjectFileExt))
                {
                    try {
                        var ap = CommonLib.CommonProc.LoadObjFromFile<AnalyseProject>(dlg.FileName);
                        if (ap != null)
                        {
                            ap.RecoverySerialObject();
                            var svm = new AnalyseProjectSummaryViewModel() { TargetProject = ap, Open = Open, Delete = Delete, Start = Start, Pause = Pause, Stop = Stop };
                            ProjectList.Add(svm);
                            OpenAnalyseProject(svm);
                        }
                        return;
                    }
                    catch (Exception ex)
                    {
                        LogSupport.Error(ex);
                    }
                }
            }
        }

        public static void W_Closed(object sender, EventArgs e)
        {
            var w = sender as System.Windows.Window;
            if (w == null) return;
            w.Closed -= W_Closed;
            if (ProjectWindowList.Contains(w))
                ProjectWindowList.Remove(w);
        }
        void SaveWorkspace()
        {
            var dlg = new SaveFileDialog() { Filter= "work space|*.workspace|(*.*)|*.*"};
            if (dlg.ShowDialog() == true)
            {
                var sp = new Workspace();
                for (int i=0;i<  ProjectList.Count;i++)
                {
                    ProjectList[i].GetTargetProject().SerialObject();
                    if (ProjectList[i].GetTargetProject() is BacktestingProject)
                        sp.BacktestingList.Add(i, ProjectList[i].GetTargetProject() as BacktestingProject);
                    if (ProjectList[i].GetTargetProject() is AnalyseProject)
                        sp.AnalyseList.Add(i, ProjectList[i].GetTargetProject() as AnalyseProject);
                }
                CommonLib.CommonProc.SaveObjToFile(sp, dlg.FileName);
            }
            
        }
        public CommonCommand SaveWorkspaceCommand
        {
            get
            {
                return new CommonCommand((o) => { SaveWorkspace(); });
            }
        }
        void LoadWorkspace()
        {
            var dlg = new OpenFileDialog() { Filter = "work space|*.workspace|(*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                var sp = CommonLib.CommonProc.LoadObjFromFile<Workspace>(dlg.FileName);
                if (sp != null)
                {
                    var count = sp.AnalyseList.Count + sp.BacktestingList.Count;
                    var array = new ProjectSummaryViewModelBase[count];
                    foreach(var kv in sp.AnalyseList)
                    {
                        kv.Value.RecoverySerialObject();
                        var svm = new AnalyseProjectSummaryViewModel() { TargetProject = kv.Value, Open = Open, Delete = Delete, Start = Start, Pause = Pause, Stop = Stop };
                        array[kv.Key] = svm;
                    }
                    foreach (var kv in sp.BacktestingList)
                    {
                        kv.Value.RecoverySerialObject();
                        var svm = new BacktestingProjectSummaryViewModel() { TargetProject = kv.Value, Open = Open, Delete = Delete, Start = Start, Pause = Pause, Stop = Stop };
                        array[kv.Key] = svm;
                    }
                    ProjectList.Clear();
                    foreach (var p in array)
                        ProjectList.Add(p);

                }
                else
                    MessageBox.Show("Load workspace faild from " + dlg.FileName);
            }
        }
        public CommonCommand LoadWorkspaceCommand
        {
            get
            {
                return new CommonCommand((o) => { LoadWorkspace(); });
            }
        }

        void Start(ProjectSummaryViewModelBase project,Action finishedAction=null)
        {
            if (project == null || project.GetTargetProject() == null) return;
            if (!project.GetTargetProject().CanRun())
            {
                MessageBox.Show("Project can not run, please suppliment information");
                return;
            }
            project.OnPropertyChanged("IsRunning");
            project.GetTargetProject().Status = ProjectStatus.Running;
            Task.Factory.StartNew(() => CurrentProject.GetTargetProject().Start()).ContinueWith((t)=> {
                if (finishedAction != null)
                    finishedAction();
            });
            project.StartObserveViewModel();
            SynchroniseWindow(project);
        }
        void Pause(ProjectSummaryViewModelBase project)
        {
            if (project == null || project.GetTargetProject() == null) return;
            project.OnPropertyChanged("IsRunning");
            project.GetTargetProject().Status = ProjectStatus.Pause;
            project.GetTargetProject().Pause();
            project.StopObserveViewModel();
            project.Refresh();
            SynchroniseWindow(project);
        }
        void Stop(ProjectSummaryViewModelBase project)
        {
            if (project == null || project.GetTargetProject() == null) return;
            project.OnPropertyChanged("IsRunning");
            project.GetTargetProject().Stop();
            project.GetTargetProject().Status = ProjectStatus.Stopped;

            project.StopObserveViewModel();
            project.Refresh();
            SynchroniseWindow(project);
        }

        public CommonCommand StartCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (!OperateAllProject)
                    {
                        Start(CurrentProject);
                    }
                    else
                    {
                        foreach(var p in ProjectList)
                        {
                            if (p.GetTargetProject() != null && p.GetTargetProject().Status != ProjectStatus.Running)
                                Start(p);
                        }
                    }
                });
            }
        }
        public CommonCommand StopCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (!OperateAllProject)
                    {
                        Stop(CurrentProject);
                    }
                    else
                    {
                        foreach (var p in ProjectList)
                        {
                            if (p.GetTargetProject() != null && p.GetTargetProject().Status != ProjectStatus.Stopped)
                                Stop(p);
                        }
                    }
                });
            }
        }
        public CommonCommand PauseCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (!OperateAllProject)
                    {
                        Pause(CurrentProject);
                    }
                    else
                    {
                        foreach (var p in ProjectList)
                        {
                            if (p.GetTargetProject() != null && p.GetTargetProject().Status == ProjectStatus.Running)
                                Pause(p);
                        }
                    }
                });
            }
        }

        int _CreateProjectNum = 1;
        public int CreateProjectNum
        {
            get { return _CreateProjectNum; }
            set { _CreateProjectNum = value; OnPropertyChanged("CreateProjectNum"); }
        }

        bool _AutoOpenProject = false;
        public bool AutoOpenProject
        {
            get { return _AutoOpenProject; }
            set { _AutoOpenProject = value; OnPropertyChanged("AutoOpenProject"); }
        }

        public CommonCommand ShowLogCommand
        {
            get {
                return new CommonCommand((o) =>
                {
                    var w = new Window() { Title="Log view",Icon=new BitmapImage(new Uri("pack://application:,,,/Images/log.png"))};
                    var control = new LogControl();
                    w.Content = control;
                    w.Show();
                });
            }
        }

        public CommonCommand MarketDataManageCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    var md = new MarketDataSupport.MainWindow();
                    md.Show();

                });
            }
        }
        public CommonCommand AboutCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    var w = new AboutWindow();
                    w.Show();
                });
            }
        }
        public CommonCommand DistributeAnalyseCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    var w = new DistributeAnalyseWindow() ;
                    w.Show();
                });
            }
        }
    }
    public class Workspace
    {
        Dictionary<int, BacktestingProject> _BacktestingList = new Dictionary<int, BacktestingProject>();
        public Dictionary<int, BacktestingProject> BacktestingList { get { return _BacktestingList; } }

        Dictionary<int, AnalyseProject> _AnalyseList = new Dictionary<int, AnalyseProject>();
        public Dictionary<int, AnalyseProject> AnalyseList { get { return _AnalyseList; } }
    }
}
