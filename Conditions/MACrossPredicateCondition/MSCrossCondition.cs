using BackTestingCommonLib;
using BackTestingInterface;
using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacTec.TA.Library;

namespace MACrossPredicateCondition
{
    [SerialObjectAttribute(Key = "MA Cross Predicate", Name = "MA Cross Predicate")]
    public class MSCrossCondition : ConditionBase
    {
        public override ICondition CreateInstance()
        {
            return new MSCrossCondition();
        }
        int _BaseUnit = 5;
        [ParameterOperation]
        public int BaseUnit { get { return _BaseUnit; } set { _BaseUnit = value; } }

        int _Unit = 3;
        [ParameterOperation]
        public int Unit { get { return _Unit; } set { _Unit = value; } }

        MarketDataGrade _Grade = MarketDataGrade.Day;
        [ParameterOperation]
        public MarketDataGrade Grade
        {
            get { return _Grade; }
            set { _Grade = value; }
        }

        protected override ISignal Calculate(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade? grade)
        {
            if (grade == null)
                grade = Grade;

            return IsCross(instrument,startTime,endTime, grade.Value);
        }

        ISignal IsCross(IInstrument instrument, DateTime start, DateTime end, MarketDataGrade grade)
        {
            var ml = GetMarketData(instrument, start, end, grade);
            if (ml.Count > BaseUnit)
            {
                return IsCross(ml);
            }
            
                return null;


        }
        ISignal IsCross(List<IMarketData> marketDataList)
        {
            var m = marketDataList.FirstOrDefault();

            var Ls = GetMA(marketDataList, Unit);
            var Ll = GetMA(marketDataList, BaseUnit);
            if (Ls.Count > 1 && Ll.Count > 1)
            {
                var prev = (Ls[Ls.Count - 2] - Ll[Ll.Count - 2])/Ll[Ll.Count-2];
                var curr= (Ls[Ls.Count - 1] - Ll[Ll.Count - 1])/Ll[Ll.Count-1];

                if (isSignalBiggerMeansBetter&& curr > prev && prev <= 0 && curr >= 0)
                {
                    return CreateSignal(m.InstrumentTicker, marketDataList.Max(v => v.Time), (curr - prev) , marketDataList.LastOrDefault().Close);

                }
                if (!isSignalBiggerMeansBetter && curr < prev && prev >= 0 && curr <= 0)
                {
                    return CreateSignal(m.InstrumentTicker, marketDataList.Max(v => v.Time), (curr - prev) , marketDataList.LastOrDefault().Close);

                }
            }
            return null;
        }
        List<double> GetMA(List<IMarketData> marketDataList, int n)
        {
            try
            {
                var valueList = new List<double>();
                int begin = 0;
                int rlength = 0;
                int rbegin = 0;
                double[] values = new double[marketDataList.Count];
                var dataList = marketDataList.Select(v => v.Close).ToArray();
                Core.Sma(begin, marketDataList.Count - 1, dataList, n, out rbegin, out rlength, values);
                valueList = values.ToList();
                CommonProc.TrimDoubleList(valueList);
                if (IsAnalyse)
                {
                    if (valueList.Count > 0)
                    {
                        var o = new TimeValueObject() { Name = Name, Value = valueList.LastOrDefault() };
                        o.Time = marketDataList.LastOrDefault().Time;
                        AddReference(n.ToString(), o);
                    }

                }

                return valueList;
            }
            catch (Exception ex)
            {
                LogSupport.Error(ex);
                throw ex;
            }
        }
    }
}
