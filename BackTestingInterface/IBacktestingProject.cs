using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    public interface IBacktestingProject:IProject
    {
        bool IsUnlimited { get; set; }
        ITradeGate CurrentTradeGate { get; set; }

        IPortfolio TargetPortfolio { get; set; }
        IStrategy TestStrategy { get; set; }
        IRiskControl RiskPolicy { get; set; }

        SerialInfo StrategySerial { get; set; }
        SerialInfo TradeGateSerial { get; set; }
        SerialInfo RiskPolicySerial { get; set; }

        Money MaxLost { get; }
        double MaxLostPercent { get; }
        double Efficiency { get; }
        double AverageEfficiency { get; }
        double MaxCapital { get; }

        List<TimeValueObject> GetEfficiencyTrace();
        List<IMarketData> MarketDataList { get; }
        List<IOrder> OrderList { get; }
        List<TimeValueObject> CurrentValueList { get; }
        List<TimeValueObject> StandardValueList { get; }
        
        DateTime TestStartTime { get; set; }
        DateTime TestEndTime { get; set; }
        DateTime TestCurrentTime { get; set; }

        Money CurrentValue { get; }
        Money StandardValue { get; }
        Money Pnl { get; }
        Money StandardPnl { get; }

        bool UseFirstMarketDataInit { get; set; }

        MarketDataGrade AnalyseGrade { get; set; }
        DateTime AnalyseStartTime { get; set; }
        
    }
}
