using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BackTestingInterface;
using BackTestingCommonLib;

using Newtonsoft.Json;

namespace BackTestingCommonLib
{
    public class Account:IAccount
    {
        bool _IsUnlimited = true;
        public bool IsUnlimited { get { return _IsUnlimited; } set { _IsUnlimited = value;if (value) Quantity = 0; } }
        public bool IsValidModify(Money target)
        {
            if (IsUnlimited) return true;
            var targetMoney = CurrentValue + target;
            if (targetMoney.Number < MaxValue && targetMoney.Number > MinValue)
                return true;
            return false;
        }
        public bool ObtainMoney(Money target)
        {
            var targetMoney=CurrentValue + target;
            if (targetMoney.Number < MaxValue&&targetMoney.Number>MinValue)
            {
                Quantity = targetMoney.Number;
                CurrentCurrency = targetMoney.FxCode;
                return true;
            }
            return false;
        }
        public bool CommitMoney(Money target)
        {
            var targetMoney = CurrentValue - target;
            if (targetMoney.Number < MaxValue && targetMoney.Number > MinValue)
            {
                Quantity = targetMoney.Number;
                CurrentCurrency = targetMoney.FxCode;
                return true;
            }
            return false;
        }

        [JsonIgnore]
        public Money CurrentValue
        {
            get
            {
                var money = new Money() { Number = Quantity, FxCode = CurrentCurrency };
                return money;
            }
        }

        private double _Quantity = 0;
        public double Quantity
        {
            get { return _Quantity; }
            set
            {
                if (value < MaxValue && value > MinValue)
                {
                    _Quantity = value;
                    IsChanged = true;
                }
                else
                {
                    throw (new Exception("Account quantity overflow!",
                        new Exception("Value is:" + value.ToString() + ",Max is:" + MaxValue.ToString() + ",Min is:" + MinValue.ToString())));
                }
            }
        }

        private bool _IsEnable = true;
        public bool IsEnable
        {
            get { return _IsEnable;  }
            set { _IsEnable = value; IsChanged = true; }
        }

        private double _MaxValue = double.MaxValue;
        public double MaxValue
        {
            get { return _MaxValue; }
            set { _MaxValue = value;  IsChanged = true; }
        }

        private double _MinValue = double.MinValue;
        public double MinValue
        {
            get { return _MinValue; }
            set { _MinValue = value;  IsChanged = true; }
        }

        private string _CurrentCurrency = "RMB";
        public string CurrentCurrency
        {
            get { return _CurrentCurrency; }
            set
            {
                if (CurrentCurrency == value || String.IsNullOrEmpty(value)) return;
                _CurrentCurrency = value;
                IsChanged = true;
            }
        }

        bool _IsChanged = false;
        public bool IsChanged
        {
            get
            {
                return _IsChanged;
            }
            set
            {
                _IsChanged = value;
                _LastModify = DateTime.Now;
            }
        }

        DateTime _LastModify = DateTime.Now;
        public DateTime LastModifyTime
        {
            get
            {
                return _LastModify;
            }
        }

        public object Clone()
        {
            var acc = new Account();
            acc.CurrentCurrency = CurrentCurrency;
            acc.MinValue = MinValue;
            acc.MaxValue = MaxValue;
            acc.IsEnable = IsEnable;
            acc.Quantity = Quantity;

            return acc;
        }
    }
}
