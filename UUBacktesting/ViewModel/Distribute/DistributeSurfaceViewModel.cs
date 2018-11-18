using BackTestingCommonLib;
using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UUBacktesting.ViewModel.Distribute
{
    class DistributeSurfaceViewModel : ViewModelBase
    {
        bool _IsBusy = false;
        public bool IsBusy
        {
            get { return _IsBusy; }
            set { _IsBusy = value; OnPropertyChanged("IsBusy"); }
        }
        ObservableCollection<DistributeValue> _ValueList = new ObservableCollection<DistributeValue>();
        public ObservableCollection<DistributeValue> ValueList { get { return _ValueList; } }

        public void LoadData(List<DistributeValue> valueList)
        {
            ValueList.Clear();
            valueList.ForEach(v => ValueList.Add(v));
        }
    }
}
