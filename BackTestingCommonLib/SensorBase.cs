using BackTestingInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingCommonLib
{
    public abstract class SensorBase : SerialSupportObject, ISensor
    {
        List<ISensorData> _DataList = new List<ISensorData>();
        public List<ISensorData> DataList
        {
            get
            {
                return _DataList;
            }
        }

        int _DelayMs = 50000;
        [ParameterOperation]
        public int DelayMs
        {
            get
            {
                return _DelayMs;
            }

            set
            {
                _DelayMs = value;
            }
        }

        public bool Fine
        {
            get;set;
        }

        public abstract void FinishWork();

        public abstract void PrepareWork();

        public abstract List<ISensorData> GetSensorDataList(List<IInstrument> instrumentList, DateTime start, DateTime end);

        ISensor originalObject;
        public virtual void SaveOriginalStatus()
        {
            originalObject = Clone() as ISensor;
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
