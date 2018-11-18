using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using Newtonsoft.Json;

namespace BackTestingCommonLib
{
    public class Order:IOrder
    {
        public string Owner { get; set; }
        public static int MinOperationShares = 100;

        public string Ticker { get; set; }

        public int Shares
        { get; set; }

        public DateTime OrderTime
        { get; set; }

        string _Currency = "RMB";
        public string Currency { get { return _Currency; } set { _Currency = value; } }

        public OrderType OrderDirection
        { get; set; }

        public double Price
        { get; set; }

        double _OrderMargin = 1;
        public double OrderMargin
        { get { return _OrderMargin; } set { _OrderMargin = value; } }

        public Money CashFlow
        {
            get { 
                var m = new Money() {FxCode = Currency};
                m.Number = GetCashDirection()*(OrderMargin*Price*Shares);
                return m;
            }
        }
        public Money PositionValueChange {
            get            
            {
                var m = new Money() { FxCode = Currency };
                m.Number =GetOrderDirection() * Price * Shares ;
                return m;
            }
        }
        public string Comment
        { get; set; }

        public OrderStatus Status
        { get; set; }
        public int GetOrderDirection()
        {
            return -1 * GetCashDirection();
        }
        public int GetCashDirection()
        {
            var dir = -1;
            if (OrderDirection == OrderType.Buy)
                dir *= 1;
            else
                dir *= -1;
            return dir;
        }

        public object Clone()
        {
            var order = new Order()
            {
                Ticker = Ticker,
                    Shares = Shares,
                    OrderTime = OrderTime,
                    OrderDirection = OrderDirection,
                    OrderMargin = OrderMargin,

                    Price = Price,
                    Comment = Comment,
                    Status = Status,
                    Currency = Currency,
                    SettleTime=SettleTime
                };
            return order;
        }

        public DateTime SettleTime
        {
            get;
            set;
        }
    }
}
