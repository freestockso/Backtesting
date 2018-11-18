using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using CommonLib;
using TicTacTec.TA.Library;
using BackTestingCommonLib;

namespace CCICondition
{
    [SerialObjectAttribute(Key = "CCI Indicator", Name = "CCI Indicator")]
    public class CCIPredicate : ConditionBase
    {
        public override bool IsReferenceIndependence { get { return true; } }
        public override ICondition CreateInstance()
        {
            return new CCIPredicate();
        }
        protected override ISignal Calculate(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade? grade)
        {
            isSignalBiggerMeansBetter = true;
            if (grade == null)
                grade = Grade;
            try
            {
                var dl = AnalyseDataSource.GetDataList(instrument, startTime, endTime, grade.Value);
                var CCI = new double[dl.Count];

                int bindex;
                int element;

                Core.Cci(0, dl.Count - 1, dl.Select(p => p.High).ToArray(),dl.Select(p=>p.Low).ToArray(),dl.Select(p=>p.Close).ToArray(),
                    Period, out bindex, out element, CCI);
                var valueList = CCI.ToList();
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
                        AddReference("CCI", obj);
                        //var dif = dl.Count - valueList.Count;
                        //for (int i = valueList.Count - 1; i >= 0; i--)
                        //{

                        //    var o = new TimeValueObject() { Name = Name, Value = valueList[i] };
                        //    o.Time = dl[dif + i].Time;
                        //    AddReference("CCI", o);
                        //}
                    }

                }
                //var o = new TimeValueObject() { Name = Name, Time = endTime, Value = null };
                //o.Time = dl.Max(v => v.Time);
                //o.Value = RSI.ToArray().LastOrDefault();
                var cciValue = GetValue(CCI.ToList());
                if(cciValue!=null)
                    return CreateSignal(instrument, dl.Max(v => v.Time), cciValue.Value, dl.LastOrDefault().Close);
                return null;
            }
            catch (Exception e)
            {
                LogSupport.Error(e);

            }
            return null;
        }

        double? GetValue(List<double> valueList)
        {
            if (valueList.Count < 2) return null;
            var p = valueList[valueList.Count - 2];
            var c = valueList[valueList.Count - 1];

            if (p < 100 && c > 100) return 10;
            if (p > 100 && c > 100 & c > p) return GetKeepTime(valueList);
            if (p > 100 && c > 100 & p > c) return -5* GetKeepTime(valueList);
            if (p > -100 && c < -100) return -10;
            if (p < -100 && c < -100 && p > c) return -1* GetKeepTime(valueList);
            if (p < -100 && c < -100 && p < c) return 5 * GetKeepTime(valueList);
            return null;
        }

        int GetKeepTime(List<double> valueList)
        {
            var p = valueList.LastOrDefault();
            int n = 0;
            if (p > 100)
            {
                
                for(int i = valueList.Count - 1; i >= 0; i--)
                {
                    if (valueList[i] > 100) n++;
                }
            }
            if (p < 100)
            {

                for (int i = valueList.Count - 1; i >= 0; i--)
                {
                    if (valueList[i] < 100) n++;
                }
            }
            return n;
        }

        MarketDataGrade _Grade = MarketDataGrade.Day;
        [ParameterOperation]
        public MarketDataGrade Grade
        {
            get { return _Grade; }
            set { _Grade = value; }
        }

        int _Period = 14;
        [ParameterOperation]
        public int Period
        {
            get { return _Period; }
            set { _Period = value; }
        }


    }
}
