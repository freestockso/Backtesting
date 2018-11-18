using BackTestingCommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using TicTacTec.TA.Library;
using CommonLib;

namespace MACDGoldCross
{
    [SerialObjectAttribute(Key = "Macd Gold Cross Predicate", Name = "MACD Gold Cross Predicate")]
    public class MacdGoldCross : ConditionBase
    {
        public override bool IsReferenceIndependence { get { return true; } }
        public override ICondition CreateInstance()
        {
            return new MacdGoldCross();
        }

        int _FastPeriod = 12;
        [ParameterOperation]
        public int FastPeriod
        {
            get { return _FastPeriod; }
            set { _FastPeriod = value; }
        }
        int _SlowPeriod = 26;
        [ParameterOperation]
        public int SlowPeriod
        {
            get { return _SlowPeriod; }
            set { _SlowPeriod = value; }
        }
        int _SignalPeriod = 9;
        [ParameterOperation]
        public int SignalPeriod
        {
            get { return _SignalPeriod; }
            set { _SignalPeriod = value; }
        }
        MarketDataGrade _Grade = MarketDataGrade.Day;
        [ParameterOperation]
        public MarketDataGrade Grade
        {
            get { return _Grade; }
            set { _Grade = value; }
        }
        protected override ISignal Calculate(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade? grade)
        {
            isSignalBiggerMeansBetter = true;
            if (grade == null)
                grade = Grade;
            try
            {
                var dl = GetMarketData(instrument, startTime, endTime, grade.Value);
                if (dl.Count == 0) return null;
                var macd = new double[dl.Count];
                var macdhis = new double[dl.Count];
                var macdsignal = new double[dl.Count];
                int bindex;
                int element;

                Core.Macd(0, dl.Count - 1, dl.Select(p => p.Close).ToArray(), FastPeriod, SlowPeriod, SignalPeriod, out bindex, out element, macd, macdsignal, macdhis);
                var signal = macdsignal.ToList();
                CommonProc.TrimDoubleList(signal);
                if (IsAnalyse)
                {
                    var macdList = macd.ToList();
                    CommonProc.TrimDoubleList(macdList);
                    var his = macdhis.ToList();
                    CommonProc.TrimDoubleList(his);
                    //if (!AnalyseValueList.ContainsKey("MACD"))
                    //{
                    //    AnalyseValueList.Add("MACD", new List<TimeValueObject>());
                    //}
                    //if (!AnalyseValueList.ContainsKey("Signal"))
                    //{
                    //    AnalyseValueList.Add("Signal", new List<TimeValueObject>());
                    //}
                    //if (!AnalyseValueList.ContainsKey("His"))
                    //{
                    //    AnalyseValueList.Add("His", new List<TimeValueObject>());
                    //}
                    if (macdList.Count > 0)
                    {
                        //AddSignal(instrument, dl.Max(v => v.Time), macdList.LastOrDefault(), dl.LastOrDefault().Close, "MACD", SignalType.Analyse);
                        //var dif = dl.Count - macdList.Count;
                        //for (int i = macdList.Count - 1; i >= 0; i--)
                        //{

                        //    var o = new TimeValueObject() { Name = Name, Value = macdList[i] };
                        //    o.Time = dl[dif + i].Time;
                        //    AddReference("MACD", o);
                        //}
                        var o = new TimeValueObject() { Name = Name, Value = macdList.LastOrDefault() };
                        o.Time = dl.Max(v => v.Time);
                        AddReference("MACD", o);
                    }
                    if (signal.Count > 0)
                    {
                        //var dif = dl.Count - signal.Count;
                        //for (int i = signal.Count - 1; i >= 0; i--)
                        //{

                        //    var o = new TimeValueObject() { Name = Name, Value = signal[i] };
                        //    o.Time = dl[dif + i].Time;
                        //    AddReference("Signal", o);
                        //}
                        //AddSignal(instrument, dl.Max(v => v.Time), signal.LastOrDefault(), dl.LastOrDefault().Close, "Signal", SignalType.Analyse);
                        var o = new TimeValueObject() { Name = Name, Value = signal.LastOrDefault() };
                        o.Time = dl.Max(v => v.Time);
                        AddReference("Signal", o);
                        //AnalyseValueList["Signal"].Add(o);
                    }
                    if (his.Count > 0)
                    {
                        //var dif = dl.Count - his.Count;
                        //for (int i = his.Count - 1; i >= 0; i--)
                        //{

                        //    var o = new TimeValueObject() { Name = Name, Value = his[i] };
                        //    o.Time = dl[dif + i].Time;
                        //    AddReference("His", o);
                        //}
                        //AddSignal(instrument, dl.Max(v => v.Time), his.LastOrDefault(), dl.LastOrDefault().Close, "His", SignalType.Analyse);
                        var o = new TimeValueObject() { Name = Name, Value = his.LastOrDefault() };
                        o.Time = dl.Max(v => v.Time);
                        AddReference("His", o);
                        //AnalyseValueList["His"].Add(o);
                    }
                }
                if (signal.Count>1&& signal[signal.Count - 2] < 0 && signal[signal.Count - 1] > 0)
                {
                    //var o = new TimeValueObject() { Name = Name, Time = dl.Max(v=>v.Time), Value = null };
                    //o.Value = signal[signal.Count - 1] - signal[signal.Count - 2];

                    //return o;
                    return CreateSignal(instrument, dl.Max(v => v.Time), signal[signal.Count - 1] - signal[signal.Count - 2], dl.LastOrDefault().Close);
                }
            }
            catch (Exception e)
            {
                //LogSupport.Error(e);

            }
            return null;
        }
    }
}
