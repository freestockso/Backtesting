using BackTestingCommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using CommonLib;

namespace BasicFundamentalCondition
{
    [SerialObjectAttribute(Key = "Common fundamental condition", Name = "Basic fundamental condition", Memo = "filter instrument from fundamental information")]
    public class FundamentalCondition : ConditionBase
    {
        double _PELimit = 30;
        [ParameterOperation]
        public double PELimit
        {
            get { return _PELimit; }
            set { _PELimit=value; }
        }
        double _PBLimit = 3;
        [ParameterOperation]
        public double PBLimit
        {
            get { return _PBLimit; }
            set { _PBLimit=value; }
        }
        bool _IsSelectBetterThanAverage = false;
        [ParameterOperation]
        public bool IsSelectBetterThanAverage
        {
            get { return _IsSelectBetterThanAverage; }
            set { _IsSelectBetterThanAverage = value; }
        }
        bool _IsSelectBetterPE = true;
        [ParameterOperation]
        public bool IsSelectBetterPE
        {
            get { return _IsSelectBetterPE; }
            set { _IsSelectBetterPE = value; }
        }
        public override ICondition CreateInstance()
        {
            return new FundamentalCondition();
        }
        double average = 0;
        protected override ISignal Calculate(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade? grade)
        {
            var o = new Signal() { Name = Name, Time = endTime,Ticker=instrument.Ticker,IsPositive=isSignalBiggerMeansBetter };
            isSignalBiggerMeansBetter = false;
            if (average < CommonProc.EPSILON)
            {
                average = GetInstrumentList().Where(v => v.PE > 0).Average(v => v.PE);
            }
            if (IsSelectBetterThanAverage)
            {
                if (instrument.PE > average)
                {
                    o.Value = instrument.PE;
                    return o;
                }
            }
            else
            {
                if (instrument.PE < PELimit && instrument.PB < PBLimit)
                    if (IsSelectBetterPE)
                    {
                        o.Value = instrument.PE;
                        return o;
                    }
                    else {
                        o.Value = instrument.PB;
                        return o;
                    }
            }
            return null;
        }
        public override void GenerateResult(IInstrument instruemnt, DateTime startTime, DateTime endTime, MarketDataGrade? grade)
        {
            
            var source = GetInstrumentList();
            if (IsSelectBetterThanAverage)
            {
                average = source.Where(v => v.PE > 0).Average(v => v.PE);
                if (instruemnt.PE > average)
                    AddSignal(instruemnt,endTime, instruemnt.PE,instruemnt.CurrentPrice);
            }
            else
            {
                if(instruemnt.PE<PELimit&&instruemnt.PB<PBLimit)
                    if(IsSelectBetterPE)
                        AddSignal(instruemnt,endTime, instruemnt.PE,instruemnt.CurrentPrice);
                    else
                        AddSignal(instruemnt, endTime, instruemnt.PB, instruemnt.CurrentPrice);
            }

        }


    }
}
