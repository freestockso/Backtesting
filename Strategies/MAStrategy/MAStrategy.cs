using BackTestingCommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using CommonLibForWPF;
using System.Windows;

namespace MAStrategy
{
    [SerialObjectAttribute(Key = "MA Strategy", Name = "MA Strategy")]
    public class MAStrategy:StrategyBase, IEditControlSupport
    {

        public override IStrategy CreateInstance()
        {
            return new MAStrategy();
        }


        int _SlowUnit = 10;
        public int SlowUnit
        {
            get { return _SlowUnit; }
            set { _SlowUnit = value; }
        }

        int _FastUnit = 10;
        public int FastUnit
        {
            get { return _FastUnit; }
            set { _FastUnit = value; }
        }
        public override void SaveOriginalStatus()
        {
            


        }

        public override void LoadOriginalStatus()
        {
            
        }


        public double? LastSlowValue { get; set; }
        public double? LastFastValue { get; set; }
        public DateTime? LastSlowTime { get; set; }
        public DateTime? LastFastTime { get; set; }
        public double CurrentPrice
        {
            get { if (CurrentPortfolio != null) {
                    var i = CurrentPortfolio.PositionList.FirstOrDefault(v => v.InstrumentTicker == CurrentTicker);
                    if (i != null) return i.CurrentPrice;
                }
                return 0;
            }

        }
        public string CurrentTicker { get; set; }
        public bool IsSlowOverFast
        {
            get;set;
        }

        IOrder Judgement()
        {
            var inst = GetInstrumentList().FirstOrDefault(v => v.Ticker == CurrentTicker);
            var price = 0d;
            if (inst != null)
                price = inst.CurrentPrice;

            if (IsSlowOverFast && LastSlowValue < LastFastValue)
            {

                IsSlowOverFast = false;
                return CurrentPortfolio.GenerateOrder (inst.Ticker, price, Order.MinOperationShares,  OrderType.Buy); 
            }

            if (!IsSlowOverFast && LastSlowValue < LastFastValue)
            {
                IsSlowOverFast = true;
                return CurrentPortfolio.GenerateOrder(inst.Ticker, price, Order.MinOperationShares, OrderType.Sell);
                
            }
            return null;
        }
        //public override void ProcessIndicatorData(List<IIndicatorValue> data)
        //{
        //    OrderList.Clear();
        //    data.ForEach(v =>
        //    {
        //        ProcessIndicatorData(v);
                
        //    });
        //    var o= Judgement();
        //    if (o != null)
        //        OrderList.Add(o);
        //}
        //public void ProcessIndicatorData(IIndicatorValue data)
        //{
        //    if (CurrentTicker==data.InstrumentTicker&& data.IndicatorName.StartsWith("MA"))
        //    {
        //        var ns = data.IndicatorName.Substring(2, data.IndicatorName.Length - 2);
        //        int tn = 0;
        //        int.TryParse(ns, out tn);
        //        if (tn == SlowUnit)
        //        {
        //            if (data.Time > LastSlowTime||LastSlowTime==null)
        //            {
        //                LastSlowTime = data.Time;
        //                if(LastSlowTime>LastDataTime)
        //                    LastDataTime = LastSlowTime.Value;
        //                LastSlowValue = data.GetValue(data.IndicatorName);
        //                MADataList.Add(new MAData() { DataTime = data.Time, DataType = "Slow", DataValue = LastSlowValue.Value, IsSlowOverFast = IsSlowOverFast });
        //            }
        //        }
        //        if (tn == FastUnit)
        //        {
        //            if (data.Time > LastFastTime||LastFastTime==null)
        //            {
        //                LastFastTime = data.Time;
        //                if (LastFastTime > LastDataTime)
        //                    LastDataTime = LastFastTime.Value;
        //                LastFastValue = data.GetValue(data.IndicatorName);
        //                MADataList.Add(new MAData() { DataTime = data.Time, DataType = "Fast", DataValue = LastFastValue.Value, IsSlowOverFast = IsSlowOverFast });
        //            }
        //        }
                
        //    }

        //}

        public override void ProcessMarketData(List<IMarketData> data)
        {

        }


        //public override List<IStrategy> CreateSimulateObject(SimulateDirection ward)
        //{
        //    var sl = new List<IStrategy>();
        //    var s = new MAStrategy();

        //    if(ward== SimulateDirection.Backward)
        //    {
        //        s.SlowUnit = SlowUnit;
        //        s.FastUnit = FastUnit - 1;
        //    }
        //    else
        //    {
        //        s.SlowUnit = SlowUnit;
        //        s.FastUnit = FastUnit + 1;
        //    }
        //    sl.Add(s);

        //    s = new MAStrategy();

        //    if (ward == SimulateDirection.Backward)
        //    {
        //        s.SlowUnit = SlowUnit - 1;
        //        s.FastUnit = FastUnit;
        //    }
        //    else
        //    {
        //        s.SlowUnit = SlowUnit + 1;
        //        s.FastUnit = FastUnit;
        //    }
        //    sl.Add(s);

        //    s = new MAStrategy();

        //    if (ward == SimulateDirection.Backward)
        //    {
        //        s.SlowUnit = SlowUnit - 1;
        //        s.FastUnit = FastUnit - 1;
        //    }
        //    else
        //    {
        //        s.SlowUnit = SlowUnit + 1;
        //        s.FastUnit = FastUnit + 1;
        //    }
        //    sl.Add(s);
        //    return sl;
        //}

        public override void Initialize()
        {
            MADataList.Clear();
        }


        public FrameworkElement GetEditControl()
        {
            return new MAControl() { DataContext =new MAStrategyViewModel() { TargetObject = this } };
        }

        List<MAData> _MADataList = new List<MAData>();
        public List<MAData> MADataList { get { return _MADataList; } }

        public override void SaveToParameterList()
        {

            var p = ParameterList.FirstOrDefault(v => v.Name == "FastUnit");
            if (p == null)
            {
                p = new Parameter() { Name = "FastUnit" };
                ParameterList.Add(p);
            }
            p.Value = FastUnit.ToString();

            p = ParameterList.FirstOrDefault(v => v.Name == "SlowUnit");
            if (p == null)
            {
                p = new Parameter() { Name = "SlowUnit" };
                ParameterList.Add(p);
            }
            p.Value = SlowUnit.ToString();

            p = ParameterList.FirstOrDefault(v => v.Name == "CurrentTicker");
            if (p == null)
            {
                p = new Parameter() { Name = "CurrentTicker" };
                ParameterList.Add(p);
            }
            p.Value = CurrentTicker;
        }

        public override void LoadFromParameterList()
        {
            foreach (var v in ParameterList)
            {
                if (v.Name == "FastUnit"&&!string.IsNullOrEmpty(v.Value))
                {
                    FastUnit = Convert.ToInt32(v.Value);

                }
                if (v.Name == "SlowUnit" && !string.IsNullOrEmpty(v.Value))
                {
                    SlowUnit = Convert.ToInt32(v.Value);

                }
                if (v.Name == "CurrentTicker")
                {
                    CurrentTicker = v.Value;
                }
            }
        }

        public override void ProcessCondition(List<ICondition> condition)
        {

        }

        //public override void ProcessPortfolio(IPortfolio portfolio)
        //{

        //}
    }

    public class MAData
    {
        public string DataType { get; set; }
        public double DataValue { get; set; }
        public DateTime DataTime { get; set; }
        public bool? IsSlowOverFast { get; set; }
    }
}
