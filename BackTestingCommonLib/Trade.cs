using BackTestingInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingCommonLib
{
    public class Trade : ITrade
    {
        public string Memo
        {
            get;set;
        }

        public string Name
        {
            get; set;
        }

        public string Owner
        {
            get; set;
        }

        public double Price
        {
            get; set;
        }

        public string Currency { get; set; }

        public double Shares
        {
            get;set;
        }

        public string Ticker
        {
            get; set;
        }

        public DateTime Time
        {
            get; set;
        }

        public object Clone()
        {
            var t = new Trade();
            t.Name = Name;
            t.Memo = Memo;
            t.Owner = Owner;
            t.Price = Price;
            t.Shares = Shares;
            t.Ticker = Ticker;
            t.Time = Time;
            t.Currency = Currency;
            return t;

        }
    }
}
