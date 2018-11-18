using BackTestingInterface;
using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingCommonLib.Strategy
{
    [SerialObjectAttribute(Key = "Standard Strategy", Name = "Standard Strategy",Memo ="All actions depend on condition result and portfolio risk control")]
    public class StandardStrategy : StrategyBase
    {
        public override string GetSerialParameter()
        {
            TradeAlgorithmSerialString = CommonProc.ConvertObjectToString(TradeAlgorithmList);
            return base.GetSerialParameter();
        }
        public override void DeserialParameter(string parameterSerialString)
        {
            base.DeserialParameter(parameterSerialString);
            if (!string.IsNullOrEmpty(TradeAlgorithmSerialString))
            {
                var l = CommonProc.ConvertStringToObject<List<ITradeAlgorithm>>(TradeAlgorithmSerialString);
                if (l != null)
                {
                    TradeAlgorithmList.Clear();
                    l.ForEach(v => TradeAlgorithmList.Add(v));
                }
            }
        }

        [ParameterOperation]
        public string TradeAlgorithmSerialString { get; set; }

        //double _StopProfitPercent = 0.15;
        //[ParameterOperation]
        //public double StopProfitPercent
        //{
        //    get { return _StopProfitPercent; }
        //    set { _StopProfitPercent = value; }
        //}

        //double _StopLossPercent = 0.1;
        //[ParameterOperation]
        //public double StopLossPercent
        //{
        //    get { return _StopLossPercent; }
        //    set { _StopLossPercent = value; }
        //}

        List<ITradeAlgorithm> _TradeAlgorithmList = new List<ITradeAlgorithm>();
        public List<ITradeAlgorithm> TradeAlgorithmList
        {
            get
            {
                return _TradeAlgorithmList;
            }
        }

        public override IStrategy CreateInstance()
        {
            return new StandardStrategy();
        }

        //IOrder AdjustRisk(IPosition p)
        //{
        //    if (p == null) return null;

        //    if (p.IsProfit && ((p.MaxPrice - p.CurrentPrice) / p.MaxPrice > StopProfitPercent))
        //    {
        //        var o = CurrentPortfolio.GenerateOrder(p.InstrumentTicker, p.CurrentPrice, (int)p.Shares, OrderType.Sell);
        //        return o;
        //    }
        //    if (p.IsLoss && (p.Cost - p.CurrentPrice) / p.Cost > StopLossPercent)
        //    {
        //        var o = CurrentPortfolio.GenerateOrder(p.InstrumentTicker, p.CurrentPrice, (int)p.Shares, OrderType.Sell);
        //        return o;
        //    }

        //    return null;
        //}
        //public override void ProcessPortfolio()
        //{
        //    base.ProcessPortfolio();
        //    //CurrentPortfolio.PositionList.ForEach(v =>
        //    //{
        //    //    var o = AdjustRisk(v);
        //    //    if (o != null && o.Ticker != null)
        //    //        OrderList.Add(o);
        //    //});
        //}
        public override void ProcessSignal(List<ISignal> signalList)
        {

                TradeAlgorithmList.ForEach(t =>
                {
                    var ol = t.ProcessSignal(signalList, CurrentPortfolio);
                    if (ol != null && ol.Count > 0)
                        OrderList.AddRange(ol);

                });


        }

        public override void PrepareWork()
        {
            base.PrepareWork();
            TradeAlgorithmList.ForEach(v =>
            {
                v.CurrentOrderTimes = 0;
            });
        }

        public override object Clone()
        {
            var o= base.Clone() as StandardStrategy;
            TradeAlgorithmList.ForEach(v => o.TradeAlgorithmList.Add(v.Clone() as ITradeAlgorithm));
            return o;
        }
    }
}
