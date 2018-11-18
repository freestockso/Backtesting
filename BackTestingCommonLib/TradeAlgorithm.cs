using BackTestingInterface;
using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingCommonLib
{
    public class TradeAlgorithm : ITradeAlgorithm
    {
        bool _IsChanged = false;
        public bool IsChanged
        {
            get
            {
                return _IsChanged;
            }
            set
            {
                _IsChanged = value;
                _LastModify = DateTime.Now;
            }
        }

        DateTime _LastModify = DateTime.Now;
        public DateTime LastModifyTime
        {
            get
            {
                return _LastModify;
            }
        }
        int _MaxOrderTimes = int.MaxValue;
        public int MaxOrderTimes { get { return _MaxOrderTimes; } set { _MaxOrderTimes = value;IsChanged = true; } }
        public int CurrentOrderTimes { get; set; }

        string _TargetInstrumentTicker;
        public string TargetInstrumentTicker { get { return _TargetInstrumentTicker; } set { _TargetInstrumentTicker = value; IsChanged = true; } }

        string _TargetSignalName;
        public string TargetSignalName { get { return _TargetSignalName; } set { _TargetSignalName = value; IsChanged = true; } }

        DateTime _StartTime = DateTime.MinValue;
        public DateTime StartTime { get { return _StartTime; } set { _StartTime = value;IsChanged = true; } }

        DateTime _EndTime = DateTime.MaxValue;
        public DateTime EndTime { get { return _EndTime; } set { _EndTime = value; IsChanged = true; } }

        public bool IsEnable(DateTime current)
        {
            if (current >= StartTime && current <= EndTime) return true;
            return false;
        }
        //List<string> _SignalNameList = new List<string>();
        //public List<string> SignalNameList { get { return _SignalNameList; } }

        double _PercentLimit = 0;
        public double PercentLimit
        {
            get { return _PercentLimit; } set { _PercentLimit = value; IsChanged = true; }
        }

        OrderType _TargetOrderType = OrderType.Buy;
        public OrderType TargetOrderType
        {
            get { return _TargetOrderType; } set { _TargetOrderType = value; IsChanged = true; }
        }

        //bool _IsMoreThan = true;
        //public bool IsMoreThan { get { return _IsMoreThan; } set { _IsMoreThan = value; IsChanged = true; } }

        double _Threshold = 0;
        public double Threshold
        {
            get { return _Threshold; } set { _Threshold = value; IsChanged = true; }
        }

        public object Clone()
        {
            var o = new TradeAlgorithm();
            o.PercentLimit = PercentLimit;
            o.Threshold = Threshold;
            o.MaxOrderTimes = MaxOrderTimes;
            o.CurrentOrderTimes = CurrentOrderTimes;
            o.StartTime = StartTime;
            o.EndTime = EndTime;
            o.TargetOrderType = TargetOrderType;
            o.TargetInstrumentTicker = TargetInstrumentTicker;
            o.TargetSignalName = TargetSignalName;
            //SignalNameList.ForEach(v => o.SignalNameList.Add(v));
            return o;
        }

        public List<IOrder> ProcessSignal(List<ISignal> signalList,IPortfolio targetPortfolio)
        {
            if (targetPortfolio == null||signalList==null||signalList.Count==0) return null;
            var ol = new List<IOrder>();
            signalList.ForEach(s =>
            {
                if(TargetSignalName==s.Name&&s.ResultType== SignalType.Trade&&
                (string.IsNullOrEmpty(TargetInstrumentTicker)|| TargetInstrumentTicker==s.Ticker))
                {
                    if (Threshold < CommonProc.EPSILON || 
                    (s.IsPositive && s.Value >= Threshold) || 
                    (!s.IsPositive && s.Value <= Threshold))
                    {
                        if (PercentLimit < CommonProc.EPSILON)
                        {
                            var o = targetPortfolio.GenerateOrder(s.Ticker, s.Price, Order.MinOperationShares, TargetOrderType);
                            if (o != null)
                            {
                                o.OrderTime = s.Time;
                                ol.Add(o);
                            }
                        }
                        else if (MaxOrderTimes > 0 && CurrentOrderTimes < MaxOrderTimes)
                        {

                            var p = PercentLimit / (MaxOrderTimes - CurrentOrderTimes);
                            var o = targetPortfolio.GenerateOrderByPercent(s.Ticker, s.Price, p, TargetOrderType);
                            if (o != null)
                            {
                                o.OrderTime = s.Time;
                                ol.Add(o);
                                CurrentOrderTimes++;
                            }
                        }
                    }
                }
            });
            return ol;
        }
    }
}
