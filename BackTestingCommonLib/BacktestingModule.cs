using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommonLib;
using CommunicationInterface;

namespace BackTestingCommonLib
{
    public abstract class BacktestingModule : IDisposable, IIdentifiedObject
    {
        public string TargetWorkspaceID { get; set; }

        public string Description { get; set; }
        public string Status { get; set; }

        Dictionary<string, object> _ParameterList = new Dictionary<string, object>();
        public Dictionary<string, object> ParameterList { get { return _ParameterList; } }

        IMqSupport _mq = BackTestingCommon.GetMqSupport();
        public IMqSupport MessageQueueSupport
        { get { return _mq; } }

        private string _CommandQueue = "CommandQueue";
        public string CommandQueueName
        {
            get { return ObjectID + "_" + _CommandQueue; }
        }

        public string StatusExchange { get; set; }
        public string DataExchange { get; set; }
        public string LogExchange { get; set; }

        public void WriteLog(string Message,string LogType)
        {
            var log = new LogMessage() {CreateTime = DateTime.Now, Message = Message, MessageType = LogType};
            if(!string.IsNullOrEmpty(LogExchange))
                MessageQueueSupport.SendMessage(LogExchange,CommonLib.CommonProc.ConvertObjectToString(log));
        }
        public void ReportSetting(CommonQuery query)
        {
            query.ParameterList.Clear();
            foreach (var p in ParameterList)
                query.ParameterList.Add(new Parameter()
                    {
                        Name=p.Key,
                        Value=CommonLib.CommonProc.ConvertObjectToString(p.Value),
                        DataTypeFullName = p.Value.GetType().AssemblyQualifiedName
                    });
            CommonQuery.ReplyQuery(query, MessageQueueSupport);
        }
        public void ModifySetting(CommonQuery query)
        {
            foreach (var p in query.ParameterList)
            {
                if (ParameterList.ContainsKey(p.Name))
                    ParameterList[p.Name] = p.Value;
                else
                {
                    ParameterList.Add(p.Name, p.Value);
                }
            }
        }

        public void RegisterModule(string targetQueue)
        {
            MessageQueueSupport.CreateMessageQueue(targetQueue);
            MessageQueueSupport.SubscriptionMessage(targetQueue, (s) =>
            {
                ProcessMessage(s);
            });
        }

        Dictionary<string, Action<string>> _CommandDic = new Dictionary<string, Action<string>>();
        public Dictionary<string, Action<string>> CommandDic
        {
            get { return _CommandDic; }
        }

        public void ProcessMessage(string commandName)
        {
            if (CommandDic.ContainsKey(commandName))
                CommandDic[commandName].BeginInvoke(commandName, null, null);
        }

        public abstract void InitializeModule();
        public virtual void Initialize(string taregtWorkspace)
        {
            TargetWorkspaceID = taregtWorkspace;

            InitializeModule();
        }

        public Timer statusTimer = null;
        public Timer StatusTimer
        {
            get
            {
                if (statusTimer == null)
                {
                    statusTimer = new Timer(UpdateStatus, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(1));
                }
                return statusTimer;
            }
        }

        void UpdateStatus(object status)
        {
            MessageQueueSupport.SendMessage("Status", Status);
        }

        public void Dispose()
        {

        }

        private Guid _ObjectID = Guid.NewGuid();
        public Guid ObjectID
        {
            get { return _ObjectID; }
            set { _ObjectID = value;  }
        }
    }
}
