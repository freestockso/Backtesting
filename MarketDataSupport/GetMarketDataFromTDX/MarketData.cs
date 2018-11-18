using ReportCommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetMarketDataFromTDX
{
    public class MarketData
    {
        public MarketData(Byte[] data)
        {
            ReadInfo(data);
        }
        [ReportOutputIgnore]
        public string UID
        {
            get
            {
                if (Time == new DateTime(2000, 1, 1) || Ticker==null) return null;
                return Time.ToString("yyyymmddhhMM") + Ticker;
            }
        }
        public string Ticker { get; set; }
        [ReportOutputIgnore]
        public int Year { get; set; }
        [ReportOutputIgnore]
        public int Month { get; set; }
        [ReportOutputIgnore]
        public int Day { get; set; }
        [ReportOutputIgnore]
        public int Hour { get; set; }
        [ReportOutputIgnore]
        public int Minute { get; set; }

        public float Open { get; set; }
        public float Close { get; set; }
        public float High { get; set; }
        public float Low { get; set; }
        public float Volume { get; set; }
        public ulong Shares { get; set; }

        [ReportOutputIgnore]
        public string KeyWords { get; set; }

        [ReportOutputIgnore]
        public float BackData { get; set; }

        DateTime _Time = new DateTime(2000,1,1);//default date means no valid value
        public DateTime Time
        {
            get
            {
                if (Year == 0 || Month == 0 || Day == 0) return _Time;
                if (Hour < 9 || Hour > 15) return _Time;
                if (Minute < 0 || Minute > 60) return _Time;
                return _Time= new DateTime(Year,Month,Day,Hour,Minute,0);


            }
            set
            {
                _Time = value;
                Year = value.Year;
                Month = value.Month;
                Day = value.Day;
                Hour = value.Hour;
                Minute = value.Minute;
                
            }
        }

        public void ReadInfo(Byte[] data)
        {
            if (data.Length != 32) return;
            var dh = data[1];
            var dl = data[0];
            ushort uRev = (ushort)((dh << 8) + dl);
            var d = Convert.ToInt16(uRev);
            Year = ((d / 2048) + 2004);
            Month = ((d % 2048) / 100);
            Day = ((d % 2048) % 100);

            var th = data[3];
            var tl = data[2];
            ushort tRev = (ushort)((th << 8) + tl);
            var t = Convert.ToInt16(tRev);
            Hour = t / 60;
            Minute = t % 60;

            var bt = new byte[4];
            bt[0] = data[4];
            bt[1] = data[5];
            bt[2] = data[6];
            bt[3] = data[7];
            Open = BitConverter.ToSingle(bt, 0);

            bt[0] = data[8];
            bt[1] = data[9];
            bt[2] = data[10];
            bt[3] = data[11];
            High = BitConverter.ToSingle(bt, 0);

            bt[0] = data[12];
            bt[1] = data[13];
            bt[2] = data[14];
            bt[3] = data[15];
            Low = BitConverter.ToSingle(bt, 0);

            bt[0] = data[16];
            bt[1] = data[17];
            bt[2] = data[18];
            bt[3] = data[19];
            Close = BitConverter.ToSingle(bt, 0);

            bt[0] = data[20];
            bt[1] = data[21];
            bt[2] = data[22];
            bt[3] = data[23];
            Volume = BitConverter.ToSingle(bt, 0);

            bt[0] = data[24];
            bt[1] = data[25];
            bt[2] = data[26];
            bt[3] = data[27];
            Shares = BitConverter.ToUInt32(bt, 0);

            bt[0] = data[28];
            bt[1] = data[29];
            bt[2] = data[30];
            bt[3] = data[31];
            BackData = BitConverter.ToSingle(bt, 0);

            if (Open > High || Close > High||Open<Low||Close<Low||Low>High) throw new Exception("Error read byte");
        }
    }
}
