using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    public interface IRiskControl : IOriginalSupport, ISerialSupport, IWorkObject, IChangeNotify
    {
        bool PredictOrder(IOrder order, IPortfolio portfolio);
        List<IOrder> AdjustRisk(IPortfolio portfolio);

        double MaxPositionPercent { get; set; }
        double MinPositionPercent { get; set; }
        List<IPositionControl> PositionControlList { get; }
        IRiskControl CreateInstance();
    }
    public interface IPositionControl :ICloneable
    {
        double StopLossPercent { get; set; }
        double StopProfitPercent { get; set; }
        string TargetInstrumentTicker { get; set; }
        DateTime StartTime { get; set; }
        DateTime EndTime { get; set; }
        bool IsEnable(DateTime current);
        TimeSpan MaxPositionTime { get; set; }
        TimeSpan MinPositionTime { get; set; }
        double MaxWeightPercent { get; set; }
        double MinWeightPercent { get; set; }
        IOrder AdjustRisk(IPortfolio portfolio);
    }

}
