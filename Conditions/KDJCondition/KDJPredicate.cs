using BackTestingCommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using CommonLib;
using TicTacTec.TA.Library;

namespace KDICondition
{
    [SerialObjectAttribute(Key = "KDJ Predicate", Name = "KDJ Predicate")]
    public class KDJPredicate : ConditionBase
    {
        public override bool IsReferenceIndependence { get { return true; } }
        public override ICondition CreateInstance()
        {
            return new KDJPredicate();
        }

        protected override ISignal Calculate(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade? grade)
        {
            isSignalBiggerMeansBetter = true;
            if (grade == null)
                grade = Grade;
            try
            {
                var dl =AnalyseDataSource.GetDataList(instrument, startTime, endTime, grade.Value);
                var slowK = new double[dl.Count];
                var slowD = new double[dl.Count];

                int bindex;
                int element;

                Core.Stoch(0, dl.Count - 1, dl.Select(p => p.High).ToArray(), dl.Select(p => p.Low).ToArray(), dl.Select(p => p.Close).ToArray(),
                    FastKPeriod, SlowKPeriod, SlowKType, SlowDPeriod, SlowDType, out bindex, out element, slowK, slowD);
                var skl = slowK.ToList<double>();
                var sdl = slowD.ToList<double>();
                CommonProc.TrimDoubleList(skl);
                CommonProc.TrimDoubleList(sdl);
                if (IsAnalyse)
                {
                    //if (!AnalyseValueList.ContainsKey("K"))
                    //{
                    //    AnalyseValueList.Add("K", new List<TimeValueObject>());
                    //}
                    //if (!AnalyseValueList.ContainsKey("D"))
                    //{
                    //    AnalyseValueList.Add("D", new List<TimeValueObject>());
                    //}
                    if(skl.Count>0)
                    {
                        //var dif = dl.Count - skl.Count;
                        //for (int i = skl.Count - 1; i >= 0; i--)
                        //{

                        //    var o = new TimeValueObject() { Name = Name, Value = skl[i] };
                        //    o.Time = dl[dif + i].Time;
                        //    AddReference("K", o);
                        //}
                        var o = new TimeValueObject() { Name = Name, Value = skl.LastOrDefault() };
                        o.Time = dl.Max(v => v.Time);
                        AddReference("K", o);
                        //AddSignal(instrument, dl.Max(v => v.Time), skl.LastOrDefault(), dl.LastOrDefault().Close, "K", SignalType.Analyse);
                        //AnalyseValueList["K"].Add(o);
                    }
                    if (sdl.Count > 0)
                    {
                        //var dif = dl.Count - sdl.Count;
                        //for (int i = skl.Count - 1; i >= 0; i--)
                        //{

                        //    var o = new TimeValueObject() { Name = Name, Value = sdl[i] };
                        //    o.Time = dl[dif + i].Time;
                        //    AddReference("D", o);
                        //}
                        var o = new TimeValueObject() { Name = Name, Value = sdl.LastOrDefault() };
                        o.Time = dl.Max(v => v.Time);
                        AddReference("D", o);
                        //AnalyseValueList["D"].Add(o);
                        //AddSignal(instrument, dl.Max(v => v.Time), sdl.LastOrDefault(), dl.LastOrDefault().Close, "D", SignalType.Analyse);
                    }
                }
                if (skl[skl.Count - 2] < skl[skl.Count - 1] && sdl[sdl.Count - 2] < sdl[sdl.Count - 1])//current K,D increase
                {
                    return CreateSignal(instrument, dl.Max(v => v.Time), ((skl[skl.Count - 1] - skl[skl.Count - 2]) + (sdl[sdl.Count - 1] - sdl[sdl.Count - 2])) / 2d, dl.LastOrDefault().Close);
                    //var o = new TimeValueObject() { Name = Name, Time = endTime, Value = null };
                    //o.Time = dl.Max(v => v.Time);
                    //o.Value= ((skl[skl.Count - 1] - skl[skl.Count - 2]) + (sdl[sdl.Count - 1] - sdl[sdl.Count - 2])) / 2d;
                    //return o;
                }
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

        int _FastKPeriod = 12;
        [ParameterOperation]
        public int FastKPeriod
        {
            get { return _FastKPeriod; }
            set { _FastKPeriod = value; }
        }
        int _SlowKPeriod = 26;
        [ParameterOperation]
        public int SlowKPeriod
        {
            get { return _SlowKPeriod; }
            set { _SlowKPeriod = value; }
        }
        Core.MAType _SlowKType = Core.MAType.Sma;
        [ParameterOperation]
        public Core.MAType SlowKType
        {
            get { return _SlowKType; }
            set { _SlowKType = value; }
        }
        int _SlowDPeriod = 26;
        [ParameterOperation]
        public int SlowDPeriod
        {
            get { return _SlowDPeriod; }
            set { _SlowDPeriod = value; }
        }
        Core.MAType _SlowDType = Core.MAType.Sma;
        [ParameterOperation]
        public Core.MAType SlowDType
        {
            get { return _SlowDType; }
            set { _SlowDType = value; }
        }
    }
}
