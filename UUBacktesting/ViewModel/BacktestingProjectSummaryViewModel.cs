using UUBacktesting.View;
using BackTestingCommonLib;
using BackTestingInterface;
using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace UUBacktesting.ViewModel
{
    class BacktestingProjectSummaryViewModel: ProjectSummaryViewModelBase
    {

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
        IBacktestingProject _TargetProject = null;
        public IBacktestingProject TargetProject
        {
            get { return _TargetProject; }
            set
            {
                if (value != null)
                {
                    _TargetProject = value;

                }
            }
        }

        public override IProject GetTargetProject()
        {
            return TargetProject;
        }

        public override void Refresh()
        {
            if (TargetProject.Status != ProjectStatus.Running)
            {
                StopObserveViewModel();
            }
            OnPropertyChanged("CurrentValue");
            
            OnPropertyChanged("Pnl");
            OnPropertyChanged("StandardPnl");
            OnPropertyChanged("Efficiency");
            OnPropertyChanged("MaxLost");
            base.Refresh();
        }

        public double Pnl
        {
            get
            {
                if (TargetProject == null) return 0;
                return TargetProject.Pnl.Number;
            }
        }

        public double StandardPnl
        {
            get
            {
                if (TargetProject == null) return 0;
                return TargetProject.StandardPnl.Number;
            }
        }

        
    }
}
