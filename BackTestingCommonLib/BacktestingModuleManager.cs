using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib;
using CommunicationInterface;

namespace BackTestingCommonLib
{
    public abstract class BacktestingModuleManager : IDisposable, IIdentifiedObject
    {
        private Guid _ObjectID = Guid.NewGuid();
        public Guid ObjectID
        {
            get { return _ObjectID; }
            set { _ObjectID = value; }
        }



        public abstract void InitModule();
        public void Initialize()
        {
            IMqSupport mq = BackTestingCommon.GetMqSupport();

            mq.CreateMessageQueue(ObjectID + "_" + "CommandQueue");

            mq.BindingQueueToExchange(ObjectID + "_" + "LogQueue", BacktestingServer.GetLogExchange(null));
            mq.BindingQueueToExchange(ObjectID + "_" + "CommandQueue", BacktestingServer.GetCommandExchange(null));

            mq.SubscriptionMessage(ObjectID + "_CommandQueue", (s) =>
                {
                    var query = CommonLib.CommonProc.ConvertStringToObject<CommonQuery>(s);
                    if (query == null) return;
                    var id = query.GetParameter<string>("WorkspaceID");
                    if (query.Command == "Create Module" && !string.IsNullOrEmpty(id))
                    {
                        AddModule(query);
                        Console.WriteLine(s);
                    }
                    if (query.Command == "Remove Module" && !string.IsNullOrEmpty(id))
                    {
                        RemoveModule(query);
                        Console.WriteLine(s);
                    }
                } );
            InitModule();
            Register();
        }
        public string Name { get; set; }
        IMqSupport _mq = BackTestingCommon.GetMqSupport();
        public IMqSupport MessageQueueSupport
        { get { return _mq; } }
        List<BacktestingModule> _ModuleList = new List<BacktestingModule>();
        public List<BacktestingModule> ModuleList { get { return _ModuleList; } }

        protected string _CommandQueue = "";
        public string CommandQueue { get { return ObjectID + "_" + _CommandQueue; } }

        public void Register()
        {
            var query = new CommonQuery();
            query.Command = "Register";
            query.SetParameter("ModuleManager",this);
            CommonQuery.StartQuery(query, MessageQueueSupport);
        }
        public void UnRegister()
        {
            var query = new CommonQuery();
            query.Command = "UnRegister";
            query.SetParameter("ModuleManager", this);
            CommonQuery.StartQuery(query, MessageQueueSupport);
        }

        public abstract void AddModule(CommonQuery command);
        public abstract void RemoveModule(CommonQuery command);

        public void Dispose()
        {
            
        }
    }
}
