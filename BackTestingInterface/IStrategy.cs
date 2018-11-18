using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommonLib;


namespace BackTestingInterface
{
    public interface IStrategy : IOriginalSupport, ISerialSupport, IWorkObject, IChangeNotify
    {
        DateTime CurrentTime { get; set; }

        void ProcessMarketData(List<IMarketData> data);
        void ProcessSignal(List<ISignal> signalList);
        void ProcessPortfolio();

        List<IOrder> GetOrderList();
        
        IStrategy CreateInstance();

        IPortfolio CurrentPortfolio { get; set; }
        Func<List<IInstrument>> GetInstrumentList { get; set; }

    }
}
