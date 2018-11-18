using BackTestingInterface;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backtesting7.ViewModel
{
    class DataSourceViewModel
    {
        public IDataSource TargetObject { get; set; }
        public virtual string Name
        {
            get { if (TargetObject != null) return TargetObject.Name; return ""; }

        }

        public virtual string Memo
        {
            get { if (TargetObject != null) return TargetObject.Memo; return ""; }

        }

        //ObservableCollection<IMarketData> _DataList = new ObservableCollection<IMarketData>();
        //public ObservableCollection<IMarketData> DataList { get { return _DataList; } }
    }
}
