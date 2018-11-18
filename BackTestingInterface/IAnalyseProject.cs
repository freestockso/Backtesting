using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    //analyse all market instruments and select some instruments, base on project
    public interface IAnalyseProject:IProject
    {
        
        List<ICondition> PredicateList { get; }// conditions combine by "and"
        DateTime AnalyseEndTime { get; set; }
        DateTime AnalyseStartTime { get; set; }
        int DefaultFilterNumber { get; set; }

        List<IInstrument> ResultList { get; }
        List<IInstrument> BlockList { get; }
        void GenerateResult();

        List<SerialInfo> PredicateSerialList { get; }// predicate serial info

    }
}
