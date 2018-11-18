using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    
    public interface IProject:ICloneable, IOriginalSupport, IDataObject,IWorkObject, IChangeNotify
    {
        MarketDataGrade Grade { get; set; }
        List<IInstrument> InstrumentList { get; }
        List<ICondition> ConditionList { get; }
        List<SerialInfo> ConditionSerialList { get; }
        IDataSource CurrentDataSource { get; set; }
        SerialInfo DataSourceSerial { get; set; }

        ProjectStatus Status { get; set; }
        int TestStepDelayMS { get; set; }

        bool CanRun();
        void Start();//data source start
        void Pause();//stop send data
        void Stop();//data source object reback status
        double FinishPercent { get; }

        DateTime ProjectStartTime { get; set; }

        void SerialObject();
        void RecoverySerialObject();

        string GetConditionInfo();
        int SetConditionInfo(string s);
        string GetInstrumentInfo();
        int SetInstrumentInfo(string s);

        string GetResult();
    }

    public enum ProjectStatus
    {
        Running,Pause,Stopped
    }
}
