using BackTestingInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib;

namespace BackTestingCommonLib
{
    public abstract class ConditionBase : SerialSupportObject, ICondition
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
        public Func<List<IInstrument>> GetInstrumentList
        {
            get;set;
        }
        public IDataSource AnalyseDataSource { get; set; }
        public virtual void PrepareWork()
        {
            //CalculateList.Clear();
            _ResultList.Clear();
        }

        int _MaxResultCount = int.MaxValue;
        [ParameterOperation]
        public int MaxResultCount { get { return _MaxResultCount; } set { _MaxResultCount = value;IsChanged = true; } }

        public abstract ICondition CreateInstance();
        protected abstract ISignal Calculate(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade? grade);

        List<ISignal> _ResultList = new List<ISignal>();

        public override object Clone()
        {
            var info = CreateInstance();
            info.MaxResultCount = MaxResultCount;
            //info.IsAnalyse = IsAnalyse;
            info.Name = Name;
            info.Memo = Memo;
            info.isSignalBiggerMeansBetter = isSignalBiggerMeansBetter;
            SaveToParameterList();
            ParameterList.ForEach(v => info.ParameterList.Add(v.GetData()));
            info.LoadFromParameterList();
            return info;
        }

        ICondition originalObject;
        public virtual void SaveOriginalStatus()
        {
            originalObject = Clone() as ICondition;
        }
        public virtual void LoadOriginalStatus()
        {
            if (originalObject == null) return;
            originalObject.SaveToParameterList();
            ParameterList.Clear();
            originalObject.ParameterList.ForEach(v => ParameterList.Add(v.GetData()));
            LoadFromParameterList();
        }
        protected void AddSignal(IInstrument instrument, DateTime time, double value, double price, string name = null, SignalType type = SignalType.Trade)
        {

            var signal = CreateSignal(instrument, time, value, price, name, type);
            _ResultList.RemoveAll(v => v.Ticker == signal.Ticker && v.Time == time);
            _ResultList.Add(signal);
        }
        protected void AddSignal(ISignal signal)
        {
            if (signal == null) return;
            _ResultList.RemoveAll(v => v.Ticker == signal.Ticker && v.Time == signal.Time);

            _ResultList.Add(signal);
        }
        protected void AddSignal(string ticker,DateTime time,double value,double price,string name=null,SignalType type= SignalType.Trade)
        {
            AddSignal(Instrument.AllInstrumentList.FirstOrDefault(v => v.Ticker == ticker), time, value, price, name, type);
        }
        protected ISignal CreateSignal(string ticker, DateTime time, double value, double price, string name = null, SignalType type = SignalType.Trade)
        {
            
            return CreateSignal(Instrument.AllInstrumentList.FirstOrDefault(v => v.Ticker == ticker), time, value, price, name, type);
        }
        protected ISignal CreateSignal(IInstrument instrument, DateTime time, double value, double price, string name = null, SignalType type = SignalType.Trade)
        {
            if (string.IsNullOrEmpty(name))
                name = Name;
            string instTicker = null;
            string instName = null;
            if (instrument != null)
            {
                instTicker = instrument.Ticker;
                instName = instrument.Name;
            }
            var signal = new Signal()
            {
                Ticker = instTicker,
                Time = time,
                Value = value,
                ResultType = type,
                Owner = Name,
                IsPositive = isSignalBiggerMeansBetter,
                Price = price,
                Name = name,
                InstrumentName = instName
            };
            return signal;
        }
        protected void AddReference(string name,TimeValueObject value)
        {
            if (!ReferenceValueList.ContainsKey(name))
                ReferenceValueList.Add(name, new List<TimeValueObject>());
            var l = ReferenceValueList[name];
            var obj = l.FirstOrDefault(v => v.Time == value.Time && v.TargetName == value.TargetName);
            if (obj != null)
                l.Remove(obj);
            l.Add(value);
        }
        public virtual void GenerateResult(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade? grade)
        {
            if (IsAnalyse)
                ReferenceValueList.Clear();
            var d = Calculate(instrument, startTime, endTime,grade);
            
            if (d!=null)
            {
                if (string.IsNullOrEmpty(d.Name))
                    d.Name = Name;
                d.Owner = Name;
                var vl = _ResultList.Where(r => r.Time == d.Time).OrderBy(r=>r.Value).ToList();
                if (vl.Count>0&&vl.Count > MaxResultCount)
                {
                    if(isSignalBiggerMeansBetter)
                    {
                        var v = vl.FirstOrDefault();
                        _ResultList.Remove(v);
                    }
                    else
                    {
                        var v = vl.LastOrDefault();
                        _ResultList.Remove(v);
                    }
                   
                }
                _ResultList.Add(d);
                //AddToCalculate(instrument, d.DoubleValue);
                //RefreshResult();
            }
        }
        public virtual void AnalyseResult(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade? grade)
        {
            ReferenceValueList.Clear();
            IsAnalyse = true;
            var g = MarketDataGrade.FiveMinutes;
            if (grade != null)
                g = grade.Value;
            AnalyseDataSource.PrepareCache(instrument, startTime, endTime, g);
            for (var time= startTime;time<= endTime; time = MarketData.GetNextTime(time, grade.Value))
            {
                ISignal d = null;
                try
                {
                    d = Calculate(instrument, startTime, time, grade);
                }
                catch
                {

                }
                if(d!=null)
                    AddReference(Name, new TimeValueObject() { TargetName = instrument.Ticker, Time = d.Time, Value = d.Value, Name = instrument.Name });

            }
            IsAnalyse = false;
        }
        public void ClearResult()
        {
            _ResultList.Clear();
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
        public virtual bool IsReferenceIndependence { get { return false; } }
        Dictionary<string, List<TimeValueObject>> _ReferenceValueList = new Dictionary<string, List<TimeValueObject>>();
        public Dictionary<string, List<TimeValueObject>> ReferenceValueList { get { return _ReferenceValueList; } }

        public List<IMarketData> GetMarketData(IInstrument instrument, DateTime start, DateTime end, MarketDataGrade grade)
        {
            if (AnalyseDataSource != null)
                return AnalyseDataSource.GetDataList(instrument, start, end, grade);
            return null;
        }

        bool _IsAnalyse = false;
        protected bool IsAnalyse { get { return _IsAnalyse; } set { _IsAnalyse = value; } }
        //public virtual List<TimeValueObject> GetAnalyseResult(IInstrument instrument, DateTime startTime, DateTime endTime,MarketDataGrade grade)
        //{
        //    IsAnalyse = true;
        //    AnalyseValueList.Clear();
        //    if (AnalyseDataSource == null)
        //        throw new Exception ("No valid data source to analyse");
        //    AnalyseDataSource.CacheStartTime =startTime;
        //    AnalyseDataSource.CacheEndTime = endTime;
            
        //    var dl = new List<TimeValueObject>();
        //    for(var d = startTime; d <= endTime; d = MarketData.GetNextTime(d, grade))
        //    {
        //        var v = Calculate(instrument, startTime, d,grade);
        //        var t = dl.FirstOrDefault(dt => dt.Time == v.Time && dt.Name == v.Name && dt.TargetName == v.TargetName);
        //        if (t != null)
        //            dl.Remove(t);
        //        if(v!=null)
        //            dl.Add(v);
        //    }
        //    IsAnalyse = false;
        //    return dl;
        //}

        bool _isSignalBiggerMeansBetter = true;
        [ParameterOperation]
        public bool isSignalBiggerMeansBetter
        {
            get { return _isSignalBiggerMeansBetter; }
            set { _isSignalBiggerMeansBetter = value;IsChanged = true; }
        }

        public List<ISignal> GetResult(string ticker = null, DateTime? starttime = null, DateTime? endtime = null, double? threshold = null)
        {
            if (_ResultList.Count == 0) return _ResultList;
            var l = _ResultList;
            if (!string.IsNullOrEmpty(ticker))
                l = l.Where(v => v.Ticker == ticker).ToList();
            if (endtime == null)
                endtime = _ResultList.Max(v => v.Time);
            if(starttime ==null)
                starttime= _ResultList.Min(v => v.Time);
            l = l.Where(v => v.Time <= endtime&&v.Time >= starttime).ToList();
            if (threshold != null)
            {
                if (isSignalBiggerMeansBetter)
                    l= l.Where(v => v.Value >= threshold).ToList();
                else
                    l = l.Where(v => v.Value <= threshold).ToList();
            }
            return l;
        }

        public virtual void FinishWork()
        {

        }

    }
}
