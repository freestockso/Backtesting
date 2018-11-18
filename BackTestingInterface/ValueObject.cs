using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib;
using Newtonsoft.Json;

namespace BackTestingInterface
{
    public class NameValueObject
    {
        public virtual string TargetName { get; set; }
        public virtual string Name { get; set; }
        public virtual string Memo { get; set; }
        public virtual object Value { get; set; } //must support json
        public Func<double> GetDoubleValue { get; set; }
        public virtual object GetData()
        {
            var copy = new TimeValueObject();
            copy.Name = Name;
            copy.Memo = Memo;
            copy.TargetName = TargetName;
            copy.Value = Value;
            copy.IsTested = IsTested;
            return copy;
        }

        public virtual void SetData(TimeValueObject obj)
        {
            if (obj == null) return;
            Name = obj.Name;
            Memo = obj.Memo;
            TargetName = obj.TargetName;
            Value = obj.Value;
            IsTested = obj.IsTested;
        }

        private bool _IsTested = false;
        public bool IsTested
        {
            get { return _IsTested; }
            set { _IsTested = value; }
        }

        [JsonIgnore]
        public virtual string ValueShowString
        {
            get
            {
                if (Value == null) return null; return Value.ToString();
            }
        }

        [JsonIgnore]
        public double DoubleValue
        {
            get
            {
                if (GetDoubleValue != null)
                    return GetDoubleValue();
                if (Value != null)
                {
                    if (Value is Money)
                        return Convert.ToDouble((Value as Money).Number);
                    return Convert.ToDouble(Value);
                }
                return 0;
            }
            set { Value = value; }
        }
    }

    public class TimeValueObject 
    {
        public virtual string TargetName { get; set; }
        public virtual string Name { get; set; }
        public virtual string Memo { get; set; }
        public virtual DateTime Time { get; set; }
        public virtual object Value { get; set; } //must support json
        public Func<double> GetDoubleValue { get; set; }
        public virtual object GetData()
        {
            var copy = new TimeValueObject();
            copy.Name = Name;
            copy.Memo = Memo;
            copy.TargetName = TargetName;
            copy.Time = Time;
            copy.Value = Value;
            copy.IsTested = IsTested;
            return copy;
        }

        public virtual void SetData(TimeValueObject obj)
        {
            if (obj == null) return;
            Name = obj.Name;
            Memo = obj.Memo;
            TargetName = obj.TargetName;
            Time = obj.Time;
            Value = obj.Value;
            IsTested = obj.IsTested;
        }

        private bool _IsTested = false;
        public bool IsTested
        {
            get { return _IsTested; }
            set { _IsTested = value;  }
        }

        [JsonIgnore]
        public virtual string ValueShowString
        {
            get
            {
                if (Value == null) return null; return Value.ToString();
            }
        }

        [JsonIgnore]
        public double DoubleValue
        {
            get {
                if (GetDoubleValue != null)
                    return GetDoubleValue();
                if (Value != null)
                {
                    if (Value is Money)
                        return Convert.ToDouble((Value as Money).Number);
                    return Convert.ToDouble(Value);
                }
                return 0; }
            set { Value = value; }
        }


    }

}
