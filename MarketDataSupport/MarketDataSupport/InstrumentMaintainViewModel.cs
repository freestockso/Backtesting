using BackTestingCommonLib;
using BackTestingInterface;
using CommonLibForWPF;
using Dapper;
using Microsoft.Win32;
using ReportCommonLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MarketDataSupport
{
    class InstrumentMaintainViewModel : ViewModelBase
    {
        ObservableCollection<IInstrument> _InstrumentList = new ObservableCollection<IInstrument>();
        public ObservableCollection<IInstrument> InstrumentList
        {
            get { return _InstrumentList; }
        }
        public IInstrument CurrentInstrument { get; set; }
        void ClearChangedFlag()
        {
            foreach (var v in InstrumentList)
            {
                var im = v as InstrumentModel;
                if (im != null)
                    im.IsChanged = false;
            }
        }
        List<string> _SeperateCharList = new List<string>() { "Comma(,)", "Tab" };
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
            var dlg = new OpenFileDialog() { Filter = "Text File|*.txt|Csv File|*.CSV| (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                var ep = new ReportCommonLib.ReportExportHelper();
                var dic = ep.LoadMappingFile(System.AppDomain.CurrentDomain.BaseDirectory + "MarketDataSupport\\InstrumentMapping.txt");
                var l=ep.LoadFromCsvFile<InstrumentModel>(dlg.FileName, Encoding.GetEncoding("GB18030") , GetSeperateChar(), dic , preProcess);
                InstrumentList.Clear();
                l.ForEach(v => InstrumentList.Add(v));
            }
            ClearChangedFlag();
        }
        void LoadFromDB()
        {
            var connectionStr = ConfigurationManager.ConnectionStrings["MarketDataConnectionString"];

            using (var conn = new SqlConnection(connectionStr.ConnectionString))
            {
                conn.Open();
                var rl = conn.Query<Instrument>("Select * from Table_Instruments", new { }, null, true, 60000).ToList();
                InstrumentList.Clear();
                foreach (var r in rl)
                {
                    r.Ticker = r.Ticker.Trim();
                    r.Currency = r.Currency.Trim();
                    r.Industory = r.Industory.Trim();
                    r.Name = r.Name.Trim();
                    r.Region = r.Region.Trim();
                    if (r.Memo != null)
                        r.Memo = r.Memo.Trim();

                    InstrumentList.Add(r);
                }
            }
            ClearChangedFlag();
        }

        void SaveToDB()
        {
            var dh = new DatabaseDataProvider();
            var connectionStr = ConfigurationManager.ConnectionStrings["MarketDataConnectionString"].ConnectionString;
            dh.GetDatabaseConnection = () => { return new SqlConnection(connectionStr); };

            dh.DeleteBatch(null, "Table_Instruments", new List<string>() {"Ticker" }, InstrumentList.Cast<object>().ToList());
            dh.InsertBatch(null, "Table_Instruments", InstrumentList.Cast<object>().ToList());
        }

        void SaveToFile()
        {
            var dlg = new SaveFileDialog() { Filter = "Text File|*.txt |Csv File| *.csv | (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                var ep = new ReportCommonLib.ReportExportHelper();
                ep.CreateCsvReport<IInstrument>(dlg.FileName, InstrumentList.ToList());

            }
        }

        void Clear() { InstrumentList.Clear(); }
        void New()
        {
            var instrument = new InstrumentModel();
            InstrumentList.Add(instrument);
        }
        void Delete()
        {
            if (CurrentInstrument != null && InstrumentList.Contains(CurrentInstrument))
                InstrumentList.Remove(CurrentInstrument);
        }

        public CommonCommand LoadFromDBCommand { get { return new CommonCommand((o) => { LoadFromDB(); }); } }
        public CommonCommand LoadFromFileCommand { get { return new CommonCommand((o) => { LoadFromFile(); }); } }
        public CommonCommand SaveToDBCommand { get { return new CommonCommand((o) => { SaveToDB(); }); } }
        public CommonCommand SaveToFileCommand { get { return new CommonCommand((o) => { SaveToFile(); }); } }
        public CommonCommand ClearCommand { get { return new CommonCommand((o) => { Clear(); }); } }
        public CommonCommand NewCommand { get { return new CommonCommand((o) => { New(); }); } }
        public CommonCommand DeleteCommand { get { return new CommonCommand((o) => { Delete(); }); } }
        public CommonCommand OpenCurrentInstrumentDataCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (CurrentInstrument==null||string.IsNullOrEmpty(CurrentInstrument.Ticker))
                        MessageBox.Show("Please select a instrument");
                    else {
                        var sql = "Select * from Table_TradePrice where Ticker='"+CurrentInstrument.Ticker+"'";

                        var connectionStr = ConfigurationManager.ConnectionStrings["MarketDataConnectionString"];

                        using (var conn = new SqlConnection(connectionStr.ConnectionString))
                        {
                            conn.Open();
                            var rl = conn.Query<BackTestingCommonLib.MarketData>(sql, new { }, null, true, 60000).ToList();
                            InstrumentControl.ShowInstrumentInfo(CurrentInstrument.Ticker, rl.Cast<IMarketData>().ToList());
                        }

                        
                    }
                });
            }
        }
    }
}
