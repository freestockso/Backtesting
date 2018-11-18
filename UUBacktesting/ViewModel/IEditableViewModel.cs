using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UUBacktesting.ViewModel
{
    interface IEditableViewModel: INotifiedViewModel
    {
        bool IsChanged { get; }
        void Save();
    }
}
