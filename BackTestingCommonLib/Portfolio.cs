using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using CommonLib;
using BackTestingCommonLib;
using Newtonsoft.Json;

namespace BackTestingCommonLib
{
    public class Portfolio :  IPortfolio
    {
        bool _IsUnlimited = true;
        public bool IsUnlimited { get { return _IsUnlimited; } set { _IsUnlimited = value;AccountList.ForEach(v => v.IsUnlimited = value);PositionList.Clear(); } }
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
        List<IAccount> _AccountList = new List<IAccount>();
        public List<IAccount> AccountList
        {
            get
            {
                return _AccountList;
            }
        }

        List<IPosition> _PositionList = new List<IPosition>();
        public List<IPosition> PositionList
        {
            get { return _PositionList; }
        }

        string _Name = "UnNamed Portfolio by " + DateTime.Now.ToString();
        public string Name
        {
            get { return _Name; }
            set { _Name = value; IsChanged = true; }
        }

        string _Memo = "";
        public string Memo
        {
            get { return _Memo; }
            set { _Memo = value; IsChanged = true; }
        }

        private string _CurrentCurrency = "RMB";
        public string CurrentCurrency
        {
            get { return _CurrentCurrency; }
            set { _CurrentCurrency = value; IsChanged = true; }
        }
        public DateTime LastMarketDataTime
        {
            get; set;
        }

        [JsonIgnore]
        public Func<List<IInstrument>> GetInstrumentList { get; set; }

        public double GetWeight(string ticker)
        {
            var p = PositionList.FirstOrDefault(v => v.InstrumentTicker == ticker);
            if (p == null) return 0;
            return p.CurrentValue.Number / PositionCapital.Number;
        }
        
        [JsonIgnore]
        public Money CurrentValue
        {
            get
            {
                var m = CurrentCapital ;

                foreach (var v in PositionList)
                {
                    m += v.CurrentValue;
                }
                return m;
            }
        }

        public IPosition GetTargetPosition(IInstrument inst)
        {
            if (inst == null) return null;
            return PositionList.FirstOrDefault(v => v.InstrumentTicker == inst.Ticker);
        }

        public Dictionary<IInstrument, double> GetWeight()
        {
            var totle = new Money() { FxCode = CurrentCurrency };
            PositionList.ForEach(v => { totle += v.CurrentValue; });
            var d = new Dictionary<IInstrument, double>();
            PositionList.ForEach(v =>
            {
                d.Add(new Instrument() { Ticker = v.InstrumentTicker, Name = v.InstrumentName }, v.CurrentValue / totle);
            });
            return d;
        }

        public bool ProcessMoney(Money money)
        {
            var m = AccountList.FirstOrDefault(v => v.CurrentCurrency == money.FxCode);
            if (m == null)
            {
                m = new Account() { CurrentCurrency = money.FxCode, Quantity = money.Number };

                AccountList.Add(m);
            }
            else {
                if (!m.IsValidModify(money)) return false;
                m.ObtainMoney(money);
            }
            return true;
        }
        void ProcessOrder(IOrder order)
        {
            try
            {
                if (order == null) return;
                var inst = GetInstrumentList().FirstOrDefault(v => v.Ticker == order.Ticker);
                if (inst == null) return;

                var p = PositionList.FirstOrDefault(v => v.InstrumentTicker == order.Ticker);
                if (p == null)
                {
                    var pos = new Position();
                    pos.InstrumentName = inst.Name;
                    pos.InstrumentTicker = inst.Ticker;
                    pos.CurrentPrice = order.Price;
                    pos.ProcessOrder(order);
                    pos.CreateTime = order.OrderTime;
                    pos.DataTime = order.OrderTime;
                    if(ProcessMoney(order.CashFlow))
                        PositionList.Add(pos);
                }
                else
                {
                    if (ProcessMoney(order.CashFlow))
                        p.ProcessOrder(order);
                }
            }
            catch (Exception ex)
            {
                LogSupport.Error(ex);
            }
        }

        public void ProcessOrderList(List<IOrder> orderlist)
        {
            orderlist.ForEach(v =>
            {
                ProcessOrder(v);
            });
            PositionList.RemoveAll(v => Math.Abs(v.Shares) < CommonProc.EPSILON);
        }

        public Money GetPnl()
        {
            return CurrentValue - OriginalValue;
        }

        [JsonIgnore]
        public Money CurrentCapital
        {
            get
            {
                var m = new Money() { FxCode = CurrentCurrency };
                AccountList.ForEach(v =>
                {
                    m += v.CurrentValue;
                });
                return m;
            }
        }

        [JsonIgnore]
        public Money PositionCapital
        {
            get
            {
                var m = new Money() { FxCode = CurrentCurrency };
                PositionList.ForEach(v => m += v.CurrentValue);
                return m;
            }
        }

        void LoadData(object obj)
        {
            if (obj == null) return;

            var p = obj as IPortfolio;
            if (p == null) return;

            PositionList.Clear();
            p.PositionList.ForEach(v =>
                {
                    PositionList.Add(v.Clone() as IPosition);
                });
            p.AccountList.ForEach(v =>
            {

                AccountList.Add(v.Clone() as Account);
            });
            Name = p.Name;
            Memo = p.Memo;

            LastMarketDataTime = p.LastMarketDataTime;
            CurrentCurrency = p.CurrentCurrency;
        }

        public void ProcessMarketData(List<IMarketData> data)
        {
            PositionList.ForEach(v => v.ProcessMarketData(data));
            if(PositionList.Count>0)
                LastMarketDataTime = PositionList.Max(v => v.DataTime);
        }

        public void SaveOriginalStatus()
        {
            OriginalObject = Clone() as Portfolio;
        }

        public void LoadOriginalStatus()
        {
            
            LoadData(OriginalObject);
        }

        public object Clone()
        {
            var p = new Portfolio();
            p.LoadData(this);
            return p;
        }

        IPortfolio OriginalObject;
        [JsonIgnore]
        public Money OriginalValue
        {
            get
            {
                if (OriginalObject == null)
                    OriginalObject = this;
                return OriginalObject.CurrentValue; 
            }
        }

        List<Log> _LogList = new List<Log>();
        public List<Log> LogList
        {
            get
            {
                return _LogList;
            }
        }

        bool _Fine = true;
        public bool Fine
        {
            get
            {
                return _Fine;
            }

            set
            {
                _Fine = value;
            }
        }

        public Dictionary<string ,double> GetWeightList()
        {
            var wl = new Dictionary<string, double>();
            if (PositionCapital.Number == 0) return wl;
            PositionList.ForEach(v =>
            {
                wl.Add(v.InstrumentTicker, v.CurrentValue.Number / PositionCapital.Number);
            });
            return wl;
        }
        IAccount GetAccount(string currency)
        {

            var account = AccountList.FirstOrDefault(v => v.CurrentCurrency == currency);
            if (account == null)
            {
                if (!IsUnlimited)
                    return null;
                else
                    account = new Account()
                    {
                         CurrentCurrency=currency, IsUnlimited=true, Quantity=0
                    };
            }
            return account;
        }
        public IOrder GenerateOrderByPercent(string ticker, double price, double percent, OrderType orderType, string currency = null)
        {
            if (string.IsNullOrEmpty(currency))
                currency = "RMB";
            Order o = new Order();
            o.Ticker = ticker;
            o.Price = price;
            o.Currency = currency;
            o.OrderDirection = orderType;
            o.OrderTime = DateTime.Now;
            if (orderType == OrderType.Buy)
            {
                var account = GetAccount(currency);
                if (account == null)
                    return null;
                var num = Convert.ToInt32(account.CurrentValue.Number * percent/price) / Order.MinOperationShares;
                num *= Order.MinOperationShares;
                if (num >= Order.MinOperationShares)
                    o.Shares = num;
            }
            else
            {
                if (IsUnlimited)
                {
                    o.Shares = Order.MinOperationShares;
                }
                else
                { 
                    var position = PositionList.FirstOrDefault(v => v.InstrumentTicker == ticker);
                    if (position != null && position.Shares > 0)
                    {
                        var num = Convert.ToInt32(position.Shares * percent / price) / Order.MinOperationShares;
                        num *= Order.MinOperationShares;
                        if (num >= Order.MinOperationShares)
                        {
                            o.Shares = num;
                        }
                    }
                }
            }
            if(o.Shares>0)
                return o;
            return null;
        }
        public IOrder GenerateOrder(string ticker, double price, int shares, OrderType orderType, string currency = null)
        {
            if (shares < CommonProc.EPSILON)
                return null;
            if (string.IsNullOrEmpty(currency))
                currency = "RMB";
            Order o = new Order();
            o.Ticker = ticker;
            o.Price = price;
            o.Currency = currency;
            o.OrderDirection = orderType;
            o.OrderTime = DateTime.Now;
            if (orderType == OrderType.Buy)
            {
                var account = GetAccount(currency);
                if (account == null)//no valid money
                    return null;
                if(account.CurrentValue.Number >= price * shares&& shares>= Order.MinOperationShares)//min buy is 100;
                {
                    o.Shares = shares;
                }
            }
            else
            {
                if (IsUnlimited)
                {
                    o.Shares = shares;
                }
                var position = PositionList.FirstOrDefault(v => v.InstrumentTicker == ticker);
                if (position != null && position.Shares > 0)
                {
                    o.Shares =Math.Min(Convert.ToInt32(position.Shares), shares);
                }
            }
            if (o.Shares > 0)
                return o;
            return null;
        }
        public IOrder GenerateOrderByCapital(string ticker,double price, double capital, OrderType orderType, string currency = null)
        {
            if (price < CommonProc.EPSILON) return null;
            var t = capital / price;
            if (t < CommonProc.EPSILON)
                return null;
            var shares = Convert.ToInt32(t);
            shares= Math.Max(Order.MinOperationShares, shares);
            if (string.IsNullOrEmpty(currency))
                currency = "RMB";
            Order o = new Order();
            o.Ticker = ticker;
            o.Price = price;
            o.Currency = currency;
            o.OrderDirection = orderType;
            o.OrderTime = DateTime.Now;
            if (orderType == OrderType.Buy)
            {
                var account = GetAccount(currency);
                if (account == null)
                    return null;
                if (account.CurrentValue.Number >= price * shares && shares >= Order.MinOperationShares)//min buy is 100;
                {
                    o.Shares = (int)shares;
                }
            }
            else
            {
                if (IsUnlimited)
                {
                    o.Shares = shares;
                }
                else {
                    var position = PositionList.FirstOrDefault(v => v.InstrumentTicker == ticker);
                    if (position != null && position.Shares > 0)
                    {
                        o.Shares = (int)Math.Min(Convert.ToInt32(position.Shares), shares);
                    }
                }
            }
            if (o.Shares > 0)
                return o;
            return null;
        }

        public void PrepareWork()
        {
            PositionList.ForEach(v => v.TradeTrace.Clear());
            
        }

        public void FinishWork()
        {
        }
    }

}
