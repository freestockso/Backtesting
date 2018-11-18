using BackTestingInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingCommonLib
{
    public class IndicatorValue : IIndicatorValue
    {
        public string IndicatorName { get; set; }
        public string InstrumentTicker { get; set; }
        public string CreatorName { get; set; }
        public DateTime Time { get; set; }

        Dictionary<string, double> _ValueList = new Dictionary<string, double>();
        public Dictionary<string, double> ValueList { get { return _ValueList; } }

        public double MainValue
        {
            get
            {
                if (ValueList.Count > 0)
                    return ValueList.FirstOrDefault().Value;
                return 0;
            }
        }

        public double? GetValue(string name)
        {
            if (ValueList.ContainsKey(name))
                return ValueList[name];
            return null;
        }

        public void SetValue(string name, double value)
        {
            if (ValueList.ContainsKey(name))
                ValueList[name] = value;
            else
                ValueList.Add(name, value);
        }

        public object Clone()
        {
            var v = new IndicatorValue();
            v.IndicatorName = IndicatorName;
            v.InstrumentTicker = InstrumentTicker;
            v.Time = Time;
            foreach(var kv in ValueList)
            {
                v.ValueList.Add(kv.Key, kv.Value);
            }
            return v;
        }
    }


}
