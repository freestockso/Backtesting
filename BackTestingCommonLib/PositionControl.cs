using BackTestingInterface;
using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingCommonLib
{
    public class PositionControl : IPositionControl
    {
        public bool IsEnable(DateTime current)
        {
            if (current >= StartTime && current <= EndTime) return true;
            return false;
        }

        TimeSpan _MaxPositionTime = TimeSpan.MaxValue;
        [ParameterOperation]
        public TimeSpan MaxPositionTime
        {
            get
            {
                return _MaxPositionTime;
            }

            set
            {
                _MaxPositionTime = value;
            }
        }

        double _StopProfitPercent = 0.15;
        [ParameterOperation]
        public double StopProfitPercent
        {
            get { return _StopProfitPercent; }
            set { _StopProfitPercent = value; }
        }

        double _StopLossPercent = 0.1;
        [ParameterOperation]
        public double StopLossPercent
        {
            get { return _StopLossPercent; }
            set { _StopLossPercent = value; }
        }

        [ParameterOperation]
        public TimeSpan MinPositionTime
        {
            get; set;
        }
        double _MaxWeightPercent = 1;
        [ParameterOperation]
        public double MaxWeightPercent
        {
            get
            {
                return _MaxWeightPercent;
            }

            set
            {
                _MaxWeightPercent = value;
            }
        }

        [ParameterOperation]
        public double MinWeightPercent
        {
            get; set;
        }

        [ParameterOperation]
        public string TargetInstrumentTicker
        {
            get; set;
        }

        DateTime _StartTime = DateTime.MinValue;
        [ParameterOperation]
        public DateTime StartTime
        {
            get
            {
                return _StartTime;
            }

            set
            {
                _StartTime = value;
            }
        }

        DateTime _EndTime = DateTime.MaxValue;
        [ParameterOperation]
        public DateTime EndTime
        {
            get
            {
                return _EndTime;
            }

            set
            {
                _EndTime = value;
            }
        }

        public object Clone()
        {
            var o = new PositionControl();
            o.MaxWeightPercent = MaxWeightPercent;
            o.MaxPositionTime = MaxPositionTime;
            o.MinWeightPercent = MinWeightPercent;
            o.MinPositionTime = MinPositionTime;
            o.TargetInstrumentTicker = TargetInstrumentTicker;
            o.StartTime = StartTime;
            o.EndTime = EndTime;
            o.StopLossPercent = StopLossPercent;
            o.StopProfitPercent = StopProfitPercent;

            return o;
        }

        public bool PredictOrder(IOrder order, IPortfolio portfolio)
        {
            if (string.IsNullOrEmpty(TargetInstrumentTicker))
            {
                return false;
            }
            var p = portfolio.PositionList.FirstOrDefault(v => v.InstrumentTicker == TargetInstrumentTicker);
            Money m;
            if (p == null)
            {
                //p = new Position() { InstrumentTicker = TargetInstrumentTicker, Shares = order.Shares, CurrentPrice = order.Price };
                m = order.PositionValueChange;
            }
            else
            {
                m = p.CurrentValue + order.PositionValueChange;
            }
            var value = m.Number / portfolio.PositionCapital.Number;
            if (value > MaxWeightPercent || value < MinWeightPercent)
                return false;
            else
            {
                if (p == null) return true;
                var leftShares = p.Shares + order.GetOrderDirection() * order.Shares;
                var t = order.OrderTime - p.CreateTime;
                if (leftShares == 0)
                {

                    if (t < MinPositionTime)
                        return false;
                }
                if (t > MaxPositionTime)
                    return false;


            }
            return true;
        }

        public IOrder AdjustRisk(IPortfolio portfolio)
        {
            var p = portfolio.PositionList.FirstOrDefault(v => v.InstrumentTicker == TargetInstrumentTicker);
            if (p == null) return null;
            if (p.DataTime < StartTime || p.DataTime > EndTime) return null;
            if (p.IsProfit && ((p.MaxPrice - p.CurrentPrice) / p.MaxPrice > StopProfitPercent))
            {
                var o = portfolio.GenerateOrder(TargetInstrumentTicker, p.CurrentPrice, (int)p.Shares, OrderType.Sell);
                return o;
            }
            if (p.IsLoss && (p.Cost - p.CurrentPrice) / p.Cost > StopLossPercent)
            {
                var o = portfolio.GenerateOrder(TargetInstrumentTicker, p.CurrentPrice, (int)p.Shares, OrderType.Sell);
                return o;
            }
            if (p.KeepTime>MaxPositionTime)
            {
                var o = portfolio.GenerateOrder(TargetInstrumentTicker, p.CurrentPrice,(int) p.Shares, OrderType.Sell);
                return o;
            }
            var w = portfolio.GetWeight(TargetInstrumentTicker);
            if (w > MaxWeightPercent&& p.KeepTime >= MinPositionTime)
            {
                var capital = (w - MaxWeightPercent) * portfolio.PositionCapital.Number;
                var o = portfolio.GenerateOrderByCapital(TargetInstrumentTicker, p.CurrentPrice, capital, OrderType.Sell);
                return o;

            }
            if (w < MinWeightPercent)
            {
                var capital = (MinWeightPercent- w ) * portfolio.PositionCapital.Number;
                var o = portfolio.GenerateOrderByCapital(TargetInstrumentTicker, p.CurrentPrice, capital, OrderType.Buy);
                return o;

            }

            return null;

        }
    }

}
