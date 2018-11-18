using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    public interface ISensorData: ICloneable
    {
        string InstrumentTicker { get; set; }
        string DataName { get; set; }
        DateTime DataTime { get; set; }
        string DataInfo { get; set; }
        //string DataTypeName { get; set; }
        //string Data { get; set; }//json format data
        double Credibility { get; set; }
    }
}
