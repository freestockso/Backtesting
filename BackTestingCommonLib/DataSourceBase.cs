using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using CommonLib;

namespace BackTestingCommonLib
{
    public abstract class DataSourceBase : SerialSupportObject, IDataSource
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

        int _CacheSize = 20;
        [ParameterOperation]
        public int CacheSize
        {
            get { return _CacheSize; }
            set { _CacheSize = value; IsChanged = true; }
        }
        [ParameterOperation]
        public DateTime? CacheStartTime { get; set; }
        [ParameterOperation]
        public DateTime? CacheEndTime { get; set; }
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

        List<MarketDataCacke> _Cache = new List<MarketDataCacke>();
        public List<MarketDataCacke> Cache { get { return _Cache; } }

        List<IMarketData> _DataList=new List<IMarketData>();
        public List<IMarketData> DataList
        {
            get { return _DataList; }
        }

        public virtual List<IMarketData> GetDataList(List<IInstrument> instrumentList, DateTime startTime, DateTime endTime,MarketDataGrade grade)
        {
            var ml = new List<IMarketData>();
            try
            {
                instrumentList.ForEach(v =>
                {
                    ml.AddRange(GetDataList(v, startTime, endTime,grade));
                });
            }
            catch (Exception ex)
            {
                LogSupport.Error(ex);
            }
            return ml;
        }
        public List<IMarketData> GetDataList(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade grade)
        {
            var cl = Cache.FirstOrDefault(v => v.Ticker == instrument.Ticker && v.StartTime <= startTime && v.EndTime >= endTime);
            if (cl != null)
            {
                return MarketData.SummaryMarketDataList(cl.DataList.Where(v=>v.Time>startTime&&v.Time<=endTime).ToList(), grade);
            }
            else
            {
                if (Cache.Count >= CacheSize)
                    Cache.RemoveAt(0);
                var start = startTime;
                var end = endTime;
                if (CacheStartTime != null)
                    start = CacheStartTime.Value;
                if (CacheEndTime != null)
                    end = CacheEndTime.Value;
                var l = GetSourceDataList(instrument, start, end);

                Cache.Add(new MarketDataCacke() { DataList = l, StartTime = start, EndTime = end, Ticker = instrument.Ticker });
                return MarketData.SummaryMarketDataList(l.Where(v => v.Time > startTime && v.Time <= endTime).ToList(), grade);
            }
        }
        public void PrepareCache(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade grade)
        {
            var cl = Cache.FirstOrDefault(v => v.Ticker == instrument.Ticker && v.StartTime <= startTime && v.EndTime >= endTime);
            if (cl != null)
            {
                return;
            }
            else
            {
                if (Cache.Count >= CacheSize)
                    Cache.RemoveAt(0);
                var start = startTime;
                var end = endTime;
                if (CacheStartTime != null)
                    start = CacheStartTime.Value;
                if (CacheEndTime != null)
                    end = CacheEndTime.Value;
                var l = GetSourceDataList(instrument, start, end);

                Cache.Add(new MarketDataCacke() { DataList = l, StartTime = start, EndTime = end, Ticker = instrument.Ticker });

            }
        }
        public abstract List<IMarketData> GetSourceDataList(IInstrument instrument, DateTime startTime, DateTime endTime);
        public virtual IMarketData GetFirstData(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade grade)
        {
            var ml = GetDataList(instrument, startTime, endTime,grade);
            if (ml.Count == 0)
                return null;
            return ml.OrderBy(v => v.Time).FirstOrDefault();
        }

        IDataSource originalObject;
        public virtual void SaveOriginalStatus()
        {
            originalObject = Clone() as IDataSource;
        }
        public virtual void LoadOriginalStatus()
        {
            if (originalObject == null) return;
            originalObject.SaveToParameterList();
            ParameterList.Clear();
            originalObject.ParameterList.ForEach(v => ParameterList.Add(v.GetData()));
            LoadFromParameterList();

            DataList.Clear();
            originalObject.DataList.ForEach(v => DataList.Add(v.Clone() as IMarketData));
        }

        public virtual void PrepareWork() { }

        public virtual void FinishWork() { }

        public abstract IDataSource CreateInstance();

        public override object Clone()
        {
            var obj = CreateInstance();
            obj.CacheEndTime = CacheEndTime;
            obj.CacheStartTime = CacheStartTime;
            obj.CacheSize = CacheSize;
            obj.Name = Name;
            obj.Memo = Memo;
            ParameterList.ForEach(v => { obj.ParameterList.Add(v.GetData()); });
            return obj;
        }
    }
}
