using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BackTestingCommonLib;
using BackTestingInterface;
using System.Configuration;
using Dapper;
using System.Data.SqlClient;
using CommonLib;

namespace CommonDataSource
{
    [SerialObjectAttribute(Key = "HistoricalDataSource", Name = "Historical DataSource", Memo = "Get all market data from database")]
    public class HistoricalDataSource:DataSourceBase
    {
        int _TimeoutMs = 3000;
        public int TimeoutMs
        {
            get
            {
                if (ConfigurationManager.AppSettings["TimeoutMs"] != null)
                {
                    var str = ConfigurationManager.AppSettings["TimeoutMs"].ToString();
                    if (string.IsNullOrEmpty(str))
                        return _TimeoutMs;
                    try
                    {
                        var ts = Convert.ToInt32(str);
                        _TimeoutMs = ts;
                    }
                    catch
                    { }
                }
                return _TimeoutMs;

            }
            set { _TimeoutMs = value; }
        }

        List<IMarketData> GetPeriodList(DateTime start, DateTime end)
        {
            return DataList.Where(v => v.Time >= start && v.Time < end).ToList();

        }
        public override IMarketData GetFirstData(IInstrument instrument, DateTime startTime, DateTime endTime, MarketDataGrade grade)
        {
            if (instrument == null || string.IsNullOrEmpty(instrument.Ticker))
                return null;
            if (grade == MarketDataGrade.FiveMinutes)
            {
                try
                {
                    string sqlconnection = "Data Source=LYNX-NOTEBOOK\\SQLEXPRESS;Initial Catalog=MarketData;Integrated Security=True";
                    var connection = new SqlConnection(sqlconnection);
                    var cs = ConfigurationManager.ConnectionStrings["MarketDataConnectionString"].ConnectionString;
                    var sql = "select top 1 * from Table_TradePrice where Time >'" + startTime.ToString() + "' and Time<='" + endTime.ToString() + "' and Ticker='" + instrument.Ticker + "' order by Time";
                    connection.Open();
                    var t = connection.Query(sql);
                    connection.Close();
                    var v = t.FirstOrDefault();
                    if (v != null)
                    {
                        var md = new MarketData();
                        md.Time = v.Time;
                        md.InstrumentTicker = instrument.Ticker;

                        md.Low = v.Low;
                        md.Open = v.Open;
                        md.High = v.High;
                        md.Close = v.Close;
                        md.Volume = v.Volume;
                        md.Shares = v.Shares;
                        md.Grade = MarketDataGrade.FiveMinutes;
                        return md;
                    }
                    return null;
                }
                catch (Exception e)
                {
                    LogSupport.Error(e);
                    throw e;
                }
            }
            return base.GetFirstData(instrument, startTime, endTime, grade);
        }
        public override List<IMarketData> GetSourceDataList(IInstrument instrument, DateTime startTime, DateTime endTime)
        {
            if (instrument == null || string.IsNullOrEmpty(instrument.Ticker))
                return null;
            var dl = new List<IMarketData>();
            try
            {
                string sqlconnection = "Data Source=LYNX-NOTEBOOK\\SQLEXPRESS;Initial Catalog=MarketData;Integrated Security=True";
                var connection = new SqlConnection(sqlconnection);
                var cs = ConfigurationManager.ConnectionStrings["MarketDataConnectionString"].ConnectionString;
                var sql = "select * from Table_TradePrice where Time >='"+ startTime.ToString()+"' and Time<='"+endTime.ToString()+"' and Ticker='"+instrument.Ticker+"'";
                connection.Open();
                var t = connection.Query(sql,new { },null,true,TimeoutMs);
                connection.Close();
                t.ToList().ForEach(v =>
                {
                    var md = new MarketData();
                    md.Time = v.Time;
                    md.InstrumentTicker = instrument.Ticker;

                    md.Low = v.Low;
                    md.Open = v.Open;
                    md.High = v.High;
                    md.Close = v.Close;
                    md.Volume = v.Volume;
                    md.Shares = v.Shares;
                    md.Grade = MarketDataGrade.FiveMinutes;
                    dl.Add(md);
                });
                return dl.OrderBy(v => v.Time).ToList(); ;
            }
            catch (Exception e)
            {
                LogSupport.Error(e);
                throw e;
            }
        }

        public override object Clone()
        {
            var ds = base.Clone() as HistoricalDataSource;

            ds.TimeoutMs = TimeoutMs;
            return ds;
        }

        public override IDataSource CreateInstance()
        {
            return new HistoricalDataSource();
        }
    }
}
