using BackTestingCommonLib;
using BackTestingInterface;
using CommonLib;
using CommonLibForWPF;
using Dapper;
using GetMarketDataFromTDX;
using Microsoft.Win32;
using ReportCommonLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace MarketDataSupport
{
    class MainWindowViewModel:ViewModelBase
    {
        ObservableCollection<IMarketData> _MarketDataList = new ObservableCollection<IMarketData>();
        public ObservableCollection<IMarketData> MarketDataList
        {
            get { return _MarketDataList; }
        }
        public IMarketData CurrentMarketData { get; set; }

        public CommonCommand ClearCommand { get { return new CommonCommand((o) => { Clear(); }); } }
        public CommonCommand NewCommand { get { return new CommonCommand((o) => { New(); }); } }
        public CommonCommand DeleteCommand { get { return new CommonCommand((o) => { Delete(); }); } }
        public CommonCommand SaveToFileCommand { get { return new CommonCommand((o) => { SaveToFile(); }); } }
        public CommonCommand LoadFromFileCommand { get { return new CommonCommand((o) => { LoadFromFile(); }); } }
        public CommonCommand LoadFromDBCommand { get { return new CommonCommand((o) => { LoadFromDB(); }); } }
        public CommonCommand SaveToDBCommand { get { return new CommonCommand((o) => { SaveToDB(); }); } }
        public CommonCommand InstrumentMaintainCommand { get
            {
                return new CommonCommand((o) =>
                {
                    var w = new InstrumentMaintain();
                    w.Show();
                });
            }
        }

        List<string> _SeperateCharList = new List<string>() { "Comma(,)","Tab"};
        public List<string> SeperateCharList
        {
            get { return _SeperateCharList; }
        }
        string _CurrentSeperateChar = "Tab";
        public string CurrentSeperateChar { get { return _CurrentSeperateChar; } set { _CurrentSeperateChar = value; OnPropertyChanged("CurrentSeperateChar"); } }

        char GetSeperateChar()
        {
            if (string.IsNullOrEmpty(CurrentSeperateChar))
                CurrentSeperateChar = "Tab";
            if (CurrentSeperateChar == "Tab")
                return '\t';
            return ',';
        }

        public CommonCommand OpenCurrentInstrumentDataCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (String.IsNullOrEmpty(InstrumentTicker))
                        MessageBox.Show("Please input valid ticker");
                    else {
                        var l = MarketDataList.Where(v => v.InstrumentTicker == InstrumentTicker).ToList();
                        InstrumentControl.ShowInstrumentInfo(InstrumentTicker, l);
                    }
                });
            }
        }
        void Clear() { MarketDataList.Clear(); }
        void New()
        {
            var obj = new BackTestingCommonLib.MarketData();
            MarketDataList.Add(obj);
        }
        void Delete()
        {
            if (CurrentMarketData != null && MarketDataList.Contains(CurrentMarketData))
                MarketDataList.Remove(CurrentMarketData);
        }

        void SaveToFile()
        {
            var dlg = new SaveFileDialog() { Filter = "Text File|*.txt |Csv File| *.csv | (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                var ep = new ReportCommonLib.ReportExportHelper();
                ep.CreateCsvReport<IMarketData>(dlg.FileName, MarketDataList.ToList());

            }
        }
        string preProcess(string value)
        {
            value = value.Trim();

            if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^\d+(\.\d+)?亿$"))
            {
                var t = Convert.ToDouble(value.Substring(0, value.Length - 1));
                return (t * 100000000).ToString();
            }
            if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^\d+(\.\d+)?万$"))
            {
                var t = Convert.ToDouble(value.Substring(0, value.Length - 1));
                return (t * 10000).ToString();
            }
            return value;
        }
        void LoadFromFile()
        {
            var dlg = new OpenFileDialog() { Filter = "Text File|*.txt |Csv File| *.csv | (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                var ep = new ReportCommonLib.ReportExportHelper();
                var l = ep.LoadFromCsvFile<BackTestingCommonLib.MarketData>(dlg.FileName, Encoding.GetEncoding("GB18030"), GetSeperateChar(),null, preProcess);
                l.ForEach(v => MarketDataList.Add(v));
            }
        }

        DateTime _StartTime = DateTime.Now - TimeSpan.FromDays(90);
        public DateTime StartTime { get { return _StartTime; } set { _StartTime = value; OnPropertyChanged("StartTime"); } }
        DateTime _EndTime = DateTime.Now;
        public DateTime EndTime { get { return _EndTime; } set { _EndTime = value; OnPropertyChanged("EndTime"); } }
        public string InstrumentTicker { get; set; }

        void LoadFromDB()
        {
            var sql = "Select * from Table_TradePrice where Time>='"+StartTime.ToString()+"' and Time<='"+EndTime.ToString()+"'";
            if (!string.IsNullOrEmpty(InstrumentTicker))
                sql = sql + " and Ticker='"+InstrumentTicker+"'";
            var connectionStr = ConfigurationManager.ConnectionStrings["MarketDataConnectionString"];

            using (var conn = new SqlConnection(connectionStr.ConnectionString))
            {
                conn.Open();
                var rl = conn.Query<BackTestingCommonLib.MarketData>(sql, new { }, null, true, 60000).ToList();
                foreach (var r in rl)
                {

                    MarketDataList.Add(r);
                }
            }
        }

        void SaveToDB()
        {
            var dh = new DatabaseDataProvider();
            var connectionStr = ConfigurationManager.ConnectionStrings["MarketDataConnectionString"].ConnectionString;
            dh.GetDatabaseConnection = () => { return new SqlConnection(connectionStr); };

            dh.DeleteBatch(null, "Table_TradePrice", new List<string>() { "Ticker,Time" }, MarketDataList.Cast<object>().ToList());
            dh.InsertBatch(null, "Table_TradePrice", MarketDataList.Cast<object>().ToList());
        }

        bool _isBusy = false;
        public bool IsBusy
        {
            get { return _isBusy; }
            set { _isBusy = value; OnPropertyChanged("IsBusy"); }
        }
        int totalCount = 0;
        int currentCount = 0;
        int successCount = 0;
        int faildCount = 0;
        double finishPercent = 0;
        TimeSpan spendTime;
        TimeSpan remainTime;

        public string BusyContent
        {
            get
            {
                return
                    "Filling data to database, , finish percent:" + finishPercent.ToString("p")+
                    " ("+ currentCount.ToString()+"/"+totalCount.ToString()+" ), success "+successCount.ToString()+",faild "+faildCount.ToString()+
                    ", time spend: "+spendTime.ToString()+", still need: "+remainTime.ToString();
            }
        }
        void ShowAutoFillLog()
        {
            if (File.Exists(MarketDataAutoFill.logFilePath))
                System.Diagnostics.Process.Start("notepad.exe", MarketDataAutoFill.logFilePath);
            else
                MessageBox.Show(MarketDataAutoFill.logFilePath + " not exist");
        }
        public CommonCommand ShowAutoFillLogCommand
        {
            get { return new CommonCommand((o) => { ShowAutoFillLog(); }); }
        }
        void RefreshProcess(int total,int current,double percent,int success,int faild,TimeSpan spend,TimeSpan remain)
        {
            totalCount = total;
            currentCount = current;
            finishPercent = percent;
            successCount = success;
            faildCount = faild;
            spendTime = spend;
            remainTime = remain;
            OnPropertyChanged("BusyContent");

        }

        bool _IsDataAppend = false;
        public bool IsDataAppend
        {
            get
            {
                return _IsDataAppend;
            }
            set
            {
                _IsDataAppend = value;OnPropertyChanged("IsDataAppend");
            }
        }

        public CommonCommand DataAutoFillCommand
        {
            get
            {
                return new CommonCommand((o) => {
                    IsBusy = true;
                    //TotalCount = 0;
                    //CurrentCount = 0;
                    try {
                        var autoFill = new MarketDataAutoFill();
                        autoFill.ProcessorCallBack =RefreshProcess;
                        Task.Factory.StartNew(() => { autoFill.DataAutoFill(IsDataAppend); }).ContinueWith((t) => {
                            
                            IsBusy = false;
                            if (File.Exists(MarketDataAutoFill. logFilePath))
                                System.Diagnostics.Process.Start("notepad.exe", MarketDataAutoFill.logFilePath);
                        });
                    }
                    catch ( Exception ex)
                    {
                        LogSupport.Error(ex);
                        IsBusy = false;
                    }
                    finally { }
                });
            }
        }
    }
}
