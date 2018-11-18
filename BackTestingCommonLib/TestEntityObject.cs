using BackTestingInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingCommonLib
{
    public abstract class TestEntityObject: ITestEntityObject,ICopyObject
    {
        public abstract string Name { get; set; }
        public abstract string Memo { get; set; }
        public TestEntityObject OriginalObject
        {
            get; set;
        }
        public void SaveOriginalStatus()
        {
            OriginalObject = GetData() as TestEntityObject;
        }
        public void LoadOriginalStatus()
        {
            LoadData(OriginalObject);
        }
        public abstract ICopyObject CreateInstance();

        public abstract object GetData();


        public abstract void LoadData(object obj);

        public virtual void Initialize()
        {
            
        }

        public virtual void PrepareWork()
        {
            
        }

        public virtual void FinishWork()
        {
            
        }

        bool _IsEnable = true;
        public bool IsEnable { get { return _IsEnable; } set { _IsEnable = value; } }

        public void LogError(string log)
        {
            LogSupport.AddLog(log, LogType.Error);
        }
        public void LogInfo(string log)
        {
            LogSupport.AddLog(log);
        }
    }
}
