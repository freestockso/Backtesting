using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    public interface IMarketData :ICloneable
    {
        string InstrumentTicker { get; set; }

        double Low { get; set; }
        double High { get; set; }
        double Open { get; set; }
        double Close { get; set; }
        double Volume { get; set; }
        double Shares { get; set; }

        DateTime Time { get; set; }

        void Random(Random r);
        string CurrentCurrency{ get; set; }

        void ChangeFxRate(double fxRate, string targetCurrency);
        string ValueShowString { get; }

        MarketDataGrade Grade { get; set; }
    }

    public enum MarketDataGrade
    {
        FiveMinutes,FifteenMinutes,HalfHour,Hour,HalfDay,Day,ThreeDays,Week,HalfMonth,Month,Season,HalfYear,Year
    }
}
