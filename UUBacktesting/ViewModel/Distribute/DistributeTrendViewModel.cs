using BackTestingInterface;
using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BackTestingCommonLib;

namespace UUBacktesting.ViewModel.Distribute
{
    class DistributeTrendViewModel : ViewModelBase
    {
        bool _IsBusy = false;
        public bool IsBusy
        {
            get { return _IsBusy; }
            set { _IsBusy = value; OnPropertyChanged("IsBusy"); }
        }
        bool _IsTimeMode = true;
        public bool IsTimeMode
        {
            get { return _IsTimeMode; }
            set { _IsTimeMode = value; OnPropertyChanged("IsTimeMode");OnPropertyChanged("IsMarketMode");RefreshMode(); }
        }
        public bool IsMarketMode
        {
            get { return !_IsTimeMode; }
            set { _IsTimeMode = !value; OnPropertyChanged("IsTimeMode"); OnPropertyChanged("IsMarketMode"); RefreshMode(); }
        }

        public Telerik.Windows.Controls.RadCartesianChart Chart { get; set; }
        ObservableCollection<LendItem> _LendList=new ObservableCollection<LendItem>();
        public ObservableCollection<LendItem> LendList { get { return _LendList; } }
        void RefreshChart(Dictionary<string, List<TimeValueObject>> valueList )
        {
            var helper = new TelerikChartViewHelper();
            Chart.Series.Clear();
            var l=helper.AddSeriesToChart(valueList, Chart);

            LendList.Clear();
            foreach(var v in l)
            {
                LendList.Add(v);
            }
        }
        void RefreshChart(Dictionary<string, List<NameValueObject>> valueList)
        {
            var helper = new TelerikChartViewHelper();
            Chart.Series.Clear();
            var l = helper.AddSeriesToChart(valueList, Chart);

            LendList.Clear();
            foreach (var v in l)
            {
                LendList.Add(v);
            }
        }
        public List<DistributeValue> DataList { get; set; }
        public void LoadData(List<DistributeValue> valueList)
        {
            DataList = valueList;
            RefreshMode();
        }

        void RefreshMode()
        {
            if (DataList == null || DataList.Count == 0) return;

            if (IsTimeMode)
            {
                var d = new Dictionary<string, List<TimeValueObject>>();
                var nl = DataList.Select(v => v.Name).Distinct();
                foreach(var n in nl)
                {
                    var l = DataList.Where(v => v.Name == n);
                    var sl = new List<TimeValueObject>();
                    foreach (var data in l)
                        sl.Add(new TimeValueObject() { Time = data.Time, Value = data.Value });
                    d.Add(n, sl);
                }
                RefreshChart(d);
            }
            else
            {
                var d = new Dictionary<string, List<NameValueObject>>();
                var nl = DataList.Select(v => v.Name).Distinct();
                foreach (var n in nl)
                {
                    var l = DataList.Where(v => v.Name == n);
                    var sl = new List<NameValueObject>();
                    foreach (var data in l)
                        sl.Add(new NameValueObject() { Name = data.Name, Value = data.Value });
                    d.Add(n, sl);
                }
                RefreshChart(d);
            }
        }
    }
}
