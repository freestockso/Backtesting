using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using Newtonsoft.Json;

namespace BackTestingCommonLib
{
    public class MarketData :IMarketData
    {
        public string InstrumentTicker { get; set; }

        public double Low { get; set; }
        public double High { get; set; }
        public double Open { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public double Shares { get; set; }

        private string _currencyCode = "RMB";

        static double _StandardPriceValue = 1000;
        static double _StandardVolumeValue = 1000000;

        [JsonIgnore]
        public static double StandardPriceValue
        {
            get { return _StandardPriceValue; }
            set { _StandardPriceValue = value; }
        }
        [JsonIgnore]
        public static double StandardVolumeValue
        {
            get { return _StandardVolumeValue; }
            set { _StandardVolumeValue = value; }
        }

        public void Random(Random r)
        {
            Open = r.NextDouble() * StandardPriceValue;
            Close = r.NextDouble() * StandardPriceValue;

            High = r.NextDouble() * StandardPriceValue;
            Low = r.NextDouble() * StandardPriceValue;

            High = Math.Max(Math.Max(Open, Close), High);
            Low = Math.Min(Math.Min(Open, Close), Low);

            Volume = Convert.ToInt64(r.NextDouble() * StandardVolumeValue);
            Time = DateTime.Now;
        }

        private DateTime _time;
        public DateTime Time
        {
            get { return _time; }
            set
            {
                _time = value;
            }
        }

        public string CurrentCurrency
        {
            get { return _currencyCode; }
            set { _currencyCode = value;  }
        }

        public void ChangeFxRate(double fxRate, string targetCurrency)
        {
            if (Close>0)
                Close = Close * fxRate;
            if (Open > 0)
                Open = Open * fxRate;
            if (Low > 0)
                Low = Low * fxRate;
            if (High > 0)
                High = High * fxRate;
            CurrentCurrency = targetCurrency;
        }

        [JsonIgnore]
        public string ValueShowString
        {
            get
            {
                return InstrumentTicker+ "(" + CurrentCurrency + "),Open:"
                    + String.Format("{0:n4}", Open)
                    + ",High:" + String.Format("{0:n4}", High)
                    + ",Low:" + String.Format("{0:n4}", Low)
                    + ",Close:" + String.Format("{0:n4}", Close)
                    + ";Volume:" + String.Format("{0:n4}", Volume);
            }
        }

        MarketDataGrade _Grade = MarketDataGrade.FiveMinutes;//default
        public MarketDataGrade Grade
        {
            get
            {
                return _Grade;
            }

            set
            {
                _Grade = value;
            }
        }
        public static List<MarketDataGrade> GetMarketDataGradeList()
        {
            return new List<MarketDataGrade>()
            {
                MarketDataGrade.FiveMinutes,
                MarketDataGrade.FifteenMinutes,
                MarketDataGrade.HalfHour,
                MarketDataGrade.Hour,
                MarketDataGrade.HalfDay,
                MarketDataGrade.Day,
                MarketDataGrade.ThreeDays,
                MarketDataGrade.Week,
                MarketDataGrade.HalfMonth,
                MarketDataGrade.Month,
                MarketDataGrade.Season,
                MarketDataGrade.HalfYear,
                MarketDataGrade.Year
            };
        }
        public static DateTime GetNextTime(DateTime time,MarketDataGrade grade)
        {
            if (grade == MarketDataGrade.Year)
                return new DateTime(time.Year + 1, 1, 1);
            if(grade== MarketDataGrade.HalfYear)
            {
                if (time.Month < 7) return new DateTime(time.Year, 7, 1);
                return new DateTime(time.Year + 1, 1, 1);
            }
            if (grade == MarketDataGrade.Season)
            {
                if (time.Month < 4) return new DateTime(time.Year, 4, 1);
                if (time.Month < 7) return new DateTime(time.Year, 7, 1);
                if (time.Month < 10) return new DateTime(time.Year, 10, 1);
                return new DateTime(time.Year + 1, 1, 1);
            }
            if (grade == MarketDataGrade.Month)
            {
                if (time.Month < 12) return new DateTime(time.Year, time.Month+1 ,1);
                return new DateTime(time.Year + 1, 1, 1);
            }
            if(grade== MarketDataGrade.HalfMonth)
            {
                if (time.Day < 16) return new DateTime(time.Year, time.Month, 16);
                if (time.Month < 12) return new DateTime(time.Year, time.Month + 1, 1);
                return new DateTime(time.Year + 1, 1, 1);
            }
            if(grade== MarketDataGrade.Week)
            {
                if (time.DayOfWeek == DayOfWeek.Monday)
                    return (new DateTime(time.Year, time.Month, time.Day) + TimeSpan.FromDays(7));
                if (time.DayOfWeek == DayOfWeek.Tuesday)
                    return (new DateTime(time.Year, time.Month, time.Day) + TimeSpan.FromDays(6));
                if (time.DayOfWeek == DayOfWeek.Wednesday)
                    return (new DateTime(time.Year, time.Month, time.Day) + TimeSpan.FromDays(5));
                if (time.DayOfWeek == DayOfWeek.Thursday)
                    return (new DateTime(time.Year, time.Month, time.Day) + TimeSpan.FromDays(4));
                if (time.DayOfWeek == DayOfWeek.Friday)
                    return (new DateTime(time.Year, time.Month, time.Day) + TimeSpan.FromDays(3));
                if (time.DayOfWeek == DayOfWeek.Saturday)
                    return (new DateTime(time.Year, time.Month, time.Day) + TimeSpan.FromDays(2));
                if (time.DayOfWeek == DayOfWeek.Sunday)
                    return (new DateTime(time.Year, time.Month, time.Day) + TimeSpan.FromDays(1));
            }
            if(grade== MarketDataGrade.ThreeDays)
            {
                return (new DateTime(time.Year, time.Month, time.Day) + TimeSpan.FromDays(3));
            }
            if(grade== MarketDataGrade.Day)
            {
                return (new DateTime(time.Year, time.Month, time.Day) + TimeSpan.FromDays(1));
            }
            if (grade == MarketDataGrade.HalfDay)
            {
                if (time.Hour < 12) return new DateTime(time.Year, time.Month, time.Day, 12, 0, 0);
                var temp = time + TimeSpan.FromDays(1);
                return new DateTime(temp.Year, temp.Month, temp.Day) ;
            }
            if (grade == MarketDataGrade.Hour)
            {
                if (time.Hour <= 10&&time.Minute<=30) return new DateTime(time.Year, time.Month, time.Day, 10, 31, 0);
                if (time.Hour < 12) return new DateTime(time.Year, time.Month, time.Day, 12, 0, 0);
                if (time.Hour < 14) return new DateTime(time.Year, time.Month, time.Day, 14, 1, 0);
                var temp = time + TimeSpan.FromDays(1);
                return new DateTime(temp.Year, temp.Month, temp.Day);
            }
            if (grade == MarketDataGrade.HalfHour)
            {
                if (time.Hour < 10 ||(time.Hour==10&&time.Minute==0)) return new DateTime(time.Year, time.Month, time.Day, 10, 1, 0);
                if (time.Hour == 10&&time.Minute<=30) return new DateTime(time.Year, time.Month, time.Day, 10, 31, 0);
                if (time.Hour < 11|| (time.Hour == 11 && time.Minute == 0)) return new DateTime(time.Year, time.Month, time.Day, 11, 1, 0);
                if (time.Hour == 11 && time.Minute <= 30) return new DateTime(time.Year, time.Month, time.Day, 11, 31, 0);

                if (time.Hour == 13 && time.Minute <= 30) return new DateTime(time.Year, time.Month, time.Day, 13, 31, 0);
                if (time.Hour < 14 || (time.Hour == 14 && time.Minute == 0)) return new DateTime(time.Year, time.Month, time.Day, 14, 1, 0);
                if (time.Hour == 14 && time.Minute <= 30) return new DateTime(time.Year, time.Month, time.Day, 14, 31, 0);
                var temp = time + TimeSpan.FromDays(1);
                return new DateTime(temp.Year, temp.Month, temp.Day);
            }
            if (grade == MarketDataGrade.FifteenMinutes)
            {

                if (time.Hour == 9 && time.Minute <= 45) return new DateTime(time.Year, time.Month, time.Day, 9, 46, 0);
                if (time.Hour < 10 || (time.Hour == 10 && time.Minute == 0)) return new DateTime(time.Year, time.Month, time.Day, 10, 1, 0);
                if (time.Hour == 10 && time.Minute <= 15) return new DateTime(time.Year, time.Month, time.Day, 10, 16, 0);
                if (time.Hour == 10 && time.Minute <= 30) return new DateTime(time.Year, time.Month, time.Day, 10, 31, 0);
                if (time.Hour == 10 && time.Minute <= 45) return new DateTime(time.Year, time.Month, time.Day, 10, 46, 0);
                if (time.Hour < 11 || (time.Hour == 11 && time.Minute == 0)) return new DateTime(time.Year, time.Month, time.Day, 11, 1, 0);
                if (time.Hour == 11 && time.Minute <= 15) return new DateTime(time.Year, time.Month, time.Day, 11, 16, 0);
                if (time.Hour == 11 && time.Minute <= 30) return new DateTime(time.Year, time.Month, time.Day, 11, 31, 0);

                if (time.Hour == 13 && time.Minute <= 15) return new DateTime(time.Year, time.Month, time.Day, 13, 16, 0);
                if (time.Hour == 13 && time.Minute <= 30) return new DateTime(time.Year, time.Month, time.Day, 13, 31, 0);
                if (time.Hour == 13 && time.Minute <= 45) return new DateTime(time.Year, time.Month, time.Day, 13, 46, 0);
                if (time.Hour < 14 || (time.Hour == 14 && time.Minute == 0)) return new DateTime(time.Year, time.Month, time.Day, 14, 1, 0);
                if (time.Hour == 14 && time.Minute <= 15) return new DateTime(time.Year, time.Month, time.Day, 14, 16, 0);
                if (time.Hour == 14 && time.Minute <= 30) return new DateTime(time.Year, time.Month, time.Day, 14, 31, 0);

                if (time.Hour == 14 && time.Minute <= 45) return new DateTime(time.Year, time.Month, time.Day, 14, 46, 0);
                var temp = time + TimeSpan.FromDays(1);
                return new DateTime(temp.Year, temp.Month, temp.Day);
            }
            var minmode = time.Minute % 5;
            var t = time + TimeSpan.FromMinutes(5) - TimeSpan.FromMinutes(minmode);
            return new DateTime(t.Year, t.Month, t.Day, t.Hour, t.Minute, 0);
        }

        public static List<IMarketData> SummaryMarketDataList(List<IMarketData> source, MarketDataGrade grade)
        {
            if (source.Count == 0) return source;
            var fm = source.FirstOrDefault();
            if (grade == fm.Grade) return source;
            if (grade < fm.Grade) throw new Exception("Con not summary market data from "+fm.Grade .ToString() +" to "+ grade.ToString());
            var tl = FindTimeStep(source, grade);
            var l = new List<IMarketData>();
            for(int i = 0; i < tl.Count-1; i++)
            {
                if( source.Any(v => v.Time > tl[i] && v.Time <= tl[i + 1]))
                {
                    var m = Compose(source.Where(v => v.Time > tl[i] && v.Time <= tl[i + 1]).ToList(), grade);
                    l.Add(m);
                }
                
            }
            return l;


        }
        public static IMarketData Compose(List<IMarketData> source, MarketDataGrade grade)
        {
            if (source.Count == 0) return null;
            var m = new MarketData();
            var s = source.OrderBy(v => v.Time);
            m.Open = s.FirstOrDefault().Open;
            m.High = s.FirstOrDefault().High;
            m.Low = s.FirstOrDefault().Low;
            m.Close = s.LastOrDefault().Close;
            m.Time = s.LastOrDefault().Time;
            foreach(var v in s)
            {
                if (m.High < v.High)
                    m.High = v.High;
                if (m.Low > v.Low)
                    m.Low = v.Low;
                m.Shares += v.Shares;
                m.Volume += v.Volume;

            }
            m.InstrumentTicker = source.FirstOrDefault().InstrumentTicker;
            m.Grade = grade;
            return m;
        }
        public static List<DateTime> FindTimeStep(List<IMarketData> source, MarketDataGrade grade)
        {
            var tl = new List<DateTime>();
            var st = source.Min(v => v.Time);
            var et = source.Max(v => v.Time);
            tl.Add(st - TimeSpan.FromDays(1));
            if(grade== MarketDataGrade.Year)
            {
                for(int i = st.Year; i <= et.Year; i++)
                {
                    tl.Add(new DateTime(i, 1, 1));
                }
            }
            if (grade == MarketDataGrade.HalfYear)
            {
                for (int i = st.Year; i <= et.Year; i++)
                {
                    tl.Add(new DateTime(i, 1, 1));
                    tl.Add(new DateTime(i, 7, 1));
                }
            }
            if(grade== MarketDataGrade.Season)
            {
                for (int i = st.Year; i <= et.Year; i++)
                {
                    tl.Add(new DateTime(i, 1, 1));
                    tl.Add(new DateTime(i, 4, 1));
                    tl.Add(new DateTime(i, 7, 1));
                    tl.Add(new DateTime(i, 10, 1));
                }
            }
            if (grade == MarketDataGrade.Month)
            {
                for (int i = st.Year; i <= et.Year; i++)
                {
                    tl.Add(new DateTime(i, 1, 1));
                    tl.Add(new DateTime(i, 2, 1));
                    tl.Add(new DateTime(i, 3, 1));
                    tl.Add(new DateTime(i, 4, 1));
                    tl.Add(new DateTime(i, 5, 1));
                    tl.Add(new DateTime(i, 6, 1));
                    tl.Add(new DateTime(i, 7, 1));
                    tl.Add(new DateTime(i, 8, 1));
                    tl.Add(new DateTime(i, 9, 1));
                    tl.Add(new DateTime(i, 10, 1));
                    tl.Add(new DateTime(i,11, 1));
                    tl.Add(new DateTime(i, 12, 1));
                }
            }
            if (grade == MarketDataGrade.HalfMonth)
            {
                var start = CommonLib.CommonProc.GetWeekMonday(st);

                for (var i = start; i <= et+TimeSpan.FromDays(20); i=i+TimeSpan.FromDays(14))
                {
                    tl.Add(new DateTime(i.Value.Year, i.Value.Month, i.Value.Day));
                }
            }
            if (grade == MarketDataGrade.Week)
            {
                var start = CommonLib.CommonProc.GetWeekMonday(st);

                for (var i = start; i <= et + TimeSpan.FromDays(8); i = i + TimeSpan.FromDays(7))
                {
                    tl.Add(new DateTime(i.Value.Year, i.Value.Month, i.Value.Day));
                }
            }
            if (grade == MarketDataGrade.ThreeDays)
            {
                for (var i = st.Date; i <= et + TimeSpan.FromDays(3); i = i + TimeSpan.FromDays(3))
                {
                    tl.Add(new DateTime(i.Year, i.Month, i.Day));
                }
            }
            if (grade == MarketDataGrade.Day)
            {
                for (var i = st.Date; i <= et + TimeSpan.FromDays(1); i = i + TimeSpan.FromDays(1))
                {
                    tl.Add(new DateTime(i.Year, i.Month, i.Day));
                }
            }
            if (grade == MarketDataGrade.HalfDay)
            {
                for (var i = st.Date; i <= et + TimeSpan.FromDays(1); i = i + TimeSpan.FromDays(1))
                {
                    tl.Add(new DateTime(i.Year, i.Month, i.Day));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day,12,0,0));
                }
            }
            if(grade== MarketDataGrade.Hour)
            {
                for (var i = st.Date; i <= et + TimeSpan.FromDays(1); i = i + TimeSpan.FromDays(1))
                {
                    //tl.Add(new DateTime(i.Year, i.Month, i.Day));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 10, 30, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 11, 30, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 14, 0, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 15, 0, 0));
                }
            }
            if (grade == MarketDataGrade.HalfHour)
            {
                for (var i = st.Date; i <= et + TimeSpan.FromDays(1); i = i + TimeSpan.FromDays(1))
                {
                    //tl.Add(new DateTime(i.Year, i.Month, i.Day));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 10, 0, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 10, 30, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 11, 0, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 11, 30, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 13, 30, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 14, 0, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 14, 30, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 15, 0, 0));
                }
            }
            if (grade == MarketDataGrade.FifteenMinutes)
            {
                for (var i = st.Date; i <= et + TimeSpan.FromDays(1); i = i + TimeSpan.FromDays(1))
                {
                    //tl.Add(new DateTime(i.Year, i.Month, i.Day));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 9,45, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 10, 0, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 10, 15, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 10, 30, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 10, 45, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 11, 0, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 11, 15, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 11, 30, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 13, 15, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 13, 30, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 13, 45, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 14, 0, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 14, 15, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 14, 30, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 14, 45, 0));
                    tl.Add(new DateTime(i.Year, i.Month, i.Day, 15, 0, 0));
                }
            }
            return tl;
        }

        public static MarketDataGrade? GetGrade(List<IMarketData> dataList)
        {
            if (dataList == null || dataList.Count == 0) return null; 
            var maxgrade=dataList.Max(v => v.Grade);
            var mingrade = dataList.Min(v => v.Grade);
            if (maxgrade == mingrade)
                return maxgrade;
            return null;//means list contains more than one grade
        }

        public object Clone()
        {
            var md = new MarketData();
            md.InstrumentTicker = InstrumentTicker;
            md.High = High;
            md.Low = Low;
            md.Open = Open;
            md.Close = Close;
            md.Volume = Volume;
            md.Time = Time;
            md.CurrentCurrency = CurrentCurrency;

            return md;
        }
    }

}
