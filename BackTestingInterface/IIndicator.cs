using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib;


namespace BackTestingInterface
{
    public delegate void IndicatorEventHandler(object sender, IndicatorEventArgs e);

    public interface IIndicator : ICloneable, IDataObject, IOriginalSupport, ISerialSupport, IParameterSupportObject, IWorkObject
    {

        List<IIndicatorValue> DataList { get; }

        IInstrument InterestedInstrument { get; set; }

        List<IIndicatorValue> GetIndicatorData(List<IMarketData> data);

        IIndicator CreateInstance();
    }

    public class IndicatorEventArgs : EventArgs
    {
        public List<IIndicatorValue> Data { get; set; }
        public IndicatorEventArgs(List<IIndicatorValue> data)
        {
            Data = data;
        }
    }
}
