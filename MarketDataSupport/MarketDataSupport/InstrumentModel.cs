using BackTestingCommonLib;
using BackTestingInterface;
using CommonLibForWPF;
using ReportCommonLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarketDataSupport
{
    public class InstrumentModel : ViewModelBase, IInstrument
    {
        public InstrumentModel()
        {

        }
        public InstrumentModel(IInstrument inst)
        {
            if (inst == null) throw new Exception("null instrument");
            TargetInstrument = inst;
        }
        [ReportOutputIgnore]
        public override bool IsChanged
        {
            get { return base.IsChanged; }
            set { base.IsChanged = value; OnPropertyChanged("IsChanged"); }
        }

        IInstrument _TargetInstrument = new Instrument();
        [ReportOutputIgnore]
        public IInstrument TargetInstrument { get { return _TargetInstrument; } set { _TargetInstrument = value; } }
        public string Ticker
        {
            get
            {
                return TargetInstrument.Ticker;
            }

            set
            {
                TargetInstrument.Ticker = value;
                OnPropertyChanged("Ticker");
            }
        }

        public double PE
        {
            get
            {

                return TargetInstrument.PE;
            }

            set
            {
                TargetInstrument.PE = value;
                OnPropertyChanged("PE");
            }
        }

        public double PB
        {
            get
            {
                return TargetInstrument.PB;
            }

            set
            {
                TargetInstrument.PB = value;
                OnPropertyChanged("PB");
            }
        }

        public double MarketValue
        {
            get
            {
                return TargetInstrument.MarketValue;
            }

            set
            {
                TargetInstrument.MarketValue = value;
                OnPropertyChanged("MarketValue");
            }
        }

        public string Industory
        {
            get
            {
                return TargetInstrument.Industory;
            }

            set
            {
                TargetInstrument.Industory = value;
                OnPropertyChanged("Industory");
            }
        }

        public string Region
        {
            get
            {
                return TargetInstrument.Region;
            }

            set
            {
                TargetInstrument.Region = value;
                OnPropertyChanged("Region");
            }
        }

        public string Currency
        {
            get
            {
                return TargetInstrument.Currency;
            }

            set
            {
                TargetInstrument.Currency = value;
                OnPropertyChanged("Currency");
            }
        }

        public double Margin
        {
            get
            {
                return TargetInstrument.Margin;
            }

            set
            {
                TargetInstrument.Margin = value;
                OnPropertyChanged("Margin");
            }
        }

        public double OrderFixedCost
        {
            get
            {
                return TargetInstrument.OrderFixedCost;
            }
            set
            {
                TargetInstrument.OrderFixedCost = value;
                OnPropertyChanged("OrderFixedCost");
            }
        }

        public double OrderPercentCost
        {
            get
            {
                return TargetInstrument.OrderPercentCost;
            }
            set
            {
                TargetInstrument.OrderPercentCost = value;
                OnPropertyChanged("OrderPercentCost");
            }
        }

        public string Name
        {
            get
            {
                return TargetInstrument.Name;
            }
            set
            {
                TargetInstrument.Name = value;
                OnPropertyChanged("Name");
            }
        }

        public string Memo
        {
            get
            {
                return TargetInstrument.Memo;
            }
            set
            {
                TargetInstrument.Memo = value;
                OnPropertyChanged("Memo");
            }
        }
        [ReportOutputIgnore]
        public double CurrentPrice
        {
            get
            {
                return TargetInstrument.CurrentPrice;
            }

            set
            {
                TargetInstrument.CurrentPrice = value;
                OnPropertyChanged("CurrentPrice");
            }
        }
        [ReportOutputIgnore]
        public string PYName
        {
            get
            {
                if (TargetInstrument == null) return null;
                return TargetInstrument.PYName;
            }
        }

        public object Clone()
        {
            return TargetInstrument.Clone() as IInstrument;
        }
    }

}
