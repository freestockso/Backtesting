using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingCommonLib;

using BackTestingInterface;

using CommonLib;
using CommonLibForWPF;

namespace WeightControlStrategy
{
    public class WeightControlStrategy : StrategyBase, IEditControlSupport
    {
        public System.Windows.FrameworkElement GetEditControl()
        {
            var mv = new EditControlViewModel() { SaveValue=SaveValue,LoadValue=LoadValue};
            mv.Init();
            mv.InitPortfolio(TargetPortfolioList);
            return new EditControl() { DataContext = mv };
        }
        void SaveValue(PosWeightTarget target)
        {
            //if (list == null||TargetPortfolio==null) return;
            //TargetList.Clear();
            //list.ForEach(v =>
            //{
            //    TargetList.Add(v);
                
            //});
            Target = target;
            //SaveInfoToParameterList();
        }
        PosWeightTarget LoadValue()
        {
            //LoadInfoFromParameterList();
            return Target;
        }


        public PosWeightTarget Target { get; set; }//only adjust one ticker
        public IPortfolio CurrentPortfolio { get; set; }

        //public double TargetWeight
        //{
        //    get
        //    {
        //        var p = Parameter.GetParameter(ParameterList, "TargetWeight");
        //        if (p == null)
        //            return 0.5;
        //        return Convert.ToDouble(p.Value);
        //    }
        //}
        //public double Threshold
        //{
        //    get
        //    {
        //        var p = Parameter.GetParameter(ParameterList, "Threshold");
        //        if (p == null)
        //            return 0.1;
        //        return Convert.ToDouble(p.Value);
        //    }
        //}
        //public double Max
        //{
        //    get
        //    {
        //        var p = Parameter.GetParameter(ParameterList, "Max");
        //        if (p == null)
        //            return 0.99;
        //        return Convert.ToDouble(p.Value);
        //    }
        //}
        //public double Min
        //{
        //    get
        //    {
        //        var p = Parameter.GetParameter(ParameterList, "Min");
        //        if (p == null)
        //            return 0.01;
        //        return Convert.ToDouble(p.Value);
        //    }
        //}
        //double _CurrentWeight = 0;
        //public double CurrentWeight
        //{
        //    get
        //    {
        //        return _CurrentWeight;
        //    }
        //    set
        //    {
        //        _CurrentWeight = value;
        //    }
        //}
        Money GetTotleValue()
        {
            //if (TargetList.Count == -0) return new Money();
            //var m = new Money() { Number = 0, FxCode = TargetList.FirstOrDefault().CurrentCurrency };
            //foreach (var v in TargetList)
            //{
            //    m += v.CurrentValue;
            //}
            //return m;
            return GetPositionValue();
        }
        //double RefreshWeight(IMarketData data)
        //{
        //    if (Target == null) return -1;

        //    var p= CurrentPortfolio.PositionList.FirstOrDefault(v => v.InstrumentTicker == data.InstrumentTicker);
        //    if (data.Time > p.DataTime)
        //    {
        //        p.DataTime = data.Time;
        //        p.CurrentPrice = data.Close.Value;
        //        if (p.InstrumentTicker == Target.TargetTicker && p.DataTime >= Target.DataTime)
        //        {
        //            Target.DataTime = p.DataTime;
        //            Target.CurrentPrice = data.Close.Value;
        //        }
        //    }
        //    return Target.CurrentWeight = Target.CurrentValue / GetTotleValue();
        //}
        void RefreshTarget(IMarketData marketData)
        {
            if (marketData.InstrumentTicker == Target.TargetTicker && marketData.Time >= Target.DataTime)
            {
                Target.CurrentPrice = marketData.Close;
                Target.DataTime = marketData.Time;
            }

            
        }
        Money GetPositionValue()
        {
            var m=new Money();
            if (CurrentPortfolio == null) return m;
            CurrentPortfolio.PositionList.ForEach(v =>
            {
                m += v.CurrentValue;
            });
            return m;
        }
        void RefreshTarget()//only invoke after order
        {
            var p = CurrentPortfolio.PositionList.FirstOrDefault(v => v.InstrumentTicker == Target.TargetTicker);
            if (p == null) return;
            Target.Shares = p.Shares;
            Target.CurrentWeight = p.CurrentValue / GetPositionValue();

            LogInfo("Current Shares:" + p.Shares.ToString());
            LogInfo("Current Price:" + p.CurrentPrice.ToString());
            LogInfo("Current Value:" + p.CurrentValue.ToString());
            LogInfo("Total Value:" + CurrentPortfolio.CurrentValue.ToString());

            LogInfo("Current Weight:" + Target.CurrentWeight.ToString());
        }

        List<Order> GetOrderList(IPortfolio portfolio)
        {
            var ol = new List<Order>();
            if (CurrentPortfolio == null) CurrentPortfolio = portfolio;
            if (portfolio.LastMarketDataTime <= CurrentPortfolio.LastMarketDataTime) return ol;
            RefreshPortfolio(portfolio);
            RefreshTarget();
            if (Target.CurrentPrice == 0)
                return ol;


            LogInfo("Target Info:" + CommonProc.ConvertObjectToString(Target));
            LogInfo("Portfolio Info:" + CommonProc.ConvertObjectToString(portfolio));
            if (Math.Abs(Target.AbsDistance) > Target.TargetThreshold)
            {
                var dn = (GetTotleValue().Number * Target.TargetWeight)/Target.CurrentPrice-Target.Shares;
                //var dn = (GetTotleValue() * (v.CurrentWeight + v.TargetThreshold) - v.CurrentValue) / (1 - v.CurrentWeight - v.TargetThreshold) / v.CurrentPrice;
                int n = Convert.ToInt32(dn);
                if (n != 0)
                    ol.Add(new Order() {
                        OrderTime = CurrentPortfolio.LastMarketDataTime, 
                        Currency = Target.CurrentCurrency, 
                        TargetPortfolioName = Target.TargetPortfolioName, 
                        Instrument =new Instrument() { Ticker = Target.TargetTicker } ,
                        Shares = n, 
                        OrderDirection = OrderType.Buy, 
                        Price = Target.CurrentPrice, 
                        Mode = OrderMode.OrderLong 
                    });
            }

            return ol;
        }
        public override string Name
        {
            get { return "Weight Control Strategy"; }
            set { }
        }

        public override string Memo
        {
            get { return "Control position weight"; }
            set { }
        }

        public override void ProcessMarketData(List<IMarketData> data)
        {
            //only work when portfolio status changed
            //RefreshWeight(data);
            data.ForEach(v => RefreshTarget(v));

        }
        public void RefreshPortfolio(IPortfolio portfolio)
        {
            
            CurrentPortfolio = portfolio;

        }
        //public override void ProcessPortfolioChange(PortfolioInfo portfolio)
        //{
        //    //if (TargetPortfolio == null) return;
        //    //TargetPortfolio.UpdateInfo(data);
        //    //if (protforlio.ObjectID != TargetPortfolio.ObjectID) return;
        //    //RefreshWeight();
        //    //GoalList.ForEach(v =>
        //    //{
        //    //    if (v.IsOverMaxThreshold)
        //    //    {
        //    //        Order(TargetPortfolio.ObjectID, v.InstrumentName, 1, OrderMode.OrderLong, OrderType.Sell);
        //    //    }
        //    //    if (v.IsLessMinThreshold)
        //    //    {
        //    //        Order(TargetPortfolio.ObjectID, v.InstrumentName, 1, OrderMode.OrderLong, OrderType.Buy);
        //    //    }
        //    //});
        //    if (!InterestedPortfolioList.Any(v=>v.ObjectID==portfolio.ObjectID))
        //        return;
        //    RefreshPortfolio(portfolio);
        //    var ol = GetOrderList(portfolio.LastDataTime);
        //    ol.ForEach(v => Order(v));
        //    //var w = GetWeight();
        //    //if (w > CurrentWeight + Threshold)
        //    //{
        //    //    Order(TargetPortfolio.ObjectID, TargetInstrument, 1, OrderMode.OrderLong, OrderType.Sell);

        //    //}
        //    //if (w < CurrentWeight - Threshold)
        //    //{
        //    //    Order(TargetPortfolio.ObjectID, TargetInstrument, 1, OrderMode.OrderLong, OrderType.Buy);
        //    //}
        //}

        public override ICopyObject CreateInstance()
        {
            return new WeightControlStrategy();
        }

        public override void ProcessPortfolioMarketChanged(IPortfolio portfolio)
        {
            if (!InterestedPortfolioList.Any(v => v.Name == portfolio.Name))
                return;
            
            var ol = GetOrderList(portfolio);
            ol.ForEach(v => Order(v));
        }



        public override void ProcessIndicatorData(List<IIndicatorValue> data)
        {
        }

        public override void ProcessPortfolioOrderChanged(IPortfolio portfolio)
        {
        }

        public override void ProcessPortfolioMoneyChanged(IPortfolio portfolio)
        {
        }

        public override void ProcessSensorData(List<ISensorData> data)
        {
        }

        public override IStrategy CreateSimulateObject(SimulateDirection ward)
        {
            var s = GetData() as WeightControlStrategy;
            if (ward == SimulateDirection.Forward)
                s.Target.TargetWeight *= 1.05;
            else
                s.Target.TargetWeight *= 0.95;
            return s;
        }
    }

}
