using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CommonLib
{
    public class LogSupport
    {
        static List<Log> _LogList = new List<Log>();
        public static List<Log> LogList { get { return _LogList; } }

        public static void Info(string message)
        {
            AddLog(message, LogType.Info);
        }
        public static void Error(string message)
        {
            AddLog(message);
            MessageBox.Show("Error", message);
        }
        public static void Error(Exception e)
        {
            Error(e.Message);
        }

        //default is error
        public static void AddLog(string message)
        {
            LogList.Add(new Log() { Message = message });
        }
        public static void AddLog(string message,LogType type)
        {
            LogList.Add(new Log() { Message = message ,LogType=type});
        }
        public static void ClearLog()
        {
            LogList.Clear();
        }
    }

    public class Log
    {
        public string Message { get; set; }

        DateTime _LogTime = DateTime.Now;
        public DateTime LogTime { get { return _LogTime; } }
        LogType _type = LogType.Error;
        public LogType LogType
        {
            get { return _type; }
            set { _type = value; }
        }
    }

    public enum LogType
    {
        Info,Error
    }
}
