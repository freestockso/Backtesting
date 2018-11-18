using BackTestingCommonLib;
using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using CommonLibForWPF;
using System.Windows;
using DistributionCondition;

namespace DistributionAnalyseCondition
{
    [SerialObjectAttribute(Name = "Distribution Condition ")]
    public class DistributionCondition : ConditionBase, IEditControlSupport
    {
        DistributeValueProcessMode _AnalyseType = DistributeValueProcessMode.VolumeChange;
        [ParameterOperation]
        public DistributeValueProcessMode AnalyseType
        {
            get { return _AnalyseType; }
            set { _AnalyseType = value; }
        }

        MarketDataGrade _Grade = MarketDataGrade.Day;
        [ParameterOperation]
        public MarketDataGrade Grade
        {
            get { return _Grade; }
            set { _Grade = value; }
        }
        DistributeAnalyse da = new DistributeAnalyse();
        protected override ISignal Calculate(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade? grade)
        {
            if (grade == null)
                grade = Grade;
            var dl = GetMarketData(instrument, startTime, endTime, grade.Value);

            da.ProcessMode = AnalyseType;
            var obj = new Signal() {Ticker=instrument.Ticker, Time = dl.LastOrDefault().Time ,Value= da.GetDistanceByTime(dl,instrument.Ticker) };
            return obj;
        }


        public override ICondition CreateInstance()
        {
            return new DistributionCondition();
        }

        public FrameworkElement GetEditControl()
        {
            return null;
        }

    }


}
