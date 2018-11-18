using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using CommonLib;
using TicTacTec.TA.Library;
using BackTestingCommonLib;

namespace RSICondition
{
    [SerialObjectAttribute(Key = "RSI Indicator", Name = "RSI Indicator")]
    public class RSIPredicate : ConditionBase
    {
        public override bool IsReferenceIndependence { get { return true; } }
        public override ICondition CreateInstance()
        {
            return new RSIPredicate();
        }
        protected override ISignal Calculate(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade? grade)
        {
            isSignalBiggerMeansBetter = true;
            if (grade == null)
                grade = Grade;
            try
            {
                var dl = AnalyseDataSource.GetDataList(instrument, startTime, endTime, grade.Value);
                var RSI = new double[dl.Count];

                int bindex;
                int element;

                Core.Rsi(0, dl.Count - 1, dl.Select(p => p.Close).ToArray(),
                    Period, out bindex, out element, RSI);
                var valueList = RSI.ToList();
                CommonProc.TrimDoubleList(valueList);
                if (IsAnalyse)
                {
                    //if (!AnalyseValueList.ContainsKey("RSI"))
                    //{
                    //    AnalyseValueList.Add("RSI", new List<TimeValueObject>());
                    //}


                    if (valueList.Count > 0)
                    {
                        //AddSignal(instrument, dl.Max(v => v.Time), valueList.LastOrDefault(), dl.LastOrDefault().Close, "RSI", SignalType.Analyse);
                        var obj = new TimeValueObject() { Name = Name, Value = valueList.LastOrDefault() };
                        obj.Time = dl.Max(v => v.Time);
                        AddReference("RSI", obj);
                        //AnalyseValueList["RSI"].Add(obj);
                        //var dif = dl.Count - valueList.Count;
                        //for (int i = valueList.Count - 1; i >= 0; i--)
                        //{

                        //    var o = new TimeValueObject() { Name = Name, Value = valueList[i] };
                        //    o.Time = dl[dif + i].Time;
                        //    AddReference("RSI", o);
                        //}
                    }

                }
                //var o = new TimeValueObject() { Name = Name, Time = endTime, Value = null };
                //o.Time = dl.Max(v => v.Time);
                //o.Value = RSI.ToArray().LastOrDefault();
                return CreateSignal(instrument,dl.Max(v=>v.Time), RSI.ToArray().LastOrDefault(),dl.LastOrDefault().Close);
            }
            catch (Exception e)
            {
                LogSupport.Error(e);

            }
            return null;
        }

        MarketDataGrade _Grade = MarketDataGrade.Week;
        [ParameterOperation]
        public MarketDataGrade Grade
        {
            get { return _Grade; }
            set { _Grade = value; }
        }

        int _Period = 12;
        [ParameterOperation]
        public int Period
        {
            get { return _Period; }
            set { _Period = value; }
        }

    }
}
