using BackTestingCommonLib;
using BackTestingInterface;
using CommonLib;
using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UUBacktesting.ViewModel.Distribute
{
    class DistributeViewModel:ViewModelBase
    {
        bool _IsBusy = false;
        public bool IsBusy
        {
            get { return _IsBusy; }
            set { _IsBusy = value; OnPropertyChanged("IsBusy"); }
        }
        public string Ticker { get; set; }

        public Dictionary<string, List<DataPoint>> DistributeDictionary
        {
            get; set;
        }

        ObservableCollection<string> _LendList = new ObservableCollection<string>();
        public ObservableCollection<string> LendList { get { return _LendList; } }

        string _CurrentTarget;
        public string CurrentTarget
        {
            get { return _CurrentTarget; }
            set
            {
                _CurrentTarget = value;
                if(_CurrentTarget!=null&& DistributeDictionary!=null&& DistributeDictionary.ContainsKey(_CurrentTarget)&& DistributeDictionary[_CurrentTarget]!=null)
                {
                    DistributeList.Clear();
                    DistributeDictionary[_CurrentTarget].ForEach(v => { DistributeList.Add(v); });
                    var l = ValueList.Where(v => v.Name == _CurrentTarget);
                    CurrentValueList.Clear();
                    foreach (var v in l)
                        CurrentValueList.Add(v);
                    CurrentValue = CurrentValueList.LastOrDefault();
                }
            }
        }
        ObservableCollection<DataPoint> _DistributeList = new ObservableCollection<DataPoint>();
        public ObservableCollection<DataPoint> DistributeList { get { return _DistributeList; } }

        ObservableCollection<DistributeValue> _CurrentValueList = new ObservableCollection<DistributeValue>();
        public ObservableCollection<DistributeValue> CurrentValueList { get { return _CurrentValueList; } }

        List<DistributeValue> _ValueList = new List<DistributeValue>();
        public List<DistributeValue> ValueList { get { return _ValueList; } }

        DistributeValue _CurrentValue;
        public DistributeValue CurrentValue { get { return _CurrentValue; } set { _CurrentValue = value; OnPropertyChanged("CurrentValue"); } }

        ObservableCollection <DataPoint> _NormalDistributeList = new ObservableCollection<DataPoint>();
        public ObservableCollection<DataPoint> NormalDistributeList
        {
            get {
                if(_NormalDistributeList.Count==0)
                {
                    var da = new BackTestingCommonLib. DistributeAnalyse();
                    var l = da.GetNormalDistribute();
                    l.ForEach(v => _NormalDistributeList.Add(v));
                }
                return _NormalDistributeList;
            }
        }

        public void LoadData(Dictionary<DateTime, List<DataPoint>> distributeList, List<DistributeValue> trendList)
        {
            Dictionary<string, List<DataPoint>> dl = new Dictionary<string, List<DataPoint>>();
            foreach (var kv in distributeList)
                dl.Add(kv.Key.ToString(), kv.Value);
            LoadData(dl, trendList);
        }
        public void LoadData(Dictionary<string,List<DataPoint>> distributeList,List<DistributeValue> trendList)
        {

            DistributeDictionary= distributeList;
            if (DistributeDictionary != null && DistributeDictionary.Count > 0)
            {
                CurrentTarget = DistributeDictionary.FirstOrDefault().Key;
                CurrentValue = CurrentValueList.LastOrDefault();
                LendList.Clear();
                foreach (var k in DistributeDictionary.Keys)
                    LendList.Add(k);
            }
            ValueList.Clear();
            trendList.ForEach(v => ValueList.Add(v));
        }
    }
}
