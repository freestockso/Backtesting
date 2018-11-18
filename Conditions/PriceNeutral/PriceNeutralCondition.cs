using BackTestingCommonLib;
using BackTestingInterface;
using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriceNeutral
{
    [SerialObjectAttribute(Key = "Neutral Price",Name = "Neutral Price")]
    public class PriceNeutralCondition:ConditionBase
    {
        double _ThresholdPercent = 0.03;
        [ParameterOperation]
        public double ThresholdPercent
        {
            get { return _ThresholdPercent; }
            set { _ThresholdPercent = Math.Abs(value); }
        }

        bool _IsHigh = true;
        [ParameterOperation]
        public bool IsHigh
        {
            get { return _IsHigh; }
            set { _IsHigh = value; }
        }

        MarketDataGrade _StandardGrade =  MarketDataGrade.Day;
        [ParameterOperation]
        public MarketDataGrade StandardGrade
        {
            get { return _StandardGrade; }
            set { _StandardGrade = value; }
        }

        public override ICondition CreateInstance()
        {
            return new PriceNeutralCondition();
        }

        protected override ISignal Calculate(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade? grade)
        {
            if (IsHigh)
                isSignalBiggerMeansBetter = true;
            else
                isSignalBiggerMeansBetter = false;
            try
            {
                var dl = AnalyseDataSource.GetDataList(instrument, startTime, endTime, grade.Value);
                var v = GetPreviousEndValue(dl);
                double value = 0;
                if (v > 0)
                {
                    double price = 0;
                    if(StandardGrade> dl.LastOrDefault().Grade)
                        if (IsHigh)
                            price = dl.LastOrDefault().High;
                        else
                            price = dl.LastOrDefault().Low;
                    else
                        price= dl.LastOrDefault().Close;
                    double d = (price - v) / v;
                    if (IsHigh&&d>Math.Abs(ThresholdPercent))
                        value =d- ThresholdPercent;
                    if(!IsHigh&&d<(-1*Math.Abs(ThresholdPercent)))
                        value =d+ Math.Abs(ThresholdPercent);

                    if (Math.Abs(value) > CommonProc.EPSILON)
                    {
                        return CreateSignal(instrument, dl.LastOrDefault().Time, value, price);
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }

            }

        double GetPreviousEndValue(List<IMarketData> dataList)
        {
            var dl = MarketData.SummaryMarketDataList(dataList, StandardGrade);
            if (dl.Count > 1)
                return dl[dl.Count - 2].Close;
            return -1;
        }
    }
}
