using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using CommonLib;
using System.Windows;
using Newtonsoft.Json;

namespace BackTestingCommonLib
{
    public abstract class StrategyBase : SerialSupportObject, IStrategy
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
        public IPortfolio CurrentPortfolio { get; set; }
        public DateTime LastDataTime { get; set; }

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

        public DateTime CurrentTime { get; set; }
        
        [JsonIgnore]
        public List<IOrder> OrderList = new List<IOrder>();

        public virtual void ProcessMarketData(List<IMarketData> data)
        {
            
        }

        public abstract void ProcessSignal(List<ISignal> signalList);
        public virtual void ProcessPortfolio()
        {

        }

        public abstract IStrategy CreateInstance();

        public List<IOrder> GetOrderList()
        {
            var ol = new List<IOrder>();
            ol.AddRange(OrderList);
            OrderList.Clear();
            return ol;
        }

        public override object Clone()
        {
            var s = CreateInstance();
            SaveToParameterList();
            ParameterList.ForEach(v => s.ParameterList.Add(v.GetData()));
            s.LoadFromParameterList();

            return s;
        }
        IStrategy originalStrategy;
        public virtual void SaveOriginalStatus()
        {
            originalStrategy = Clone() as IStrategy;
        }

        public virtual void LoadOriginalStatus()
        {
            if (originalStrategy == null) return;
            originalStrategy.SaveToParameterList();
            ParameterList.Clear();
            originalStrategy.ParameterList.ForEach(v => ParameterList.Add(v.GetData()));
            LoadFromParameterList();

        }
        public virtual void PrepareWork() { }

        public virtual void FinishWork() { }
        [JsonIgnore]
        public Func<List<IInstrument>> GetInstrumentList { get; set; }

    }
}
