using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    public interface IPredicate: ICloneable, IDataObject, IOriginalSupport, ISerialSupport, IParameterSupportObject, IWorkObject
    {
        Func<List<IInstrument>> GetInstrumentList { get; set; }
        Func<IInstrument, DateTime, DateTime, MarketDataGrade, List<IMarketData>> GetMarketData { get; set; }
        bool PredicateInstrumentList(DateTime startTime,DateTime endTime);

        List<Parameter> ParameterList { get; }
        IPredicate CreateInstance();
    }
}
