using BackTestingCommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BackTestingInterface;

namespace HighPriceStrategy
{
    [SerialObjectAttribute(Key = "HighBuyStrategy", Name = "High Buy Strategy")]
    public class HighBuyStrategy : StrategyBase
    {
        public override IStrategy CreateInstance()
        {
            return new HighBuyStrategy();
        }
        double percent { get { var c = GetInstrumentList();if (c.Count == 0) return 0; return 1d / c.Count; } }

        public override void ProcessSignal(List<ISignal> signalList)
        {
            var cl = signalList.Where(v => v.Name.EndsWith("RecentHighPriceCondition")).ToList();
            if (cl == null||cl.Count()==0)
                return ;

            cl.ForEach(v =>
            {
                var info = CurrentPortfolio.PositionList.FirstOrDefault(i => i.InstrumentTicker == v.Ticker);

                if (info != null && info.Shares == 0)
                {
                    var o = CurrentPortfolio.GenerateOrderByPercent(v.Ticker, v.Price, percent, OrderType.Buy);
                    if (o != null)
                    {
                        o.OrderTime = v.Time;
                        //info.ProcessOrder(o);
                        OrderList.Add(o);
                    }
                }
            });

        }

    }

}
