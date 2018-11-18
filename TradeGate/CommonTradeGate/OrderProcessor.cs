using System.Threading;
using BackTestingCommonLib;
using BackTestingInterface;
using CommonLib;
using System;
using System.Collections.Generic;

namespace CommonOrderProcessor
{
    public class CommonTradeGate : TradeGateBase,ITradeGate
    {
        private int _delayMs = 300;
        public override int DelayMs { get { return _delayMs; }set { _delayMs = value; } }

        public override void ProcessorOrder(List<IOrder> order)
        {
            order.ForEach(v => ProcessorOrder(v));
            PublishOrderResult(order);
        }
        public void ProcessorOrder(IOrder order)
        {
            if(DelayMs>0)
                Thread.Sleep(DelayMs);
            order.Status = OrderStatus.Success;
            order.SettleTime = order.OrderTime;

            
        }

        public override void Initialize()
        {
            base.Initialize();
        }


        public override void LoadData(object obj)
        {
            if (obj == null || !(obj is ITradeGate)) return;
            var op = obj as ITradeGate;
            DelayMs = op.DelayMs;
        }

        private string _Name = "Order Processor";
        public override string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }

        public override ICopyObject CreateInstance()
        {
            return new CommonTradeGate();
        }

        public override string Memo
        {
            get { return ""; }
            set { }
        }

    }
}
