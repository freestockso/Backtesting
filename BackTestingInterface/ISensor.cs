using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackTestingInterface
{
    //public delegate void SensorEventHandler(object sender, SensorEventArgs e);

    public interface ISensor : ICloneable, IDataObject, IOriginalSupport, ISerialSupport, IParameterSupportObject, IWorkObject, IWorkPrepareSupport
    {
        //event SensorEventHandler SensorEvent;
        int DelayMs { get; set; }
        List<ISensorData> DataList { get; }

        //DateTime StartTime { get; set; }
        //DateTime EndTime { get; set; }
        //DateTime CurrentDataTime
        //{
        //    get; set;
        //}
        List<ISensorData> GetSensorDataList(List<IInstrument> instrumentList,DateTime start, DateTime end);
        //void SendSensorData(List<ISensorData> data);
        //void RequestSensor(DateTime current);
    }

    //public class SensorEventArgs : EventArgs
    //{
    //    public List<ISensorData> Data { get; set; }
    //    public SensorEventArgs(List<ISensorData> data)
    //    {
    //        Data = data;
    //    }
    //}
}