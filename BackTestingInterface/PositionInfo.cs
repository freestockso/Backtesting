using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    public class PositionInfo:ICloneable
    {
        public string Ticker { get; set; }
        public double BuyPrice { get; set; }
        public int Shares { get; set; }
        public double MaxPrice { get; set; }
        public double MinPrice { get; set; }
        public double CurrentPrice { get; set; }
        public DateTime PriceTime { get; set; }
        public bool IsProfit
        {
            get
            {
                if (Shares > 0 && CurrentPrice > BuyPrice) return true;
                return false;
            }
        }
        public bool IsLoss
        {
            get
            {
                if (Shares > 0 && CurrentPrice < BuyPrice) return true;
                return false;
            }
        }

        public object Clone()
        {
            return new PositionInfo()
            {
                Ticker = Ticker,
                BuyPrice = BuyPrice,
                Shares = Shares,
                MaxPrice = MaxPrice,
                MinPrice = MinPrice,
                CurrentPrice = CurrentPrice,
                PriceTime = PriceTime
            };
        }
    }

}
