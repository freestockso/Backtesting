using BackTestingInterface;
using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingCommonLib
{
    public abstract class RiskControlBase : SerialSupportObject,IRiskControl
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
        double _MaxPositionPercent = double.MaxValue;
        [ParameterOperation]
        public double MaxPositionPercent
        {
            get
            {
                return _MaxPositionPercent;
            }

            set
            {
                _MaxPositionPercent = value;
                IsChanged = true;
            }
        }

        double _MinPositionPercent = 0;
        [ParameterOperation]
        public double MinPositionPercent
        {
            get { return _MinPositionPercent; } set { _MinPositionPercent = value; IsChanged = true; }
        }

        List<IPositionControl> _PositionControlList = new List<IPositionControl>();
        public List<IPositionControl> PositionControlList
        {
            get
            {
                return _PositionControlList;
            }
        }

        public virtual bool PredictOrder(IOrder order, IPortfolio portfolio)
        {
            if (portfolio.IsUnlimited) return true;
            var value = 0d;
            if (portfolio.CurrentValue.Number < CommonProc.EPSILON)
                value = 1;
            else
                value = (portfolio.PositionCapital + order.PositionValueChange).Number / portfolio.CurrentValue.Number;
            if (value > MaxPositionPercent || value < MinPositionPercent)
                return false;

            return true;
        }

        public abstract List<IOrder> AdjustRisk(IPortfolio portfolio);

        public virtual void PrepareWork() { }

        public virtual void FinishWork() { }

        public List<IOrder> GetConsistentOrderList(double capital,IPortfolio portfolio,OrderType orderType)
        {
            var ol = new List<IOrder>();
            var wl = portfolio.GetWeightList();
            if (wl == null || wl.Count == 0) return ol;
            foreach(var kv in wl)
            {
                var p = portfolio.PositionList.FirstOrDefault(v => v.InstrumentTicker == kv.Key);
                var o = portfolio.GenerateOrderByCapital(kv.Key, p.CurrentPrice, capital, orderType);
                if (o != null)
                    ol.Add(o);
            }
            return ol;
        }
        public List<IOrder> AdjustWeight(IPortfolio portfolio)
        {
            var value = portfolio.PositionCapital.Number / portfolio.CurrentValue.Number;
            
            if (value < MinPositionPercent)
            {
                var capital = (MinPositionPercent - value) * portfolio.CurrentValue.Number;
                return GetConsistentOrderList(capital, portfolio, OrderType.Buy);
            }
            if (value > MinPositionPercent)
            {
                var capital = (value-MaxPositionPercent) * portfolio.CurrentValue.Number;
                return GetConsistentOrderList(capital, portfolio, OrderType.Sell);
            }
            return null;
        }

        IRiskControl original;
        public virtual void SaveOriginalStatus()
        {
            original = Clone() as IRiskControl;
        }

        public virtual void LoadOriginalStatus()
        {
            if (original == null) return;
            original.SaveToParameterList();
            ParameterList.Clear();
            original.ParameterList.ForEach(v => ParameterList.Add(v.GetData()));
            LoadFromParameterList();

        }

        public abstract IRiskControl CreateInstance();

        public override object Clone()
        {
            var obj = CreateInstance();
            obj.MaxPositionPercent = MaxPositionPercent;
            obj.MinPositionPercent = MinPositionPercent;

            obj.Name = Name;
            obj.Memo = Memo;
            ParameterList.ForEach(v => { obj.ParameterList.Add(v.GetData()); });
            PositionControlList.ForEach(v => { obj.PositionControlList.Add(v.Clone() as IPositionControl); });
            return obj;
        }
    }

}
