using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using CommonLib;

namespace BackTestingCommonLib
{
    public abstract class TradeGateBase : SerialSupportObject, ITradeGate
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
        int _DelayMs = 0;
        [ParameterOperation]
        public virtual int DelayMs { get { return _DelayMs; } set { _DelayMs = value; IsChanged = true; } }
        int originalDelayMs = 0;

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

        public virtual List<IOrder> ProcessorOrder(List<IOrder> order)
        {
            order.ForEach(v =>
            {
                v.Status = OrderStatus.Success;
            });
            return order;
        }

        public void SaveOriginalStatus()
        {
            originalDelayMs = DelayMs;
        }

        public void LoadOriginalStatus()
        {
            DelayMs = originalDelayMs;
        }

        public virtual void PrepareWork() { }

        public virtual void FinishWork() { }

        public abstract ITradeGate CreateInstance();

        public override object Clone()
        {
            var obj = CreateInstance();
            obj.DelayMs = DelayMs;

            obj.Name = Name;
            obj.Memo = Memo;
            ParameterList.ForEach(v => { obj.ParameterList.Add(v.GetData()); });
            return obj;
        }
    }
}
