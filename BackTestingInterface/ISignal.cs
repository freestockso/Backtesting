using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    public interface ISignal
    {
        string Ticker { get; set; }
        string InstrumentName { get; set; }
        double Price { get; set; }
        DateTime Time { get; set; }
        string Name { get; set; }
        double Value { get; set; }
        string Owner { get; set; }
        SignalType ResultType { get; set; }
        bool IsPositive { get; set; }
    }

    public enum SignalType
    {
        Trade,Analyse
    }
}
