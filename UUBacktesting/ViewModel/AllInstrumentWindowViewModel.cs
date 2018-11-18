using UUBacktesting.View;
using BackTestingCommonLib;
using BackTestingInterface;
using CommonLibForWPF;
using Dapper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UUBacktesting.ViewModel
{
    public class AllInstrumentWindowViewModel : ViewModelBase
    {
        public static List<IInstrument> GetInstrumentList()
        {
            var w = new AllInstrumentWindow();
            var vm = new AllInstrumentWindowViewModel();
            vm.Init();
            w.DataContext = vm;
            //w.Closing += W_Closing;
            w.ShowDialog();
            return w.GetSelectItem();

        }

        public static IInstrument GetInstrument(string ticker)
        {
            return AllInstrumentList.FirstOrDefault(v => v.Ticker == ticker);
        }

        ObservableCollection<IInstrument> _InstrumentList = new ObservableCollection<IInstrument>();
        public ObservableCollection<IInstrument> InstrumentList {
            get { return _InstrumentList; }
        }
        public void Filter(string filter)
        {
            InstrumentList.Clear();
            if (string.IsNullOrEmpty(filter))
            {
                AllInstrumentList.ForEach(v => InstrumentList.Add(v));
            }
            else
            {
                AllInstrumentList.ForEach(v =>
                {
                    if (v.Ticker.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                    || v.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                    || v.PYName.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0)
                        InstrumentList.Add(v);
                });

            }
        }

        static List<IInstrument> _AllInstrumentList;
        public static List<IInstrument> AllInstrumentList
        {
            get
            {
                if (_AllInstrumentList == null)
                {
                    _AllInstrumentList = new List<IInstrument>();
                    var vm = new AllInstrumentWindowViewModel();
                    vm.Init();
                }
                return _AllInstrumentList;
            }
        }
        public void Init()
        {
            AllInstrumentList.Clear();
            InstrumentList.Clear();
            Instrument.AllInstrumentList.ForEach(r =>
            {
                AllInstrumentList.Add(r);
                InstrumentList.Add(r);
            });
        }
        public CommonCommand LoadCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {

                    Init();
                });
            }
        }
    }

}
