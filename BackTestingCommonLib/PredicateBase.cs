using BackTestingInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingCommonLib
{
    public abstract class PredicateBase : IPredicate
    {
        public Func<List<IInstrument>> GetInstrumentList
        {
            get;set;
        }
        public Func<IInstrument, DateTime, DateTime, MarketDataGrade, List<IMarketData>> GetMarketData { get; set; }
        public abstract string Key { get; }
        public abstract string Name { get; set; }
        public abstract string Memo { get; set; }

        public abstract IPredicate CreateInstance();
        public virtual void LoadFromParameterList()
        {
            Parameter.LoadInfoFromParameterList(this, ParameterList);
        }

        public virtual void SaveToParameterList()
        {
            ParameterList.Clear();
            ParameterList.AddRange(Parameter.SaveInfoToParameterList(this));
        }

        public abstract bool PredicateInstrumentList(DateTime startTime, DateTime endTime);

        public virtual object Clone()
        {
            var info = CreateInstance();
            SaveToParameterList();
            ParameterList.ForEach(v => info.ParameterList.Add(v.GetData()));
            info.LoadFromParameterList();

            return info;
        }

        IPredicate originalObject;
        public virtual void SaveOriginalStatus()
        {
            originalObject = Clone() as IPredicate;
        }
        public virtual void LoadOriginalStatus()
        {
            if (originalObject == null) return;
            originalObject.SaveToParameterList();
            ParameterList.Clear();
            originalObject.ParameterList.ForEach(v => ParameterList.Add(v.GetData()));
            LoadFromParameterList();
        }

        List<Parameter> _ParameterList = new List<Parameter>();
        public List<Parameter> ParameterList { get { return _ParameterList; } }
        public virtual string GetSerialParameter()
        {
            SaveToParameterList();
            return CommonLib.CommonProc.ConvertObjectToString(ParameterList);
        }

        public virtual void DeserialParameter(string parameterSerialString)
        {
            var l = CommonLib.CommonProc.ConvertStringToObject<List<Parameter>>(parameterSerialString);
            if (l != null)
                l.ForEach(v => ParameterList.Add(v));
            LoadFromParameterList();
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
    }
}
