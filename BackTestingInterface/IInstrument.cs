using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CommonLib;

namespace BackTestingInterface
{
    public interface IInstrument : ICloneable, IDataObject
    {
        string Ticker { get; set; }
        double PE { get; set; }
        double PB { get; set; }
        double MarketValue { get; set; }
        string Industory { get; set; }
        string Region { get; set; }
        string Currency { get; set; }
        double Margin { get; set; }
        double OrderFixedCost { get; set; }
        double OrderPercentCost { get; set; }
        double CurrentPrice { get; set; }
        string PYName { get; }
        
    }
}
