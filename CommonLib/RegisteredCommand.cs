using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib
{
    public class RegisteredCommand
    {
        int _TraceCount = 5;
        public int TraceCount
        {
            get { return _TraceCount; }
            set { _TraceCount = value; }
        }

        bool _IsEnable = true;
        public bool IsEnable
        {
            get { return _IsEnable; }
            set { _IsEnable = value; }
        }
        public string Name { get; set; }
        public string Memo { get; set; }
        public DateTime LastExecuteTime { get; set; }

        Dictionary<DateTime, string> _TraceList = new Dictionary<DateTime, string>();
        public Dictionary<DateTime, string> TraceList
        {
            get { return _TraceList; }
        }

        public Func<Dictionary<string, string>, string> ExecuteFunction { get; set; }
        public string Execute(Dictionary<string, string> parameterList)
        {
            if (ExecuteFunction == null)
                return "Error function, no valid function registered!";
            try
            {
                var s= ExecuteFunction(parameterList);
                TraceList.Add(DateTime.UtcNow,s);
                if (TraceList.Count > TraceCount)
                {
                    var d = TraceList.FirstOrDefault();
                    TraceList.Remove(d.Key);
                }
                return s;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
    }
}
