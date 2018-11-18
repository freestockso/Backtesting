using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    public interface IStandardStrategy:IStrategy
    {
        List<ITradeAlgorithm> TradeAlgorithmList { get; }
        //List<IRiskControl> RiskControlList { get; }
    }
}
