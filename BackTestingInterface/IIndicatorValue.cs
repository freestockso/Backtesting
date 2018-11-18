using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    public interface IIndicatorValue:ICloneable
    {
        double? GetValue(string name);
        string IndicatorName { get; set; }
        string InstrumentTicker { get; set; }
        string CreatorName { get; set; }
        void SetValue(string name, double value);
        DateTime Time { get; set; }
        System.Collections.Generic.Dictionary<string, double> ValueList { get; }
        double MainValue { get; }
    }

}
