using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib;

namespace BackTestingInterface
{
    public interface IDataSource: IOriginalSupport, ISerialSupport, IWorkObject, IChangeNotify
    {
        List<MarketDataCacke> Cache { get; }
        void PrepareCache(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade grade);
        List<IMarketData> GetDataList(List<IInstrument> instrumentList,DateTime startTime,DateTime endTime,MarketDataGrade grade);
        List<IMarketData> GetDataList(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade grade);
        List<IMarketData> GetSourceDataList(IInstrument instrument, DateTime startTime, DateTime endTime);//get data that data time more than start and less than or eque end
        IMarketData GetFirstData(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade grade);
        List<IMarketData> DataList { get; }
        int CacheSize { get; set; }
        DateTime? CacheStartTime { get; set; }
        DateTime? CacheEndTime { get; set; }
        IDataSource CreateInstance();
    }

    public class MarketDataCacke
    {
        public string Ticker { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<IMarketData> DataList { get; set; }
    }
}
