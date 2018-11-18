using BackTestingInterface;
using CommonLibForWPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WeightControlStrategy
{
    public class PosWeightTarget : ViewModelBase
    {
        public static Parameter TransParameter(PosWeightTarget value)
        {
            return new Parameter()
            {
                Name = value.TargetTicker,
                Value = CommonLib.CommonProc.ConvertObjectToString(value)
            };
        }
        public static PosWeightTarget TransPosWeightTarget(Parameter p)
        {
            if (p == null) return null;
            var o = CommonLib.CommonProc.ConvertStringToObject<PosWeightTarget>(p.Value);
            return o;
        }

        string _targetTicker= null;
        public string TargetTicker//pos ticker
        {
            get { return _targetTicker; }
            set
            {
                _targetTicker = value;
                OnPropertyChanged("TargetTicker");

            }
        }
        private double _Shares = 0;
        public double Shares
        {
            get { return _Shares; }
            set
            {
                _Shares = value;
                OnPropertyChanged("Shares");
            }
        }
        private double _CurrentPrice;
        public double CurrentPrice
        {
            get { return _CurrentPrice; }
            set
            {
                _CurrentPrice = value;
                OnPropertyChanged("CurrentPrice");
            }
        }
        double _currentweight = -1;
        public double CurrentWeight
        {
            get { return _currentweight; }
            set
           { 
                _currentweight = value;
                OnPropertyChanged("CurrentWeight");
                OnPropertyChanged("AbsDistance");
            }
        }

        double _targetweight = -1;
        public double TargetWeight
        {
            get { return _targetweight; }
            set
            {
                _targetweight = value;
                OnPropertyChanged("TargetWeight");
                OnPropertyChanged("AbsDistance");
            }
        }

        double _targetthreshold = 0.1;
        public double TargetThreshold
        {
            get { return _targetthreshold; }
            set
            {
                _targetthreshold = value;
                OnPropertyChanged("TargetThreshold");
            }
        }

        public double AbsDistance
        {
            get
            {
                return CurrentWeight - TargetWeight;
            }
        }

        private double _targetShare = 1;
        public double TargetShare
        {
            get { return _targetShare; }
            set { _targetShare = value; OnPropertyChanged("TargetShare"); }
        }

        public DateTime DataTime { get; set; }
        public Money CurrentValue
        {
            get
            {
                var m = new Money() { FxCode = CurrentCurrency };
                m.Number = CurrentPrice * Shares;
                return m;
            }
        }
        public string CurrentCurrency
        {
            get;
            set;
        }

        public string TargetPortfolioName { get; set; }
    }

}
