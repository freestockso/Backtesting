using BackTestingInterface;
using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UUBacktesting.ViewModel
{
    class TradeAlgorithmViewModel: ViewModelBase
    {
        public TradeAlgorithmViewModel(ITradeAlgorithm target)
        {
            TargetObject = target;
        }
        ITradeAlgorithm _TargetObject;
        public ITradeAlgorithm TargetObject
        {
            get { return _TargetObject; }
            set
            {
                if (value != null)
                {
                    _TargetObject = value;
                    Refresh();
                }
                OnPropertyChanged("TargetObject");
            }
        }
        void Refresh()
        {
            //ConditionNameList.Clear();
            //TargetObject.SignalNameList.ForEach(v => ConditionNameList.Add(v));
        }
        public string TargetInstrumentTicker
        {
            get { return TargetObject.TargetInstrumentTicker; }
            set { TargetObject.TargetInstrumentTicker = value;OnPropertyChanged("TargetInstrumentTicker"); }
        }
        public string TargetSignal
        {
            get { return TargetObject.TargetSignalName; }
            set { TargetObject.TargetSignalName = value; OnPropertyChanged("TargetSignal"); }
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
        public OrderType TargetOrderType
        {
            get { return TargetObject.TargetOrderType; }
            set { TargetObject.TargetOrderType = value; OnPropertyChanged("TargetOrderType"); }
        }
        public double Threshold
        {
            get { return TargetObject.Threshold; }
            set { TargetObject.Threshold = value; OnPropertyChanged("Threshold"); }
        }
        public double PercentLimit
        {
            get { return TargetObject.PercentLimit; }
            set { TargetObject.PercentLimit = value; OnPropertyChanged("PercentLimit"); }
        }

        public int MaxOrderTimes
        {
            get { return TargetObject.MaxOrderTimes; }
            set { TargetObject.MaxOrderTimes = value; OnPropertyChanged("MaxOrderTimes"); }
        }

        public int CurrentOrderTimes
        {
            get { return TargetObject.CurrentOrderTimes; }
            set { TargetObject.CurrentOrderTimes = value; OnPropertyChanged("CurrentOrderTimes"); }
        }

        List<OrderType> _OrderTypeList = new List<OrderType>() { OrderType.Buy, OrderType.Sell };
        public List<OrderType> OrderTypeList { get { return _OrderTypeList; } }

        //ObservableCollection<string> _ConditionNameList = new ObservableCollection<string>();
        //public ObservableCollection<string> ConditionNameList
        //{
        //    get { return _ConditionNameList; }
        //}
        //string _CurrentConditionName;
        //public string CurrentConditionName { get { return _CurrentConditionName; } set { _CurrentConditionName = value; OnPropertyChanged("CurrentConditionName"); } }
        //public void AddCondition(ConditionViewModel condition)
        //{
        //    if (!ConditionNameList.Contains(condition.Name))
        //        ConditionNameList.Add(condition.Name);

        //    if (!TargetObject.SignalNameList.Contains(condition.Name))
        //        TargetObject.SignalNameList.Add(condition.Name);
        //}
        //public CommonCommand AddCurrentValidConditionNameCommand
        //{
        //    get
        //    {
        //        return new CommonCommand((o) =>
        //        {
        //            if (!string.IsNullOrEmpty(CurrentConditionName))
        //            {
        //                if (!ConditionNameList.Contains(CurrentConditionName))
        //                    ConditionNameList.Add(CurrentConditionName);

        //                if (!TargetObject.SignalNameList.Contains(CurrentConditionName))
        //                    TargetObject.SignalNameList.Add(CurrentConditionName);
        //            }
        //        });
        //    }
        //}

        //public CommonCommand DeleteCurrentConditionNameCommand
        //{
        //    get
        //    {
        //        return new CommonCommand((o) =>
        //        {
        //            if (!string.IsNullOrEmpty(CurrentConditionName))
        //            {
        //                if (TargetObject.SignalNameList.Contains(CurrentConditionName))
        //                    TargetObject.SignalNameList.Remove(CurrentConditionName);
        //                if (ConditionNameList.Contains(CurrentConditionName))
        //                    ConditionNameList.Remove(CurrentConditionName);

        //            }
        //        });
        //    }
        //}
    }
}
