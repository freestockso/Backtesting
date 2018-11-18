using System.Threading;
using BackTestingCommonLib;
using BackTestingInterface;
using CommonLib;
using System;
using System.Collections.Generic;

namespace BackTestingCommonLib.OrderProcessor
{
    [SerialObjectAttribute(Key = "OrderProcessor", Name = "Default TradeGate", Memo = "Only set all order success immediately")]
    public class CommonOrderProcessor : TradeGateBase
    {
        private int _delayMs = 300;
        public override int DelayMs { get { return _delayMs; }set { _delayMs = value; } }

        public void ProcessorOrder(IOrder order)
        {
            if (order == null) return;
            if(DelayMs>0)
                Thread.Sleep(DelayMs);
            order.Status = OrderStatus.Success;
            order.SettleTime = DateTime.Now;
        }

        public override List<IOrder> ProcessorOrder(List<IOrder> order)
        {
            order.ForEach(v => ProcessorOrder(v));
            return order;
        }

        public override ITradeGate CreateInstance()
        {
            return new CommonOrderProcessor();
        }

    }
}
