using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    public interface IChartDataPoint
    {
        double? GetDoubleValue();
        object getIndex();

        DataPointType GetDataType();
    }

    public enum DataPointType
    {
        Point, Category, DateTime
    }
}
