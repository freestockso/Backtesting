using CommonLib;
using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UUBacktesting.ViewModel
{
    class LogViewModel:ViewModelBase, IObersverSupport
    {
        EntityObserver observer = new EntityObserver() { RefreshFrequency=10000  };
        public bool NeedRefresh()
        {
            return true;
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

        ObservableCollection<Log> _LogList = new ObservableCollection<Log>();
        public ObservableCollection<Log> LogList { get { return _LogList; } }

        public void Refresh()
        {
            LogList.Clear();
            LogSupport.LogList.ForEach(v => LogList.Add(v));
        }

        public CommonCommand RefreshCommand
        {
            get { return new CommonCommand((o) => Refresh()); }
        }

        public CommonCommand ClearCommand
        {
            get { return new CommonCommand((o) => { LogSupport.LogList.Clear(); Refresh(); }); }
        }
    }
}
