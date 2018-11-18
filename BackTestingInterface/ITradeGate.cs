using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace BackTestingInterface
{
    public interface ITradeGate : IOriginalSupport, ISerialSupport, IWorkObject, IChangeNotify
    {

        List<IOrder> ProcessorOrder(List<IOrder> order);

        int DelayMs { get; set; }
        ITradeGate CreateInstance();
    }
}
