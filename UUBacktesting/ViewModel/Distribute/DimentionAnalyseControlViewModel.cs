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
    class DimentionAnalyseControlViewModel:ViewModelBase
    {
        int _DistributeAnalyseStep = 20;
        public int DistributeAnalyseStep
        {
            get { return _DistributeAnalyseStep; }
            set {  _DistributeAnalyseStep=value;OnPropertyChanged("DistributeAnalyseStep"); }
        }

        bool _IsBusy = false;
        public bool IsBusy
        {
            get { return _IsBusy; }
            set { _IsBusy = value; OnPropertyChanged("IsBusy"); }
        }

        //public Telerik.Windows.Controls.RadCartesianChart Chart { get; set; }

        ObservableCollection<string> _LendList = new ObservableCollection<string>();
        public ObservableCollection<string> LendList { get { return _LendList; } }

        string _CurrentItem;
        public string CurrentItem { get { return _CurrentItem; }

            set {
                if (value == null) return;
                _CurrentItem = value;
                OnPropertyChanged("CurrentItem");
                if (ValueList.ContainsKey(value))
                {
                    DataList.Clear();
                    AbsoluteDataList.Clear();

                    ValueList[value].Item1.ForEach(v => DataList.Add(new DataPoint() { XValue = v.Item1, YValue = v.Item2 }));

                    ValueList[value].Item2.ForEach(v => AbsoluteDataList.Add(new DataPoint() { XValue=v.Item1,YValue=v.Item2}));
                }
            } }

        ObservableCollection<DataPoint> _DataList = new ObservableCollection<DataPoint>();
        ObservableCollection<DataPoint> _AbsoluteDataList = new ObservableCollection<DataPoint>();
        public ObservableCollection<DataPoint> DataList { get { return _DataList; }  }
        public ObservableCollection<DataPoint> AbsoluteDataList { get { return _AbsoluteDataList; } }
        Dictionary<string,Tuple< List<Tuple<double, double>>, List<Tuple<double, double>>>> ValueList { get; set; }
        public void LoadData(Dictionary<string, Tuple<List<Tuple<double, double>>, List<Tuple<double, double>>>> valueList)
        {
            ValueList = valueList;
            LendList.Clear();
            foreach (var v in valueList)
            {
                LendList.Add(v.Key);
            }
            CurrentItem = LendList.FirstOrDefault();
            


        }



    }


}
