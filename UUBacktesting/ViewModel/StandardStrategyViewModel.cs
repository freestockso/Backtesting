using BackTestingCommonLib;
using BackTestingCommonLib.Strategy;
using BackTestingInterface;
using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UUBacktesting.ViewModel
{
    class StandardStrategyViewModel : ParameterObjectViewModelBase<StandardStrategy>, IObersverSupport
    {
        public virtual string Name
        {
            get { if (TargetObject != null) return TargetObject.Name; return ""; }
            set { if (TargetObject != null) { TargetObject.Name = value; OnPropertyChanged("Name"); } }
        }

        public virtual string Memo
        {
            get { if (TargetObject != null) return TargetObject.Memo; return ""; }
            set { if (TargetObject != null) { TargetObject.Memo = value; OnPropertyChanged("Memo"); } }
        }
        //public double StopProfitPercent
        //{
        //    get { return TargetObject.StopProfitPercent; }
        //    set { TargetObject.StopProfitPercent = value; OnPropertyChanged("StopProfitPercent"); }
        //}
        //public double StopLossPercent
        //{
        //    get { return TargetObject.StopLossPercent; }
        //    set { TargetObject.StopLossPercent = value; OnPropertyChanged("StopLossPercent"); }
        //}
        ObservableCollection<TradeAlgorithmViewModel> _TradeAlgorithmList = new ObservableCollection<TradeAlgorithmViewModel>();
        public ObservableCollection<TradeAlgorithmViewModel> TradeAlgorithmList { get { return _TradeAlgorithmList; } }

        public TradeAlgorithmViewModel CurrentTradeAlgorithm { get; set; }

        public override void Load()
        {
            base.Load();
            TradeAlgorithmList.Clear();
            TargetObject.TradeAlgorithmList.ForEach(v =>
            {
                TradeAlgorithmList.Add(new TradeAlgorithmViewModel(v));
            });
        }

        public virtual bool NeedRefresh()
        {
            return false;
        }

        public void Refresh()
        {
            
        }

        public CommonCommand SaveOrigenalCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (TargetObject == null) return;
                    Save();
                    TargetObject.SaveOriginalStatus();
                });
            }
        }
        public CommonCommand LoadOrigenalCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (TargetObject == null) return;

                    TargetObject.LoadOriginalStatus();
                    Load();
                });
            }
        }

        public CommonCommand AddTradeConditionCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (TargetObject == null) return;
                    var t = new TradeAlgorithm();
                    var vm = new TradeAlgorithmViewModel(t); 
                    TradeAlgorithmList.Add(vm);
                    TargetObject.TradeAlgorithmList.Add(t);
                });
            }
        }
        public CommonCommand DeleteTradeAlgorithmCommand
        {
            get
            {
                return new CommonCommand((o) =>
                {
                    if (TargetObject == null) return;
                    if (CurrentTradeAlgorithm != null)
                    {
                        TargetObject.TradeAlgorithmList.Remove(CurrentTradeAlgorithm.TargetObject);
                        TradeAlgorithmList.Remove(CurrentTradeAlgorithm);
                       
                    }
                });
            }
        }
    }
}
