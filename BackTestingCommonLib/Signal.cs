using BackTestingInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingCommonLib
{
    public class Signal:ISignal
    {
        public string Ticker { get; set; }
        public string InstrumentName { get; set; }
        public double Price { get; set; }
        public DateTime Time { get; set; }
        public double Value { get; set; }
        public string Owner { get; set; }

        SignalType _ResultType = SignalType.Trade;
        public SignalType ResultType { get { return _ResultType; } set { _ResultType = value; } }

        public string Name
        {
            get;set;
        }

        public bool IsPositive
        {
            get; set;
        }
    }
}
