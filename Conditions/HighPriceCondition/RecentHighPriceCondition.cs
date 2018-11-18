using BackTestingCommonLib;
using BackTestingInterface;
using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighPriceCondition
{
    [SerialObjectAttribute(Name = "Recent Price High")]
    public class RecentHighPriceCondition : ConditionBase
    {
        public override ICondition CreateInstance()
        {
            return new RecentHighPriceCondition();
        }
        protected override ISignal Calculate(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade? grade)
        {
            isSignalBiggerMeansBetter = true;
            return IsHigher(instrument, startTime, endTime,grade);
        }

        ISignal IsHigher(IInstrument instrument, DateTime start, DateTime end, MarketDataGrade? grade)
        {
            if (grade == null)
                grade = Grade;
            var ml = AnalyseDataSource.GetDataList(instrument, start, end, grade.Value);
            if (ml.Count <= PredicatePeriod + RecentPeriod)
                return null;
            
            var recentMax = 0d;
            var predicateMax = 0d;
            for(int i = 0; i < PredicatePeriod + RecentPeriod; i++)
            {
                var data = ml[ml.Count - 1 - i];
                if (i < RecentPeriod)
                {
                    recentMax = Math.Max(recentMax, data.Close);
                }
                else
                    predicateMax = Math.Max(predicateMax, data.High);
            }

            if(recentMax >= predicateMax * (Threshold+1))
            {
                var o = new Signal() { Name = Name, Time = end, Value = 0, IsPositive=isSignalBiggerMeansBetter, Price= recentMax, Ticker=instrument.Ticker };
                o.Time = ml.Max(v => v.Time);
                o.Value= (recentMax - predicateMax) / predicateMax;
                return o;
            }
            return null;
        }

        MarketDataGrade _Grade = MarketDataGrade.Day;
        [ParameterOperation]
        public MarketDataGrade Grade
        {
            get { return _Grade; }
            set { _Grade = value; }
        }

        int _PredicatePeriod = 10;
        [ParameterOperation]
        public int PredicatePeriod
        {
            get { return _PredicatePeriod; }
            set { _PredicatePeriod = value; }
        }

        int _RecentPeriod = 3;
        [ParameterOperation]
        public int RecentPeriod
        {
            get { return _RecentPeriod; }
            set { _RecentPeriod = value; }
        }

        double _Threshold = 0.05;
        [ParameterOperation]
        public double Threshold
        {
            get { return _Threshold; }
            set { _Threshold = value; }
        }
    }

}
