using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Backtesting7.ViewModel
{
    abstract class EntityViewModelBase: ViewModelBase
    {
            public EntityViewModelBase()
            {
                refreshTask = new Task(() => RefreshOperation());
            }
            Task refreshTask;
            int _RefreshFrequency = 1000;
            public int RefreshFrequency
            {
                get { return _RefreshFrequency; }
                set { _RefreshFrequency = value; OnPropertyChanged("RefreshFrequency"); }
            }
            bool _IsRunning = false;
            public bool IsRunning
            {
                get { return _IsRunning; }
                set
                {
                    _IsRunning = value; if (value) refreshTask.Start();
                }
            }

            void RefreshOperation()
            {
                while (true)
                {
                    Thread.Sleep(RefreshFrequency);
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Refresh();
                    });
                }
            }

            public abstract void Refresh();
            //public abstract void Load();
            //public abstract void Save();

            public object TargetObject { get; set; }

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

    }
}
