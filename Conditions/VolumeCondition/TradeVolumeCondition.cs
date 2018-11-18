using BackTestingCommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using TicTacTec.TA.Library;
using CommonLib;

namespace VolumeCondition
{
    [SerialObjectAttribute(Name ="Volume Magnify")]
    public class TradeVolumeCondition : ConditionBase
    {
        public override ICondition CreateInstance()
        {
            return new TradeVolumeCondition();
        }
        protected override ISignal Calculate(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade? grade)
        {
            isSignalBiggerMeansBetter = false;
            return IsMagnify(instrument, startTime, endTime,grade);
        }

        ISignal IsMagnify(IInstrument instrument, DateTime start, DateTime end, MarketDataGrade? grade)
        {
            if (grade == null)
                grade = Grade;
            var ml = GetMarketData(instrument, start, end, grade.Value);
            if (ml.Count <= PredicatePeriod)
                return null;
            var mal = GetMA(ml, AveragePeriod);
            if (mal.Count == 0) return null;
            var acount = Math.Min(mal.Count, AveragePeriod);
            var al = new List<double>();
            for (int i = 0; i < acount; i++)
                al.Add(mal.Count-1 - i);
            var pl = new List<double> { ml[ml.Count - 5].Volume, ml[ml.Count - 4].Volume, ml[ml.Count - 3].Volume, ml[ml.Count - 2].Volume, ml[ml.Count - 1].Volume };
            var maxLimit = al.Max()*(1+Threshold);
            if (pl.Any(v => v > maxLimit))
            {
                var o = new Signal() { Name = Name, Time = ml.Max(v => v.Time), Value = (pl.Max() - maxLimit) / maxLimit };

                return o;
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
                //if (MarketDataList.Count > n) { begin = MarketDataList.Count - n; }
                double[] values = new double[marketDataList.Count];
                var dataList = marketDataList.Select(v => v.Volume).ToArray();
                Core.Sma(begin, marketDataList.Count - 1, dataList, n, out rbegin, out rlength, values);
                var l = values.ToList<double>();
                CommonProc.TrimDoubleList(l);
                return l;

            }
            catch (Exception ex)
            {
                LogSupport.Error(ex);
                throw ex;
            }
        }

        MarketDataGrade _Grade = MarketDataGrade.Day;
        [ParameterOperation]
        public MarketDataGrade Grade
        {
            get { return _Grade; }
            set { _Grade = value; }
        }

        int _PredicatePeriod = 5;
        [ParameterOperation]
        public int PredicatePeriod
        {
            get { return _PredicatePeriod; }
            set { _PredicatePeriod = value; }
        }

        int _AveragePeriod = 5;
        [ParameterOperation]
        public int AveragePeriod
        {
            get { return _AveragePeriod; }
            set { _AveragePeriod = value; }
        }

        double _Threshold = 0.15;
        [ParameterOperation]
        public double Threshold
        {
            get { return _Threshold; }
            set { _Threshold = value; }
        }
    }
}
