using BackTestingInterface;
using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace UUBacktesting.ViewModel
{
    class AnalyseProjectSummaryViewModel:ProjectSummaryViewModelBase
    {
        IAnalyseProject _TargetProject = null;
        public IAnalyseProject TargetProject
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

            OnPropertyChanged("ResultCount");
            base.Refresh();
        }



        public int ResultCount
        {
            get { if (TargetProject != null) return TargetProject.ResultList.Count;return 0; }
        }
        
    }
}
