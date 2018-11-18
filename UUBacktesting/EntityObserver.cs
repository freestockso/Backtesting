using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UUBacktesting.ViewModel;

namespace UUBacktesting
{
    public interface IObersverSupport: INotifiedViewModel
    {
        void Refresh();
        bool NeedRefresh();
    }
    class EntityObserver
    {
        //List<IObersverSupport> _objectList = new List<IObersverSupport>();
        //public List<IObersverSupport> ObersverObjectList { get { return _objectList; } }
        public IObersverSupport ObersverObject { get; set; }
        public EntityObserver()
        {
            
        }
        Task refreshTask;
        int _RefreshFrequency = 1000;
        public int RefreshFrequency
        {
            get { return _RefreshFrequency; }
            set { _RefreshFrequency = value; }
        }
        bool _IsRunning = false;
        public bool IsRunning
        {
            get { return _IsRunning; }
            set
            {

                if (value)
                {
                    if (_IsRunning) return;
                    _IsRunning = true;
                    refreshTask = new Task(() => RefreshOperation());
                    refreshTask.Start();
                    
                }
                else
                    _IsRunning = false;
                
            }
        }

        void RefreshOperation()
        {
            while (_IsRunning)
            {
                Thread.Sleep(RefreshFrequency);
                App.Current.Dispatcher.Invoke(() =>
                {
                    try {
                        ObersverObject.Refresh(); }
                    catch(Exception ex)
                    {
                        LogSupport.Error(ex);
                    }
                    if (!ObersverObject.NeedRefresh())
                        _IsRunning = false;
                    //lock (ObersverObjectList)
                    //{
                    //    ObersverObjectList.ForEach(v => v.Refresh());
                    //    ObersverObjectList.RemoveAll(v => !v.NeedRefresh());
                    //    if (ObersverObjectList.Count == 0)
                    //        _IsRunning = false;
                    //}
                });
            }
        }
    }
}
