using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    public interface ITrade: ICloneable
    {
        string Ticker { get; set; }
        string Name { get; set; }
        string Memo { get; set; }
        DateTime Time { get; set; }
        double Shares { get; set; }
        double Price { get; set; }
        string Owner { get; set; }
        string Currency { get; set; }
    }
}
