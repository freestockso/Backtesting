using BackTestingInterface;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib;
using CommonDataSource;

namespace BackTestingCommonLib
{
    public abstract class ProjectBase : IProject
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
        int _TestStepDelayMS = 500;
        public int TestStepDelayMS
        {
            get { return _TestStepDelayMS; }
            set { _TestStepDelayMS = value; IsChanged = true; }
        }

        List<ICondition> _ConditionList = new List<ICondition>();
        [JsonIgnore]
        public List<ICondition> ConditionList
        {
            get
            {
                return _ConditionList;
            }
        }

        List<SerialInfo> _ConditionSerialList = new List<SerialInfo>();
        public List<SerialInfo> ConditionSerialList
        {
            get
            {
                return _ConditionSerialList;
            }
        }

        IDataSource _CurrentDataSource = new HistoricalDataSource();
        [JsonIgnore]
        public IDataSource CurrentDataSource
        {
            get { return _CurrentDataSource; } set { if (value != null) _CurrentDataSource = value; }
        }

        public abstract double FinishPercent { get; }

        MarketDataGrade _Grade = MarketDataGrade.FiveMinutes;
        public MarketDataGrade Grade
        {
            get
            {
                return _Grade;
            }
            set
            {
                _Grade = value;IsChanged = true;
            }
        }

        List<IInstrument> _InstrumentList = new List<IInstrument>();
        public List<IInstrument> InstrumentList
        {
            get
            {
                return _InstrumentList;
            }
        }

        string _Memo = "";
        public string Memo
        {
            get { return _Memo; } set { _Memo = value; IsChanged = true; }
        }

        public abstract string Name { get; set; }

        DateTime _ProjectStartTime = DateTime.Now;
        public DateTime ProjectStartTime
        {
            get
            {
                return _ProjectStartTime;
            }

            set
            {
                _ProjectStartTime = value; IsChanged = true;
            }
        }

        ProjectStatus _Status = ProjectStatus.Stopped;
        public ProjectStatus Status
        {
            get
            {
                return _Status;
            }

            set
            {
                _Status = value;IsChanged = true;
            }
        }

        public SerialInfo DataSourceSerial
        {
            get;set;
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

        public abstract bool CanRun();

        public abstract object Clone();

        public abstract void LoadOriginalStatus();
        public abstract void SaveOriginalStatus();

        public abstract void RecoverySerialObject();
        public abstract void SerialObject();

        public abstract void Start();
        public abstract void Stop();
        public abstract void Pause();

        public string GetConditionInfo()
        {
            ConditionSerialList.Clear();
            ConditionList.ForEach(v =>
            {
                ConditionSerialList.Add(new SerialInfo() { Key = v.Key, ParameterInfo = v.GetSerialParameter() });
            });
            return CommonLib.CommonProc.ConvertObjectToString(ConditionSerialList);
        }
        public int SetConditionInfo(string s)
        {
            var l = CommonLib.CommonProc.ConvertStringToObject<List<SerialInfo>>(s);
            if (l == null || l.Count == 0) return 0;
            ConditionList.Clear();
            l.ForEach(v =>
            {
                var i = v.CreateInstance(BacktestingResource.CommonResource.ConditionPrototypeList.Cast<ISerialSupport>().ToList());
                if (i != null)
                    ConditionList.Add(i as ICondition);
            });
            return l.Count;
        }
        public string GetInstrumentInfo()
        {
            return CommonLib.CommonProc.ConvertObjectToString(InstrumentList);
        }
        public int SetInstrumentInfo(string s)
        {
            var l = CommonLib.CommonProc.ConvertStringToObject<List<Instrument>>(s);
            if (l == null || l.Count == 0) return 0;
            InstrumentList.Clear();
            l.ForEach(v => InstrumentList.Add(v));
            return l.Count;
        }
        public virtual void PrepareWork() { }

        public virtual void FinishWork() { }

        public abstract string GetResult();
    }
}
