using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib;

namespace BackTestingInterface
{
    public interface IPortfolio : IDataObject, IOriginalSupport,ICloneable,IWorkObject, IChangeNotify
    {
        bool IsUnlimited { get; set; }
        DateTime LastMarketDataTime { get;  }
        string CurrentCurrency { get; set; }
        List<IAccount> AccountList { get; }//account contains all money, each currency have one account
        List<IPosition> PositionList { get; }
        Money CurrentValue { get; }
        Money PositionCapital { get; }
        Money CurrentCapital { get; }

        void ProcessMarketData(List<IMarketData> data);
        bool ProcessMoney(Money money);
        void ProcessOrderList(List<IOrder> orderlist);

        Money GetPnl();
        Money OriginalValue { get; }
        IOrder GenerateOrderByPercent(string ticker, double price, double percent, OrderType orderType,string currency=null);// as more as portfolio can support capital percent
        IOrder GenerateOrder(string ticker, double price, int shares, OrderType orderType, string currency=null);//max shares is shares
        IOrder GenerateOrderByCapital(string ticker, double price, double capital, OrderType orderType, string currency = null);//trade target capital
        double GetWeight(string ticker);
        Dictionary<string, double> GetWeightList();

        Func<List<IInstrument>> GetInstrumentList { get; set; }
    }
}
