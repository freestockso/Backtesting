using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackTestingInterface
{
    public interface IFxRateObject
    {
        string CurrentCurrency { get; set; }
    }
}
