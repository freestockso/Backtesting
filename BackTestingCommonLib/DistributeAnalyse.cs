using BackTestingInterface;
using CommonLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingCommonLib
{
    public enum DistributeValueProcessMode//Momentum is price delta*volume
    {
        Open,Close,High,Low,
        OpenToClose, HighToLow, HighToClose, LowToClose, CloseChange, Volume, VolumeChange, MomentumChange,
        OpenToClosePercent, HighToLowPercent, HighToClosePercent, LowToClosePercent, CloseChangePercent, VolumeChangePercent
    }
    public class DistributeValue
    {
        public string Name { get; set; }
        public double Value { get; set; }
        public DateTime Time { get; set; }
    }

    public class DistributeAnalyse
    {
        public static List<DistributeAnalyseType> GetDistributeAnalyseTypeList()
        {
            return new List<DistributeAnalyseType>()
            {
                 DistributeAnalyseType.Instrument, DistributeAnalyseType.Industory, DistributeAnalyseType.Region
            };
        }
        public static List<DistributeValueProcessMode> GetProcessModeList()
        {

            var l = new List<DistributeValueProcessMode>();
            foreach (var v in Enum.GetValues(typeof(DistributeValueProcessMode)))
                l.Add((DistributeValueProcessMode)v);
            return l;
        }

        [JsonIgnore]
        public IDataSource CurrentDataSource { get; set; }
        DateTime _StartTime = DateTime.Now - TimeSpan.FromDays(60);
        public DateTime StartTime
        {
            get { return _StartTime; }
            set { _StartTime = value;  }
        }
        DateTime _EndTime = DateTime.Now;
        public DateTime EndTime
        {
            get { return _EndTime; }
            set { _EndTime = value; }
        }
        MarketDataGrade _Grade = MarketDataGrade.FiveMinutes;
        public MarketDataGrade Grade { get { return _Grade; } set { _Grade = value;  } }

        public bool IsIncludeZero { get; set; }

        DistributeAnalyseType _AnalyseType = DistributeAnalyseType.Instrument;
        public DistributeAnalyseType AnalyseType
        {
            get { return _AnalyseType; }
            set { _AnalyseType = value; }
        }

        int _StatisticStep = 5;
        public int StatisticStep
        {
            get { return _StatisticStep; }
            set { _StatisticStep = value; }
        }

        public List<DataPoint> GetNormalDistribute()
        {
            return Distribute.GetNormalDistribute(0, 1, 3.5, 1d/StatisticStep);
        }

        DistributeValueProcessMode _ProcessMode = DistributeValueProcessMode.VolumeChange;
        public DistributeValueProcessMode ProcessMode
        {
            get { return _ProcessMode; }
            set { _ProcessMode = value; }
        }
        List<IInstrument> _InstrumentList = new List<IInstrument>();
        public List<IInstrument> InstrumentList { get { return _InstrumentList; } }
        List<string> _SectorList = new List<string>();
        public List<string> SectorList { get { return _SectorList; } }

        public List<IMarketData> GetMarketDataList()
        {
            if (CurrentDataSource == null)
                return null;
            if (AnalyseType != DistributeAnalyseType.Instrument)
            {
                InstrumentList.Clear();
                SectorList.ForEach(s =>
                {
                    InstrumentList.AddRange(Instrument.GetInstrumentBySector(s, AnalyseType));
                });
            }
            return CurrentDataSource.GetDataList(InstrumentList, StartTime, EndTime, Grade);

        }
        //得到序列里面所有差值的分布
        public Dictionary<string,Tuple< List<Tuple<double, double>>, List<Tuple<double, double>>>> GetDimentionAnalyse( int n = 20)
        {
            var r = new Dictionary<string,Tuple< List<Tuple<double, double>>, List<Tuple<double, double>>>>();
            InstrumentList.ForEach(v =>
            {
                var l = GetDimentionAnalyse(v,false, n);
                var al = GetDimentionAnalyse(v, true, n);
                r.Add(v.Name,new Tuple<List<Tuple<double, double>>, List<Tuple<double, double>>>(l,al));
            });
            return r;
        }
        public List<Tuple<double, double>> GetDimentionAnalyse(IInstrument inst,bool isAbsolute=false, int n = 20)
        {
            var ml = CurrentDataSource.GetDataList(inst, StartTime, EndTime, Grade);
            return GetDimentionAnalyse(GetDistributeDataList(ml),isAbsolute, n);
        }
        public List<Tuple<double,double>> GetDimentionAnalyse(List<double> l, bool isAbsolute = false,int n=20)
        {
            if (l == null || l.Count < 3) return null;

            var dl = new List<double>();
            for(var i = 0; i < l.Count-1; i++)
            {
                for(int j = i + 1; j < l.Count; j++) {
                    if(isAbsolute)
                        dl.Add(Math.Abs(l[j] - l[i]));
                    else
                        dl.Add(l[j] - l[i]);
                }
                
            }
            var start = dl.Min();
            var dis = dl.Max() - start;
            var area = dis / n;

            List<Tuple<double, double>> r = new List<Tuple<double, double>>();
            for (var c = start; c < start + dis; c += area)
            {
                r.Add(new Tuple<double, double>(c, dl.Count(v => v < c + area)));
            }
            return r;

        }

        //相邻的节点值构成新的图形
        public Dictionary<string, List<Tuple<double, double>>> Get2DCurve()
        {
            var r = new Dictionary<string, List<Tuple<double, double>>>();
            InstrumentList.ForEach(v =>
            {
                var l = Get2DCurve(v);
                r.Add(v.Name, l);
            });
            return r;
        }
        public List<Tuple<double, double>> Get2DCurve(IInstrument inst)
        {
            var ml = CurrentDataSource.GetDataList(inst, StartTime, EndTime, Grade);
            return Get2DCurve(GetDistributeDataList(ml));
        }
        public List<Tuple<double, double>> Get2DCurve(List<double> l)
        {
            List<Tuple<double, double>> r = new List<Tuple<double, double>>();
            for (var c = 0; c < l.Count-1; c ++)
            {
                r.Add(new Tuple<double, double>(l[c], l[c+1]));
            }
            return r;
        }
        //相邻的三元组
        public Dictionary<string, List<Tuple<double, double,double>>> Get3DCurve()
        {
            var r = new Dictionary<string, List<Tuple<double, double,double>>>();
            InstrumentList.ForEach(v =>
            {
                var l = Get3DCurve(v);
                r.Add(v.Name, l);
            });
            return r;
        }
        public List<Tuple<double, double,double>> Get3DCurve(IInstrument inst)
        {
            var ml = CurrentDataSource.GetDataList(inst, StartTime, EndTime, Grade);
            return Get3DCurve(GetDistributeDataList(ml));
        }
        public List<Tuple<double, double,double>> Get3DCurve(List<double> l)
        {
            List<Tuple<double, double,double>> r = new List<Tuple<double, double,double>>();
            for (var c = 0; c < l.Count - 2; c++)
            {
                r.Add(new Tuple<double, double,double>(l[c], l[c + 1],l[c+2]));
            }
            return r;
        }

        public double GetDistanceByTime(List<IMarketData> dataList, string targetName)
        {
            var l = GetDistributeCube(dataList);
            l = l.Where(v => v.Name == targetName).ToList();
            if (!IsIncludeZero)
                l = l.Where(v => Math.Abs(v.Value) > CommonProc.EPSILON).ToList();
            return Distribute.GetDistance(l.OrderBy(v => v.Time).LastOrDefault().Value, l.Select(v => v.Value).ToList());
        }

        public Dictionary<string,List<DataPoint>> GetDistributeByTime(List<IMarketData> dataList)
        {
            var l = GetDistributeCube(dataList);
            if (!IsIncludeZero)
                l = l.Where(v => Math.Abs(v.Value) > CommonProc.EPSILON).ToList();
            var nl = l.Select(v => v.Name).Distinct().ToList();

            
            var rl = new Dictionary<string, List<DataPoint>>();
            nl.ForEach(n =>
            {
                var statistic = l.Where(v => v.Name == n).ToList();
                rl.Add(n, Distribute.GetDistribution(statistic.Select(v => v.Value).ToList(), StatisticStep));
            });
            return rl;
        }

        public Dictionary<DateTime, List<DataPoint>> GetDistributeByMarket(List<IMarketData> dataList)
        {
            var l = GetDistributeCube(dataList);
            if (!IsIncludeZero)
                l = l.Where(v => Math.Abs(v.Value) > CommonProc.EPSILON).ToList();
            var tl = l.Select(v => v.Time).Distinct().ToList();
            var rl = new Dictionary<DateTime, List<DataPoint>>();
            tl.ForEach(t =>
            {
                var statisic = l.Where(v => v.Time == t).ToList();
                rl.Add(t, Distribute.GetDistribution(statisic.Select(v => v.Value).ToList(), StatisticStep));
            });

            return rl;
        }

        public List<DistributeValue> GetDistributeTrendByTime(List<IMarketData> dataList)
        {
            var l = GetDistributeCube(dataList).OrderBy(v => v.Time).ToList();
            if (!IsIncludeZero)
                l = l.Where(v => Math.Abs(v.Value) > CommonProc.EPSILON).ToList();
            var nl = l.Select(v => v.Name).Distinct().ToList();
            var tl= l.Select(v => v.Time).Distinct().ToList();
            var rl = new List<DistributeValue>();

            nl.ForEach(v =>
            {
                var cl= l.Where(s => s.Name == v).ToList();
                var statisticList = cl.Select(s => s.Value).ToList();
                tl.ForEach(t =>
                {
                    var data = cl.FirstOrDefault(c => c.Time == t);
                    if(data!=null)
                        rl.Add(new DistributeValue() {Name=v, Time = t, Value = Distribute.GetDistance(data.Value, statisticList) });
                });
                
            });

            return rl;
        }
        public List<DistributeValue> GetDistributeTrendByMarket(List<IMarketData> dataList)
        {
            var l = GetDistributeCube(dataList);
            if (!IsIncludeZero)
                l = l.Where(v => Math.Abs(v.Value) > CommonProc.EPSILON).ToList();
            var nl = l.Select(v => v.Name).Distinct().ToList();
            var tl = l.Select(v => v.Time).Distinct().ToList();
            var rl = new List<DistributeValue>();
            tl.ForEach(t =>
            {
                var ml = l.Where(c => c.Time == t).ToList();
                var statisticList = ml.Select(m => m.Value).ToList();
                nl.ForEach(v =>
                {
                    var data = ml.FirstOrDefault(c => c.Time == t);
                    rl.Add(new DistributeValue() { Name = v, Time = t, Value = Distribute.GetDistance(data.Value, statisticList) });
                });

            });

            return rl;
        }

        //public bool IsChangeMode(DistributeValueProcessMode mode)
        //{
        //    if (mode == DistributeValueProcessMode.CloseChange || mode == DistributeValueProcessMode.VolumeChange || mode == DistributeValueProcessMode.MomentumChange
        //        ||mode== DistributeValueProcessMode.CloseChangePercent||mode== DistributeValueProcessMode.VolumeChangePercent)
        //        return true;
        //    return false;
        //}


        public List<DistributeValue> GetDistributeCube(List<IMarketData> dataList)
        {
            if (AnalyseType == DistributeAnalyseType.Instrument)
                return GetInstrumentDistributeCube(dataList);
            if ((AnalyseType != DistributeAnalyseType.Instrument)&&
                ProcessMode != DistributeValueProcessMode.Volume&& 
                ProcessMode != DistributeValueProcessMode.VolumeChange&& 
                ProcessMode != DistributeValueProcessMode.VolumeChangePercent)
            {
                return null;
            }

            var tl = dataList.Select(v => v.Time).Distinct().ToList();

            var sl = new List<DistributeValue>();
            var tsl = new List<DistributeValue>();
            tl.ForEach(t =>
            {
                var cl = dataList.Where(v => v.Time == t);
                tsl.Clear();
                SectorList.ForEach(s =>
                {
                    tsl.Add(new DistributeValue() { Name = s, Time = t });
                });
                foreach (var data in cl)
                {
                    var sector = Instrument.GetInstrumentSectorName(data.InstrumentTicker, AnalyseType);
                    var ts = tsl.FirstOrDefault(n => n.Name == sector);
                    if (ts != null)
                    {
                        ts.Value += data.Volume;
                    }
                }
                sl.AddRange(tsl);
            });
            
            var rl = new List<DistributeValue>();
            SectorList.ForEach(v =>
            {
                var l = sl.Where(s => s.Name == v).OrderBy(s => s.Time).ToList();
                rl.AddRange(GetSectorDataList(l, ProcessMode));
            });

            return rl;
        }

        public List<DistributeValue> GetInstrumentDistributeCube(List<IMarketData> dataList)
        {
            var l = new List<DistributeValue>();
            var tickerList = dataList.Select(v => v.InstrumentTicker).Distinct().ToList();
            var timeList= dataList.Select(v => v.Time).Distinct().ToList();


            tickerList.ForEach(inst =>
            {
                var instlist = dataList.Where(v => v.InstrumentTicker == inst).OrderBy(v=>v.Time).ToList();
                timeList.ForEach(t =>
                {
                    if(!instlist.Any(v => v.Time == t))
                    {
                        instlist.Add(new MarketData() { InstrumentTicker = inst, Time = t });
                    }

                });
                l.AddRange(GetInstrumentDataList(instlist, ProcessMode));
            });
            return l;
        }
        //得到需要深度分析的数据列
        public List<double> GetDistributeDataList(List<IMarketData> dataList)
        {
            return GetInstrumentDataList(dataList, ProcessMode).Select(v => v.Value).ToList();
        }

        //把市场数据转为可以用户要求的类型的分析的时间和数值序列
        List<DistributeValue> GetInstrumentDataList(List<IMarketData> dataList,DistributeValueProcessMode processMode)
        {
            var dl = dataList.OrderBy(v => v.Time).ToList();
            if (dataList.Select(v => v.InstrumentTicker).Distinct().Count() != 1)
                throw new Exception("market data list have no valid ticker or more than one ticker");
            var data = dataList.FirstOrDefault();
            var vl = new List<DistributeValue>();



            if (processMode == DistributeValueProcessMode.CloseChange)
            {
                for (int i = 1; i < dl.Count; i++)
                {
                    vl.Add(new DistributeValue() {Name=data.InstrumentTicker, Value = dl[i].Close - dl[i - 1].Close, Time = dl[i].Time });
                }
            }
            if (processMode == DistributeValueProcessMode.CloseChangePercent)
            {
                for (int i = 1; i < dl.Count; i++)
                {
                    if(dl[i-1].Close>CommonProc.EPSILON)
                        vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = (dl[i].Close - dl[i - 1].Close)/ dl[i-1].Close, Time = dl[i].Time });
                    else
                        vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = 0, Time = dl[i].Time });

                }
            }
            if (processMode == DistributeValueProcessMode.HighToClose)
            {
                for (int i = 0; i < dl.Count; i++)
                {

                    vl.Add(new DistributeValue() {Name=data.InstrumentTicker, Value = dl[i].High - dl[i].Close, Time = dl[i].Time });
                }
            }
            if (processMode == DistributeValueProcessMode.HighToClosePercent)
            {
                for (int i = 0; i < dl.Count; i++)
                {
                    if (dl[i].Close > CommonProc.EPSILON)
                        vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = (dl[i].High - dl[i].Close) / dl[i].Close, Time = dl[i].Time });
                    else
                        vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = 0, Time = dl[i].Time });

                }
            }
            if (processMode == DistributeValueProcessMode.LowToClose)
            {
                for (int i = 0; i < dl.Count; i++)
                {
                    vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = dl[i].Low - dl[i].Close, Time = dl[i].Time });
                }
            }
            if (processMode == DistributeValueProcessMode.LowToClosePercent)
            {
                for (int i = 0; i < dl.Count; i++)
                {
                    if (dl[i].Close > CommonProc.EPSILON)
                        vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = (dl[i].Low - dl[i].Close) / dl[i].Close, Time = dl[i].Time });
                    else
                        vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = 0, Time = dl[i].Time });
                }
            }
            if (processMode == DistributeValueProcessMode.OpenToClose)
            {
                for (int i = 0; i < dl.Count; i++)
                {
                    vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = dl[i].Open - dl[i].Close, Time = dl[i].Time });
                }
            }
            if (processMode == DistributeValueProcessMode.OpenToClosePercent)
            {
                for (int i =0; i < dl.Count; i++)
                {
                    if (dl[i].Close > CommonProc.EPSILON)
                        vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = (dl[i].Open - dl[i].Close) / dl[i].Close, Time = dl[i].Time });
                    else
                        vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = 0, Time = dl[i].Time });
                }
            }
            if (processMode == DistributeValueProcessMode.HighToLow)
            {
                for (int i = 0; i < dl.Count; i++)
                {
                    vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = dl[i].High - dl[i].Low, Time = dl[i].Time });
                }
            }
            if (processMode == DistributeValueProcessMode.HighToLowPercent)
            {
                for (int i = 0; i < dl.Count; i++)
                {
                    if (dl[i].Low > CommonProc.EPSILON)
                        vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = (dl[i].High - dl[i].Low) / dl[i].Low, Time = dl[i].Time });
                    else
                        vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = 0, Time = dl[i].Time });
                }
            }
            if (processMode == DistributeValueProcessMode.Open)
            {
                for (int i = 0; i < dl.Count; i++)
                {
                    vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = dl[i].Open, Time = dl[i].Time });
                }
            }
            if (processMode == DistributeValueProcessMode.Close)
            {
                for (int i = 0; i < dl.Count; i++)
                {
                    vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = dl[i].Close, Time = dl[i].Time });
                }
            }
            if (processMode == DistributeValueProcessMode.High)
            {
                for (int i = 0; i < dl.Count; i++)
                {
                    vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = dl[i].High, Time = dl[i].Time });
                }
            }
            if (processMode == DistributeValueProcessMode.Low)
            {
                for (int i = 0; i < dl.Count; i++)
                {
                    vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = dl[i].Low, Time = dl[i].Time });
                }
            }
            if (processMode == DistributeValueProcessMode.Volume)
            {
                for (int i = 0; i < dl.Count; i++)
                {
                    vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = dl[i].Volume, Time = dl[i].Time });
                }
            }
            if (processMode == DistributeValueProcessMode.VolumeChange)
            {
                for (int i = 1; i < dl.Count; i++)
                {
                    vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = dl[i].Volume - dl[i - 1].Volume, Time = dl[i].Time });
                }
            }
            if (processMode == DistributeValueProcessMode.VolumeChangePercent)
            {
                for (int i = 1; i < dl.Count; i++)
                {
                    if (dl[i-1].Volume > CommonProc.EPSILON)
                        vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = (dl[i].Volume - dl[i-1].Volume) / dl[i-1].Volume, Time = dl[i].Time });
                    else
                        vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = 0, Time = dl[i].Time });
                }
            }
            if (processMode == DistributeValueProcessMode.MomentumChange)
            {
                for (int i = 1; i < dl.Count; i++)
                {
                    vl.Add(new DistributeValue() { Name = data.InstrumentTicker, Value = (dl[i].Close - dl[i - 1].Close) * dl[i].Volume, Time = dl[i].Time });
                }
            }

            return vl;
        }
        List<DistributeValue> GetSectorDataList(List<DistributeValue> dataList, DistributeValueProcessMode processMode)
        {
            var dl = dataList.OrderBy(v => v.Time).ToList();
            if (dataList.Select(v => v.Name).Distinct().Count() != 1)
                throw new Exception("sector data list have no valid name or more than one name");

            var vl = new List<DistributeValue>();

            if (processMode == DistributeValueProcessMode.Volume)
            {
                return dl;
            }
            if (processMode == DistributeValueProcessMode.VolumeChange)
            {
                for (int i = 1; i < dl.Count; i++)
                {
                    vl.Add(new DistributeValue() { Name = dl[i].Name, Value = dl[i].Value - dl[i - 1].Value, Time = dl[i].Time });
                }
                return vl;
            }
            if (processMode == DistributeValueProcessMode.VolumeChangePercent)
            {
                for (int i = 1; i < dl.Count; i++)
                {
                    if (dl[i - 1].Value > CommonProc.EPSILON)
                        vl.Add(new DistributeValue() { Name = dl[i].Name, Value = (dl[i].Value - dl[i - 1].Value) / dl[i - 1].Value, Time = dl[i].Time });
                    else
                        vl.Add(new DistributeValue() { Name = dl[i].Name, Value = 0, Time = dl[i].Time });
                }
            }

            return vl;
        }

    }
}
