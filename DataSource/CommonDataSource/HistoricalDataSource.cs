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

namespace CommonDataSource
{
    public class HistoricalDataSource:DataSourceBase
    {
        TimeSpan _StepTime = TimeSpan.FromDays(1);
        public TimeSpan StepTime
        {
            get { return _StepTime; }
            set { _StepTime = value; }
        }

        int _DelayTimeMs = 7000;
        public int DelayTimeMs
        {
            get { return _DelayTimeMs; }
            set { _DelayTimeMs = value; }
        }

        private bool _isStart = false;
        private bool _isPause = false;
        public override void PrepareWork()
        {
            RefreshDataList();
            base.PrepareWork();
            _isStart = false;
            _isPause = false;
        }

        public override void Start()
        {
            base.Start();
            _isStart = true;
            if(!_isPause)//auto reset
                CurrentDataTime = StartTime;
            for (DateTime time = CurrentDataTime; time <= EndTime; time += StepTime)
            {
                var dl = GetPeriodList(time, time + StepTime);

                SendData(dl);
                //if(DelayTimeMs!=0)
                Thread.Sleep(DelayTimeMs);
                if (_isStart == false)
                    return;
            }
            
            _isPause = false;
            CurrentDataTime = EndTime;
            SendFinish();
        }
        public override void Pause()
        {
            _isStart = false;
        }
        //public override void Reset()
        //{
        //    _isPause = false;
        //    _currentTime = StartTime;
        //}
        public override void Stop()
        {
            _isStart = false;
        }

        List<IMarketData> GetPeriodList(DateTime start, DateTime end)
        {
            return DataList.Where(v => v.Time >= start && v.Time < end).ToList();

        }
        public override List<IMarketData> GetDataList(IInstrument instrument)
        {
            if (instrument == null || string.IsNullOrEmpty(instrument.Ticker))
                return null;
            var dl = new List<IMarketData>();
            try
            {
                string sqlconnection = "Data Source=DESKTOP-4V9J3TM\\SQLEXPRESS;Initial Catalog=MarketData;Integrated Security=True";
                var connection = new SqlConnection(sqlconnection);
                var cs = ConfigurationManager.ConnectionStrings["mysqlDB"].ConnectionString;
                var sql = "select * from marketminuteprice where Time >="+ StartTime.ToString()+"' and Time<='"+EndTime.ToString()+"' and Ticker='"+instrument.Ticker+"'";
                connection.Open();
                var t = connection.Query(sql);
                connection.Close();
                t.ToList().ForEach(v =>
                {
                    var md = new MarketData();
                    md.Time = v.Time;
                    //md.InstrumentName = instrument.Name;
                    md.InstrumentTicker = instrument.Ticker;

                    double p;
                    double.TryParse(v.Low, out p);
                    md.Low = p;

                    double.TryParse(v.Open, out p);
                    md.Open = p;

                    double.TryParse(v.High, out p);
                    md.High = p;

                    double.TryParse(v.Close, out p);
                    md.Close = p;

                    double.TryParse(v.Volume, out p);
                    md.Volume = p;

                    double.TryParse(v.Shares, out p);
                    md.Shares = p;

                    //md.CurrentCurrency = v.CRNCY;
                    dl.Add(md);
                });
                IsDataLoaded = true;
                return dl.OrderBy(v => v.Time).ToList(); ;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public override ICopyObject CreateInstance()
        {
            return new HistoricalDataSource();
        }



        public override string Name
        {
            get
            {
                return "Historical market data source";
            }

            set
            {
                
            }
        }

        public override string Memo
        {
            get
            {
                return "";
            }

            set
            {
                
            }
        }
    }
}
