using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackTestingInterface
{
    public interface IOrder : ICloneable
    {
        string Ticker { get; set; }
        int Shares { get; set; }
        DateTime OrderTime { get; set; }
        DateTime SettleTime { get; set; }
        OrderType OrderDirection { get; set; }

        double Price { get; set; }
        double OrderMargin { get; set; }
        Money CashFlow { get; }
        Money PositionValueChange { get; }
        string Comment { get; set; }

        int GetCashDirection();
        int GetOrderDirection();
        string Currency { get; set; }

        OrderStatus Status { get; set; }
        string Owner { get; set; }
    }

    public enum OrderType//message order is defined for error comment
    {
        Buy, Sell, Message
    }

    public enum OrderStatus
    {
        Wait,Commit, Success,Faild
    }
}
