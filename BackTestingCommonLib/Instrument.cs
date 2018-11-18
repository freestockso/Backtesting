using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingInterface;
using System.Configuration;
using System.Data.SqlClient;
using Dapper;
using CommonLib;

namespace BackTestingCommonLib
{
    public enum DistributeAnalyseType
    {
        Industory, Region,Instrument
    }
    public class Instrument : IInstrument
    {
        public static IInstrument GetNewInstrument(string name, string ticker )
        {
            var inst = new Instrument {Name = name};
            if (!string.IsNullOrEmpty(ticker)) inst.Ticker = ticker;
            return inst;
        }
        static List<IInstrument> _AllInstrumentList = new List<IInstrument>();
        public static List<IInstrument> AllInstrumentList
        {
            get {
                if (_AllInstrumentList.Count == 0) RefreshAllInstrument();
                return _AllInstrumentList;
            }
        }
        static List<string> _AllRegionList = new List<string>();
        public static List<string> AllRegionList
        {
            get
            {
                if (_AllRegionList.Count == 0)
                {
                    _AllRegionList.AddRange(AllInstrumentList.Select(v => v.Region).Distinct());
                }
                return _AllRegionList;
            }
        }
        static List<string> _AllIndustoryList = new List<string>();
        public static List<string> AllIndustoryList
        {
            get
            {
                if (_AllIndustoryList.Count == 0)
                {
                    _AllIndustoryList.AddRange(AllInstrumentList.Select(v => v.Industory).Distinct());
                }
                return _AllIndustoryList;
            }
        }
        public static List<IInstrument> GetInstrumentBySector(string name, DistributeAnalyseType secType = DistributeAnalyseType.Industory)
        {
            if (secType == DistributeAnalyseType.Industory)
                return AllInstrumentList.Where(v => v.Industory == name).ToList();
            if (secType == DistributeAnalyseType.Region)
                return AllInstrumentList.Where(v => v.Region == name).ToList();
            if (secType == DistributeAnalyseType.Instrument)
                return AllInstrumentList.Where(v => v.Name == name||v.Ticker==name).ToList();
            return null;
        }
        public static string GetInstrumentSectorName(string ticker, DistributeAnalyseType secType = DistributeAnalyseType.Industory)
        {
            var inst = AllInstrumentList.FirstOrDefault(i => i.Ticker == ticker);
            if(inst!=null)
            {
                if (secType == DistributeAnalyseType.Industory)
                    return inst.Industory;
                if (secType == DistributeAnalyseType.Region)
                    return inst.Region;
                if (secType == DistributeAnalyseType.Instrument)
                    return inst.Name;
            }
            return null;
        }

        public static void RefreshAllInstrument()
        {
            var connectionStr = ConfigurationManager.ConnectionStrings["MarketDataConnectionString"];
            try
            {
                using (var conn = new SqlConnection(connectionStr.ConnectionString))
                {

                    conn.Open();
                    var rl = conn.Query<Instrument>("Select * from Table_Instruments", new { }, null, true, 60000).ToList();
                    foreach (var r in rl)
                    {
                        r.Ticker = r.Ticker.Trim();
                        r.Currency = r.Currency.Trim();
                        r.Industory = r.Industory.Trim();
                        r.Name = r.Name.Trim();
                        r.Region = r.Region.Trim();
                        if (r.Memo != null)
                            r.Memo = r.Memo.Trim();

                        _AllInstrumentList.Add(r);
                    }
                }
            }catch(Exception ex)
            {
                LogSupport.Error(ex);
                
            }
        }
        public string Ticker { get; set; }

        public void LoadData(object obj)
        {
            if (obj == null) return;

            var inst = obj as IInstrument;
            if (inst == null) return;
            Name = inst.Name;
            Memo = inst.Memo;
            Ticker = inst.Ticker;
            Currency = inst.Currency;
            CurrentPrice = inst.CurrentPrice;
            Industory = inst.Industory;
            Margin = inst.Margin;
            MarketValue = inst.MarketValue;
            OrderFixedCost = inst.OrderFixedCost;
            OrderPercentCost = inst.OrderPercentCost;
            Region = inst.Region;
            PE = inst.PE;
            PB = inst.PB;
        }


        string _Currency = "RMB";
        public string Currency{ get { return _Currency; } set { _Currency = value; } }

        public string Name { get; set; }
        public string Memo { get; set; }
        public string PYName
        {
            get
            {
                if (string.IsNullOrEmpty(Name))
                    return "";
                return CommonLib.CommonProc.GetSpellCode(Name);
            }
        }
        public double CurrentPrice { get; set; }

        public double Margin { get; set; }

        public double OrderFixedCost { get; set; }

        public double OrderPercentCost { get; set; }

        public double PE
        {
            get;set;
        }

        public double PB
        {
            get; set;
        }

        public double MarketValue
        {
            get; set;
        }

        public string Industory
        {
            get; set;
        }

        public string Region
        {
            get; set;
        }

        public object GetData()
        {
            return Clone();
        }

        public object Clone()
        {
            var inst = new Instrument();
            inst.Currency = Currency;
            inst.CurrentPrice = CurrentPrice;

            inst.Ticker = Ticker;
            inst.Margin = Margin;
            inst.Name = Name;
            inst.OrderFixedCost = OrderFixedCost;
            inst.OrderPercentCost = OrderPercentCost;
            inst.Industory = Industory;
            inst.MarketValue = MarketValue;
            inst.Memo = Memo;
            inst.PB = PB;
            inst.PE = PE;
            inst.Region = Region;
            
            return inst;
        }
        public static string CorrectTicker(string ticker)
        {
            var s = "000000" + ticker;
            return s.Substring(s.Length - 6);
        }


    }
}
