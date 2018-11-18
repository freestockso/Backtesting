using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using BackTestingCommonLib;
using Newtonsoft.Json;

namespace BackTestingCommonLib
{
    public class Position : IPosition
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

        List<ITrade> _TradeTrace = new List<ITrade>();
        public List<ITrade> TradeTrace { get { return _TradeTrace; } }
        public void RefreshTradeTrace()
        {
            TradeTrace.Clear();
            TradeTrace.Add(new Trade()
            {
                Ticker = InstrumentTicker,
                Name = InstrumentName,
                Currency = CurrentCurrency,
                Memo = "Default",
                Owner = "User",
                Price = CurrentPrice,
                Shares = Shares,
                Time = DataTime
            }
            );
            //ChangeList.Clear();
            //ChangeList.Add(new PositionModify()
            //{
            //     ModifyShares=Shares, ModifyCash=new Money() {  FxCode=CurrentCurrency, Number=-1*Shares*CurrentPrice}
            //});
        }

        public string InstrumentName
        {
            get; set;
        }

        public string InstrumentTicker
        {
            get; set;
        }
        public DateTime DataTime { get; set; }

        public double MaxPrice { get; set; }
        public double MinPrice { get; set; }
        [JsonIgnore]
        public Money CurrentValue
        {
            get
            {
                var m = new Money() { FxCode = CurrentCurrency };
                m.Number = CurrentPrice * Shares;
                return m;
            }
        }
        double _Cost;
        public double Cost
        {
            get
            {
                return _Cost;
            }
            set { _Cost = value; }
        }
        [JsonIgnore]
        public double Pnl
        {
            get
            {
                return (CurrentPrice-Cost)*Shares;
            }
        }

        private double _Margin = 1;
        public double Margin
        {
            get { return _Margin; }
            set
            {
                _Margin = value;IsChanged = true;
            }
        }

        private double _Shares = 0;
        public double Shares
        {
            get { return _Shares; }
            set
            {
                _Shares = value;IsChanged = true;
            }
        }

        public Money GetPnl()
        {
            return CurrentValue - new Money() { FxCode=CurrentCurrency,Number=(CurrentPrice- Cost)*Shares};
        }

        string _CurrentCurrency = "RMB";
        public string CurrentCurrency
        {
            get { return _CurrentCurrency; }
            set { _CurrentCurrency = value; IsChanged = true; }
        }

        private double _CurrentPrice;
        public double CurrentPrice
        {
            get { return _CurrentPrice; }
            set
            {
                _CurrentPrice = value; IsChanged = true;
            }
        }
        public void ProcessMarketData(List<IMarketData> marketDataList)
        {
            if (marketDataList == null || marketDataList.Count == 0) return;
            var l = marketDataList.Where(v => v.InstrumentTicker == InstrumentTicker);
            if (l==null||l.Count() == 0) return;
            var d = l.OrderBy(v => v.Time).LastOrDefault();
            DataTime = l.Max(v => v.Time);
            CurrentPrice = d.Close;
            MaxPrice = l.Max(v => v.High);
            MinPrice = l.Min(v => v.Low);

        }
        public void ProcessOrder(IOrder order)
        {
            if (order.Ticker != InstrumentTicker)
                return;
            //ChangeList.Add(new PositionModify() { ModifyCash = order.CashFlow, ModifyShares = order.Shares * order.GetOrderDirection() });
            Shares += order.Shares * order.GetOrderDirection();
            TradeTrace.Add(new Trade()
            {
                Ticker = InstrumentTicker,
                Name = InstrumentName,
                Currency = CurrentCurrency,
                Memo = order.Comment,
                Owner = order.Owner,
                Price = order.Price,
                Shares = Shares,
                Time = order.SettleTime
            });
            RefreshCost();
        }
        void RefreshCost()
        {
            Cost = TradeTrace.Sum(v => v.Price * v.Shares)/Shares;
        }
        //public void InitStatus(double price, DateTime createTime)
        //{
        //    Cost = price;
        //    //ChangeList.Add(new PositionModify() { ModifyCash =new Money() { Number = price * Shares, FxCode = CurrentCurrency }, ModifyShares =Shares });
        //    CreateTime = createTime;
        //}
        //List<PositionModify> _ChangeList = new List<PositionModify>();
        //public List<PositionModify> ChangeList
        //{
        //    get { return _ChangeList; }
        //}

        private DateTime _CreateTime = DateTime.Now;
        public DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }

        [JsonIgnore]
        public TimeSpan KeepTime { get { return DataTime - CreateTime; } }

        public object Clone()
        {
            var info = new Position();
            info.InstrumentName = InstrumentName;
            info.InstrumentTicker = InstrumentTicker;
            info.CurrentPrice = CurrentPrice;
            //ChangeList.ForEach(v => info.ChangeList.Add(v.GetData()));
            info.CreateTime = CreateTime;
            info.DataTime = DataTime;
            info.CurrentPrice = CurrentPrice;

            info.Shares = Shares;
            info.Margin = Margin;
            info.MaxPrice = MaxPrice;
            info.MinPrice = MinPrice;
            TradeTrace.ForEach(v => { info.TradeTrace.Add(v.Clone() as ITrade); });
            
            return info;
        }
        [JsonIgnore]
        public bool IsProfit
        {
            get
            {
                if (Shares > 0 && CurrentPrice > Cost) return true;
                return false;
            }
        }
        [JsonIgnore]
        public bool IsLoss
        {
            get
            {
                if (Shares > 0 && CurrentPrice < Cost) return true;
                return false;
            }
        }
    }

}
