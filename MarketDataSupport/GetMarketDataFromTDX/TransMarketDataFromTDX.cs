using CommonLib;
using ReportCommonLib;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GetMarketDataFromTDX
{
    public class TransMarketDataFromTDX
    {
        List<MarketData> _dataList = new List<MarketData>();
        public List<MarketData> MarketDataList { get { return _dataList; } }
        public DateTime? LimitBeginTime { get; set; }

        public void TransToDB(string dirPath,string connectionStr)
        {
            if (!string.IsNullOrEmpty(dirPath) && Directory.Exists(dirPath))
            {
                var fileList = Directory.EnumerateFiles(dirPath);
                var count = fileList.Count();
                var i = 0;
                foreach (var f in fileList)
                {
                    var d = DateTime.Now;
                    if (f.EndsWith(".lc5"))
                    {
                        MarketDataList.Clear();
                        ReadData(f);
                        WriteToDB(connectionStr);
                    }
                    i++;
                    Console.WriteLine("Finish "+(i / Convert.ToDouble(count) * 100d).ToString() + "%,"+(DateTime.Now-d).TotalSeconds.ToString()+" s" );
                }
            }
        }
        public void TransToCSV(string dirPath)
        {

            if (!string.IsNullOrEmpty(dirPath)&&Directory.Exists(dirPath))
            {
                var fileList = Directory.EnumerateFiles(dirPath);
                foreach (var f in fileList)
                {
                    if (f.EndsWith("lc5"))
                    {
                        MarketDataList.Clear();
                        ReadData(f);
                        WriteData(f + ".csv");
                    }
                }
            }
        }
        public void ReadData(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }
            var fileManager = new DataFile();
            var fileName = fileManager.GetFileName(url);
            fileName = fileName.Substring(0, fileName.Length - 4);
            using (var fs = new FileStream(url, FileMode.Open, FileAccess.Read))
            {
                BinaryReader r = new BinaryReader(fs);

                var xd = new byte[32];
                for (long i = 0; i < fs.Length; i += 32)
                {
                    for (int j = 0; j < 32; j++)
                    {
                        var x = r.ReadByte();
                        xd[j] = Convert.ToByte(x);
                    }
                    var rec = new MarketData(xd);
                    rec.Ticker = fileName;
                    var l = rec.Ticker.Length;
                    if(l>6)
                        rec.Ticker = rec.Ticker.Substring(l-6);

                    MarketDataList.Add(rec);
                }


            }
        }
        public void WriteData(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return;
            }
            var s = GetCVS<MarketData>(MarketDataList);
            File.WriteAllText(url, s);
        }

        public string GetCVS<T>(List<T> objList)
        {
            if (objList == null) return "";
            string s = "";
            var pl = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var p in pl)
            {
                s += p.Name + ",";
            }
            s += "\r\n";
            objList.ForEach(v =>
            {
                foreach (var p in pl)
                {
                    var value = v.GetType().GetProperty(p.Name).GetValue(v);
                    if (value != null)
                        s += value.ToString() + ",";
                    else
                    {
                        s += ",";

                    }
                }
                s += "\r\n";
            });
            return s;
        }
        public DbConnection CreateConnection(string connectionStr)
        {
            //string sqlconnection = "server = 127.0.0.1; database = marketanalysedatabase; uid = root; pwd = root";
            //return new MySql.Data.MySqlClient.MySqlConnection( connectionStr);
            return new SqlConnection(connectionStr);
        }

        public void WriteToDB(string connectionStr)
        {
            var dh = new DatabaseDataProvider();
            dh.GetDatabaseConnection = ()=>CreateConnection(connectionStr);
            //dh.DeleteBatch(null, "marketminuteprice", "UID", MarketDataList.Cast<object>().ToList());
            //dh.InsertBatch(null, "marketminuteprice", MarketDataList.Cast<object>().ToList());
            var l = MarketDataList.Cast<object>().ToList();
            if (LimitBeginTime != null)
                l = MarketDataList.Where(v => v.Time > LimitBeginTime).Cast<object>().ToList();
            dh.DeleteBatch(null, "Table_TradePrice", new List<string>() { "Time","Ticker"},l );
            dh.InsertBatch(null, "Table_TradePrice", l);
        }

        public DateTime? GetLastTime(string conStr)
        {
            var sql = "SELECT max(Time) FROM Table_TradePrice ";
            var dh = new DatabaseDataProvider();
            var t = dh.GetQueryResult<DateTime>(sql, conStr);
            if (t != null && t.Count == 1)
                return t[0];
            return null;
        }
    }
}
