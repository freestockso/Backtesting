using BackTestingInterface;
using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    public interface ICondition :IOriginalSupport, ISerialSupport, IWorkObject, IChangeNotify
    {
        Func<List<IInstrument>> GetInstrumentList { get; set; }
        IDataSource AnalyseDataSource { get; set; }

        void GenerateResult(IInstrument instrument,DateTime startTime,DateTime endTime, MarketDataGrade? grade);
        void AnalyseResult(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade? grade);
        void ClearResult();
        //IAnalyseResult Calculate(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade? grade);//each analyse step have a value for each instrument

        int MaxResultCount { get; set; }
        List<ISignal> GetResult(string ticker=null, DateTime? starttime = null,DateTime ? endtime=null,double? threshold=null);
        //List<IAnalyseResult> ResultList { get; }
        //void RefreshResult();
        //Dictionary<IInstrument, double> CalculateList { get; }

        ICondition CreateInstance();
        bool isSignalBiggerMeansBetter { get; set; }

        //List<TimeValueObject> GetAnalyseResult(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade grade);
        //bool IsAnalyse { get; set; }
        Dictionary<string, List<TimeValueObject>> ReferenceValueList { get; }//contains indicator value
        bool IsReferenceIndependence { get;  }
    }

}
