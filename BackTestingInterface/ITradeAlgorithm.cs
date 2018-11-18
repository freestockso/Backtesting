using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    public interface ITradeAlgorithm: ICloneable, IChangeNotify
    {
        string TargetInstrumentTicker { get; set; }//null means not specific target to operate
        DateTime StartTime { get; set; }
        DateTime EndTime { get; set; }
        bool IsEnable(DateTime current);
        //List<string> SignalNameList { get; }
        string TargetSignalName { get; set; }
        List<IOrder> ProcessSignal(List<ISignal> signalList, IPortfolio targetPortfolio);
        OrderType TargetOrderType { get; set; }

        double Threshold { get; set; }
        //bool IsMoreThan { get; set; }
        double PercentLimit { get; set; }//0 means no limit
        int MaxOrderTimes { get; set; }
        int CurrentOrderTimes { get; set; }

    }

}
