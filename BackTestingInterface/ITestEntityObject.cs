using CommonLib;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    public interface ITestEntityObject:IDataObject
    {
        void Initialize();
        void PrepareWork();//after modify parameter object need adjust some work before start test
        void FinishWork();

        bool IsEnable { get; set; }
        
        void SaveOriginalStatus();
        void LoadOriginalStatus();

        //List<ITestEntityObject> ReferenceObjectList { get; }//only indicate enetity object each other reference
        //int ReferenceCount { get; set; }
    }


}
