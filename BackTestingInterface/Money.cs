using System;
using CommonLib;

namespace BackTestingInterface
{
    public class Money : ICloneable//only support RMB now
    {

        private static double EPSILON = 0.0000001;

        private static bool _isFixed = true;
        public static bool IsFixed
        {
            get { return _isFixed; }
            set { _isFixed = value; }
        }

        private static DateTime _fxRateTime = DateTime.Now;
        public static DateTime FxRateTime
        {
            get { return _fxRateTime; }
            set { _fxRateTime = value; }
        }

        private double _number = 0;
        public double Number
        {
            get { return _number; }
            set { _number = value; }
        }

        private string _fxCode = "RMB";
        public string FxCode
        {
            get
            {
                if (string.IsNullOrEmpty(_fxCode))
                    _fxCode = "RMB";
                return _fxCode;
            }
            set { _fxCode = value; }
        }

        public static Money operator +(Money m1, Money m2)
        {
            var result = new Money { FxCode = m1.FxCode };
            if (m1.FxCode == m2.FxCode)
            {
                result.Number = m1.Number + m2.Number;
            }
            

            return result;
        }
        public static Money operator -(Money m1, Money m2)
        {
            var result = new Money { FxCode = m1.FxCode };
            if (m1.FxCode == m2.FxCode)
            {
                result.Number = m1.Number - m2.Number;
            }
            

            return result;
        }

        public static bool operator >(Money m1, Money m2)
        {
            var result = m1 - m2;
            if (result.Number > 0) return true;
            return false;
        }

        public static bool operator <(Money m1, Money m2)
        {
            var result = m1 - m2;
            if (result.Number < 0) return true;
            return false;
        }

        public static bool operator ==(Money m1, Money m2)
        {
            var result = m1 - m2;
            const double epsilon = 0.00001;
            if (Math.Abs(result.Number - 0) < epsilon) return true;
            return false;
        }

        public static bool operator !=(Money m1, Money m2)
        {
            var result = m1 - m2;
            const double epsilon = 0.00001;
            if (Math.Abs(result.Number - 0) > epsilon) return true;
            return false;
        }

        public static Money operator *(Money m1, double times)
        {
            var result = new Money { FxCode = m1.FxCode, Number = m1.Number * times };
            return result;
        }

        public static Money operator /(Money m1, double times)
        {
            if (Math.Abs(times - 0) < EPSILON) throw new Exception("Divide zero");
            var result = new Money { FxCode = m1.FxCode, Number = m1.Number / times };
            return result;
        }

        public static double operator /(Money m1, Money m2)
        {
            var result = new Money { FxCode = m1.FxCode };
            if (m1.FxCode == m2.FxCode)
            {
                result.Number = m2.Number;
            }

            return m1.Number / result.Number;
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public void LoadData(object obj)
        {
            if (obj == null || !(obj is Money)) return;
            var m = obj as Money;
            Number = m.Number;
            FxCode = m.FxCode;
        }
        public object GetData()
        {
            return Clone();
        }
        public object Clone()
        {
            var m = new Money()
                {
                    Number = Number,
                    FxCode = FxCode
                };
            return m;
        }

        public override string ToString()
        {
            return Number.ToString()+" "+FxCode;
        }


    }

}
