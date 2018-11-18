using BackTestingCommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using TicTacTec.TA.Library;
using CommonLib;

namespace MALongArrangeCondition
{
    [SerialObjectAttribute(Key = "MA Long Arrange", Name = "MA Long Arrange")]
    public class MALongArrange : ConditionBase
    {
        public override ICondition CreateInstance()
        {
            return new MALongArrange();
        }
        double _PercentDifferentLimit = 0.35;
        [ParameterOperation]
        public double PercentDifferentLimit { get { return _PercentDifferentLimit; } set { _PercentDifferentLimit = value; } }

        MarketDataGrade _Grade = MarketDataGrade.Day;
        [ParameterOperation]
        public MarketDataGrade Grade
        {
            get { return _Grade; }
            set { _Grade = value; }
        }

        protected  override ISignal Calculate(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade? grade)
        {
            isSignalBiggerMeansBetter = false;
            if (grade == null)
                grade = Grade;
            var t = IsLongArrange(instrument, startTime, endTime,grade.Value);

            if (t !=null&& (PercentDifferentLimit <= CommonLib.CommonProc.EPSILON || t.Value < PercentDifferentLimit))
                return t;
            return null;
        }

        ISignal IsLongArrange(IInstrument instrument,DateTime start,DateTime end, MarketDataGrade grade)
        {
            var ml = GetMarketData(instrument, start, end, grade);
            if (ml.Count < 10) return null;
            if (ml.Count < 20) return IsLongArrange(ml, 5, 10);
            if (ml.Count < 30)
            {
                var t1 = IsLongArrange(ml, 5, 10);
                var t2=IsLongArrange(ml, 10, 20);
                if (t1 != null && t2 != null)
                {
                    if (t1.Value > t2.Value)
                        return t1;
                }
                return null;
            }
            {
                var t1 = IsLongArrange(ml, 5, 10);
                var t2 = IsLongArrange(ml, 10, 20);
                var t3=IsLongArrange(ml, 20, 30);
                if (t1 != null && t2 != null && t3 != null)//return max distance
                {
                    if (t1.Value > t2.Value)
                    {
                        if (t1.Value > t3.Value)
                            return t1;
                        else
                            return t3;
                    }
                    else
                    {
                        if (t2.Value > t3.Value)
                            return t2;
                        else
                            return t3;
                    }
                    //var t = new List<TimeValueObject>() { t1, t2, t3 }.OrderBy(v=>v.DoubleValue);
                    //return t.LastOrDefault();
                }
                return null;
            }

        }
        ISignal IsLongArrange(List<IMarketData> marketDataList,int shortPeriod,int longPeriod)
        {
            var m = marketDataList.FirstOrDefault();
            
            var Ls = GetMA(marketDataList, shortPeriod);
            var Ll = GetMA(marketDataList, longPeriod);

            if (Ls.Last() >= Ll.Last())
            {
                return CreateSignal(m.InstrumentTicker, marketDataList.Max(v => v.Time), (Ls.Last() - Ll.Last()) / Ll.Last(), marketDataList.LastOrDefault().Close);
                //var o = new TimeValueObject() { Name = Name, Time = marketDataList.Max(v => v.Time), Value = null };
                //o.Value = (Ls.Last() - Ll.Last()) / Ll.Last();
                //return o;

            }
            return null;
        }
        List<double> GetMA(List<IMarketData> marketDataList,int n)
        {
            try
            {
                var valueList = new List<double>();
                int begin = 0;
                int rlength = 0;
                int rbegin = 0;
                //if (MarketDataList.Count > n) { begin = MarketDataList.Count - n; }
                double[] values = new double[marketDataList.Count];
                var dataList = marketDataList.Select(v => v.Close).ToArray();
                Core.Sma(begin, marketDataList.Count - 1, dataList, n, out rbegin, out rlength, values);
                valueList = values.ToList();
                CommonProc.TrimDoubleList(valueList);
                if (IsAnalyse)
                {
                    //if (!AnalyseValueList.ContainsKey(n.ToString()))
                    //{
                    //    AnalyseValueList.Add(n.ToString(), new List<TimeValueObject>());
                    //}
                    
                    
                    if (valueList.Count > 0)
                    {
                        //var data = marketDataList.FirstOrDefault();
                        //AddSignal(data.InstrumentTicker, marketDataList.Max(v => v.Time), valueList.LastOrDefault(), marketDataList.LastOrDefault().Close, n.ToString(), SignalType.Analyse);
                        //var dif = marketDataList.Count - valueList.Count;
                        //for (int i =  valueList.Count-1 ;i>=0; i--)
                        //{

                        var o = new TimeValueObject() { Name = Name, Value = valueList.LastOrDefault() };
                        o.Time = marketDataList.LastOrDefault().Time;
                        AddReference(n.ToString(), o);
                        //}

                    }

                }

                //for (int i = 0; i < values.Length - n + 1; i++)
                //{

                //    valueList.Add(values[i]);
                //}
                return valueList;
            }
            catch (Exception ex)
            {
                LogSupport.Error(ex);
                throw ex;
            }
        }
    }

    struct MAInfo
    {
        public double MADifferent { get; set; }
        public bool IsLongArrange { get; set; }
    }
}
