using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonLib;

namespace BackTestingInterface
{
    public interface IPosition : ICloneable, IChangeNotify
    {
        List<ITrade> TradeTrace { get; }
        void RefreshTradeTrace();

        double MaxPrice { get; set; }
        double MinPrice { get; set; }
        bool IsProfit { get; }
        bool IsLoss { get; }
        double Cost { get; set; }
        double Pnl { get; }
        string CurrentCurrency { get; set; }

        Money CurrentValue { get; }
        DateTime CreateTime { get; }
        DateTime DataTime { get; set; }
        TimeSpan KeepTime { get; }
        double Shares { get; set; }
        double CurrentPrice { get; set; }
        double Margin { get; set; }

        string InstrumentName { get; set; }
        string InstrumentTicker { get; set; }

        //List<PositionModify> ChangeList { get; }
        //Money CurrentCost { get; }
        Money GetPnl();
        //void InitStatus(double price,DateTime createTime);//init position status, reset cost etc
        void ProcessOrder(IOrder order);
        void ProcessMarketData(List<IMarketData> marketDataList);
    }
    public class PositionModify
    {
        public Money ModifyCash { get; set; }
        public double ModifyShares { get; set; }
        public PositionModify GetData()
        {
            return new PositionModify()
                {
                    ModifyCash=new Money()
                        {
                            FxCode=ModifyCash.FxCode,
                            Number=ModifyCash.Number
                        },
                        ModifyShares=ModifyShares
                };
        }
    }
}
