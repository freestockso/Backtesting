using BackTestingInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingCommonLib
{
    public class SensorData : ISensorData,ICloneable
    {
        public double Credibility
        {
            get;set;
        }

        public string Data
        {
            get;set;
        }

        public string DataInfo
        {
            get;set;
        }

        public string DataName
        {
            get; set;
        }

        public DateTime DataTime
        {
            get; set;
        }

        public string DataTypeName
        {
            get; set;
        }

        public object Clone()
        {
            var d = new SensorData();
            d.Credibility = Credibility;
            d.Data = Data;
            d.DataInfo = DataInfo;
            d.DataName = DataName;
            d.DataTime = DataTime;
            d.DataTypeName = DataTypeName;
            return d;
        }
    }
}
