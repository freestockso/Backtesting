using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReportCommonLib
{
    public class ReportParameterHelper
    {
        Dictionary<string, string> _consoleLineParameterList = new Dictionary<string, string>();
        public Dictionary<string, string> ConsoleLineParameterList { get { return _consoleLineParameterList; } }
        public ReportParameterHelper(string[] args)
        {
            _consoleLineParameterList = GetParameterDictionary(args);
        }
        public string GetParameterValue(string name,string defaultValue)
        {
            if (_consoleLineParameterList.ContainsKey(name))//console input is proity
                return _consoleLineParameterList[name];
            var v = GetConfigurationValue(name);
            if (!string.IsNullOrEmpty(v))
                return v;
            return defaultValue;
        }

        public Dictionary<string, string> GetParameterDictionary(string[] args)
        {//every parameter starts with / and use : seperate name and value. like /targetTime:2014-5-23 /reportFormat:pdf /mailTo:yinxiao.liu@prcm.com
            var dl = new Dictionary<string, string>();
            var pl = new List<string>();
            foreach (var s in args)
            {
                var ts = s.Trim();
                if (ts.StartsWith("/"))
                {
                    pl.Add(ts);
                }
                else
                {
                    if (pl.Count > 0)
                        pl[pl.Count - 1] += (" " + ts);
                }
            }
            pl.ForEach(v =>
            {
                if (v.Contains(":"))
                {
                    var ns = v.Substring(1, v.IndexOf(":", StringComparison.Ordinal) - 1).Trim();
                    var vs = v.Substring(v.IndexOf(":", StringComparison.Ordinal) + 1).Trim();
                    dl.Add(ns, vs);
                }
                else
                {
                    dl.Add(v.Substring(1), null);
                }
            });
            return dl;
        }
        public static string GetConfigurationValue(string strKey)
        {
            foreach (string key in ConfigurationManager.AppSettings)
            {
                if (key == strKey)
                {
                    return ConfigurationManager.AppSettings[strKey];
                }
            }
            return null;
        }
    }
}
