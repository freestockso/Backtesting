using BackTestingCommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using CommonLib;

namespace CommonIndicatorCondition
{
    [SerialObjectAttribute(Name = "Common Indicator")]
    public class CommonIndicator : ConditionBase
    {
        MarketDataGrade _Grade = MarketDataGrade.Day;
        [ParameterOperation]
        public MarketDataGrade Grade
        {
            get { return _Grade; }
            set { _Grade = value; }
        }

        protected override ISignal Calculate(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade? grade)
        {
            throw new NotImplementedException();
        }

        public override ICondition CreateInstance()
        {
            return new CommonIndicator();
        }

    }
}
