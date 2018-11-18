using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    public interface IWorkObject
    {
        //List<Log> LogList { get; }
        bool Fine { get; set; }
        void PrepareWork();
        void FinishWork();
    }
}
