using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportCommonLib
{
    public interface IFundStrategyFilterSupport
    {
        string Fund { get; }
        string Strategy { get; }
        string Folder { get; }
    }
    public class FundStrategyFolderFilter
    {
        public string Fund { get; set; }
        public string Strategy { get; set; }
        public string Folder { get; set; }
        public string FilterString
        {
            get
            {
                if (string.IsNullOrEmpty(Fund)) return "";
                var s = Fund;
                if (string.IsNullOrEmpty(Strategy)) return s;
                s += ("." + Strategy);
                if (string.IsNullOrEmpty(Folder)) return s;
                return s += ("." + Folder);
            }
        }

        public static List<FundStrategyFolderFilter> GetFilterList(string key)
        {
            if (ConfigurationManager.AppSettings[key] == null)
                return new List<FundStrategyFolderFilter>();
            var rl = new List<FundStrategyFolderFilter>();

            var fl = ConfigurationManager.AppSettings[key].ToString().Split(';');
            foreach (var f in fl)
            {
                var dl = f.Split('.');
                if (!string.IsNullOrEmpty(dl[0].Trim()))
                {
                    var filter = new FundStrategyFolderFilter() { Fund = dl[0].Trim() };
                    if (dl.Length > 1)
                        filter.Strategy = dl[1].Trim();
                    if (dl.Length > 2)
                        filter.Folder = dl[2].Trim();
                    rl.Add(filter);
                }
            }
            return rl;
        }

        public static List<T> FilterResult<T>(List<T> sourceList, List<FundStrategyFolderFilter> filterList) where T : IFundStrategyFilterSupport
        {
            if (filterList == null) return sourceList;
            if (filterList.Count == 0 || (filterList.Any(f =>f.Fund.Equals("All", StringComparison.CurrentCultureIgnoreCase)))) return sourceList;
            var rl = new List<T>();
            sourceList.ForEach(v =>
            {
                if (filterList.Any(f =>
                    f.Fund.Equals(v.Fund, StringComparison.CurrentCultureIgnoreCase) 
                    &&(string.IsNullOrEmpty(f.Strategy)|| f.Strategy.Equals(v.Strategy, StringComparison.CurrentCultureIgnoreCase))
                    && (string.IsNullOrEmpty(f.Folder) || f.Folder.Equals(v.Folder, StringComparison.CurrentCultureIgnoreCase))))
                    rl.Add(v);

            });

            return rl;
        }

        public static string GetFilterQueryString(List<FundStrategyFolderFilter> filterList)
        {
            if (filterList.Count == 0) return "";
            var Fl = filterList.Where(v =>!string.IsNullOrEmpty(v.Fund)&& string.IsNullOrEmpty(v.Strategy) && string.IsNullOrEmpty(v.Folder)).ToList();
            var sl = filterList.Where(v => !string.IsNullOrEmpty(v.Fund) && !string.IsNullOrEmpty(v.Strategy) && string.IsNullOrEmpty(v.Folder)).ToList();
            var fl = filterList.Where(v => !string.IsNullOrEmpty(v.Fund) && !string.IsNullOrEmpty(v.Strategy) && !string.IsNullOrEmpty(v.Folder)).ToList();

            string Fs="",ss="",fs = "";
            if (Fl.Count > 0)
            {
                var s = "(";
                Fl.ForEach(v =>
                {
                    s = s + "'" + v.Fund + "',";
                });
                s = s.Substring(0, s.Length - 1) + ")";
                Fs = " fund in " + s;
            }
            if (sl.Count > 0)
            {
                var s = "(";
                sl.ForEach(v =>
                {
                    s = s + "'" + v.Fund +"/"+v.Strategy+ "',";
                });
                s = s.Substring(0, s.Length - 1) + ")";
                ss = " fund+'/'+strategy in " + s;
            }
            if (fl.Count > 0)
            {
                var s = "(";
                fl.ForEach(v =>
                {
                    s = s + "'" + v.Fund + "/" + v.Strategy +"/"+v.Folder + "',";
                });
                s = s.Substring(0, s.Length - 1) + ")";
                fs = " fund+'/'+strategy +'/'+folder in " + s;
            }

            var queryStr="";
            if (!string.IsNullOrEmpty(Fs))
                queryStr = Fs;
            if (!string.IsNullOrEmpty(ss))
            {
                if (!string.IsNullOrEmpty(queryStr))
                    queryStr += " OR ";
                queryStr += ss;
            }
            if (!string.IsNullOrEmpty(fs))
            {
                if (!string.IsNullOrEmpty(queryStr))
                    queryStr += " OR ";
                queryStr += fs;
            }
            if (!string.IsNullOrEmpty(queryStr))
                queryStr = "(" + queryStr + ")";
            return queryStr;
        }

        
    }
}
