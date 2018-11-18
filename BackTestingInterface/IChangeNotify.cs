using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    public interface IChangeNotify
    {
        bool IsChanged { get; set; }
        DateTime LastModifyTime { get; }
    }
}
