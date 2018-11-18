using CommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackTestingInterface
{
    public interface IParameterSupportObject
    {
        List<Parameter> ParameterList { get; }
        void SaveToParameterList();
        void LoadFromParameterList();
    }
}
