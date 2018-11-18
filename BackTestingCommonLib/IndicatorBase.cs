using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using CommonLib;

namespace BackTestingCommonLib
{
    public abstract class IndicatorBase : SerialSupportObject, IIndicator
    {
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

        List<IIndicatorValue> _DataList = new List<IIndicatorValue>();
        public List<IIndicatorValue> DataList { get { return _DataList; } }

        public IInstrument InterestedInstrument
        {
            get;set;
        }

        public abstract IIndicator CreateInstance();


        public abstract void ProcessMarketData(List<IMarketData> data);

        public override object Clone()
        {
            var info = CreateInstance();
            SaveToParameterList();
            ParameterList.ForEach(v => info.ParameterList.Add(v.GetData()));
            info.LoadFromParameterList();

            return info;
        }

        public List<IIndicatorValue> GetIndicatorData(List<IMarketData> data)
        {
            DataList.Clear();
            ProcessMarketData(data);
            return DataList;
        }

        

        IIndicator originalObject;
        public virtual void SaveOriginalStatus()
        {
            originalObject = Clone() as IIndicator;
        }
        public virtual void LoadOriginalStatus()
        {
            if (originalObject == null) return;
            originalObject.SaveToParameterList();
            ParameterList.Clear();
            originalObject.ParameterList.ForEach(v => ParameterList.Add(v.GetData()));
            LoadFromParameterList();
        }

        

    }
}
