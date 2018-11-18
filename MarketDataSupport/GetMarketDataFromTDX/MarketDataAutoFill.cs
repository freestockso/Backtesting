using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetMarketDataFromTDX
{
    public class MarketDataAutoFill
    {
        public static string logFilePath = AppDomain.CurrentDomain.BaseDirectory + "AutoFillInfo.txt";
        //total,current,finished percent,success,faild,spend,remain
        public Action<int,int,double,int,int,TimeSpan,TimeSpan> ProcessorCallBack { get; set; }

        public void DataAutoFill(bool isAppend=false)
        {
            autoFillStartTime = DateTime.Now;
            AutoFillInfo.Clear();
            var basedir = ConfigurationManager.AppSettings["TDX export base folder"];
            var cs = ConfigurationManager.ConnectionStrings["MarketDataConnectionString"].ConnectionString;
            var trans = new TransMarketDataFromTDX();
            DateTime? InsertBeginTime= null;
            if(isAppend)
                InsertBeginTime = trans.GetLastTime(cs);
            Result = "Auto fill data job start at " + autoFillStartTime.ToString() + ", base dir is " + basedir + ", connection string is " + cs;
            AutoFillInfo.Add(Result);
            var l = GetAllInstrumentTicker();
            if (l.Count > 0)
                AutoFillInfo.Add("Total count is " + l.Count.ToString());
            TotalCount = l.Count;
            successCount = 0;
            faildFileList.Clear();
            for (int i = 0; i < TotalCount; i++)
            {
                CurrentCount = i + 1;
                AutoFillInfo.Add("Current count is " + CurrentCount.ToString());
                var fs = basedir + GetFileName(l[i]);//only add exist instrument market data
                if (File.Exists(fs))
                {
                    AutoFillInfo.Add("Current file is " + fs);
                    trans.MarketDataList.Clear();
                    trans.LimitBeginTime = InsertBeginTime;
                    try
                    {
                        trans.ReadData(fs);
                        AutoFillInfo.Add(fs + " data read success");
                        trans.WriteToDB(cs);
                        AutoFillInfo.Add(fs + " data fill success");
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        AutoFillInfo.Add("Error: " + fs + " data fill error, message is " + ex.Message);
                        faildFileList.Add("Error: " + fs + " data fill error, message is " + ex.Message);
                    }
                }
                else {
                    AutoFillInfo.Add("Error: file " + fs + " not exist!");
                    faildFileList.Add("Error: file " + fs + " not exist!");
                }
                if(ProcessorCallBack!=null)
                {
                    var spendtime = DateTime.Now-autoFillStartTime;
                    var remain = TimeSpan.FromSeconds((spendtime.TotalSeconds / FinishPercent) - spendtime.TotalSeconds);

                    ProcessorCallBack(TotalCount, CurrentCount, FinishPercent, successCount, faildFileList.Count, spendtime, remain);
                }
            }
            DateTime finishTime = DateTime.Now;
            Result = "job finished at " + finishTime.ToString() + ", spend time:" + (finishTime - autoFillStartTime).ToString() + " , total success " + successCount.ToString();
            AutoFillInfo.Add(Result);
            if (faildFileList.Count > 0)
            {
                AutoFillInfo.Add("Faild count " + faildFileList.Count.ToString());
                faildFileList.ForEach(v => AutoFillInfo.Add(v + ";"));
            }
            if (File.Exists(logFilePath))
                File.Delete(logFilePath);
            File.WriteAllLines(logFilePath, AutoFillInfo.ToArray());
        }
        public string Result { get; set; }
        List<string> GetAllInstrumentTicker()
        {
            var connectionStr = ConfigurationManager.ConnectionStrings["MarketDataConnectionString"];

            using (var conn = new SqlConnection(connectionStr.ConnectionString))
            {
                conn.Open();
                var rl = conn.Query<string>("Select Ticker from Table_Instruments", new { }, null, true, 60000).ToList();
                return rl;
            }
        }
        string GetFileName(string ticker)
        {
            if (ticker.Trim().StartsWith("0")) return "sz\\fzline\\sz" + ticker.Trim() + ".lc5";
            if (ticker.Trim().StartsWith("3")) return "sz\\fzline\\sz" + ticker.Trim() + ".lc5";
            if (ticker.Trim().StartsWith("6")) return "sh\\fzline\\sh" + ticker.Trim() + ".lc5";
            return null;
        }
        DateTime autoFillStartTime;
        int successCount = 0;
        List<string> faildFileList = new List<string>();
        List<string> _AutoFillInfo = new List<string>();
        public List<string> AutoFillInfo { get { return _AutoFillInfo; } }
        int _TotalCount = 0;
        public int TotalCount
        {
            get { return _TotalCount; }
            set { _TotalCount = value; }
        }

        int _CurrentCount = 0;
        public int CurrentCount
        {
            get { return _CurrentCount; }
            set { _CurrentCount = value;}
        }

        public double FinishPercent
        {
            get { if (TotalCount == 0) return 0; else return CurrentCount / Convert.ToDouble(TotalCount); }
        }
    }


}
