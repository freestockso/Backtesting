using BackTestingInterface;
using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace UUBacktesting.ViewModel
{
    public abstract class ProjectSummaryViewModelBase : ViewModelBase, IObersverSupport
    {
        EntityObserver observer = new EntityObserver() { RefreshFrequency = 5000 };
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
            OnPropertyChanged("StatusBrush");
            OnPropertyChanged("FinishPercent");
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
                var spendtime = (DateTime.Now - GetTargetProject().ProjectStartTime);
                var remiantime = TimeSpan.FromSeconds(spendtime.TotalSeconds / FinishPercent) - spendtime;
                return FinishPercent.ToString("p") + ",time spend:" + spendtime.ToString() + ", time remain:" + remiantime.ToString();
            }
        }
        public abstract IProject GetTargetProject();

        public string Name
        {
            get { if (GetTargetProject() != null) return GetTargetProject().Name; return ""; }
            set { if (GetTargetProject() != null) { GetTargetProject().Name = value; OnPropertyChanged("Name"); } }
        }

        public string Memo
        {
            get { if (GetTargetProject() != null) return GetTargetProject().Memo; return ""; }
            set { if (GetTargetProject() != null) { GetTargetProject().Memo = value; OnPropertyChanged("Memo"); } }
        }

        SolidColorBrush runningBrush = new SolidColorBrush(Colors.Green);
        SolidColorBrush pauseBrush = new SolidColorBrush(Colors.Yellow);
        SolidColorBrush stopBrush = new SolidColorBrush(Colors.Red);

        public SolidColorBrush StatusBrush
        {
            get
            {
                if (GetTargetProject().Status == ProjectStatus.Pause) return pauseBrush;
                if (GetTargetProject().Status == ProjectStatus.Running) return runningBrush;
                return stopBrush;
            }
        }

        public SolidColorBrush SelectedBrush { get; set; }
        SolidColorBrush selectedBrush = new SolidColorBrush(Colors.LightBlue);
        public void Selected() { SelectedBrush = selectedBrush; OnPropertyChanged("SelectedBrush"); }
        public void DeSelected() { SelectedBrush = null; OnPropertyChanged("SelectedBrush"); }

        public Action<ProjectSummaryViewModelBase> Delete { get; set; }
        public Action<ProjectSummaryViewModelBase> Open { get; set; }
        public Action<ProjectSummaryViewModelBase,Action> Start { get; set; }
        public Action<ProjectSummaryViewModelBase> Pause { get; set; }
        public Action<ProjectSummaryViewModelBase> Stop { get; set; }

        public CommonCommand DeleteCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (Delete != null)
                        Delete(this);
                });
            }
        }

        public CommonCommand OpenCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (Open != null)
                        Open(this);

                });
            }
        }

        public CommonCommand StartCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (Start != null)
                        Start(this,null);

                });
            }
        }

        public CommonCommand PauseCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (Pause != null)
                        Pause(this);

                });
            }
        }

        public CommonCommand StopCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (Stop != null)
                        Stop(this);

                });
            }
        }
    }
}
