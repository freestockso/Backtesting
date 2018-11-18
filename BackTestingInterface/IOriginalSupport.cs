using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    public interface IOriginalSupport
    {
        void SaveOriginalStatus();
        void LoadOriginalStatus();
    }
}
