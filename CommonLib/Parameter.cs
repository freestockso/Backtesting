using CommonLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib
{
    public class Parameter
    {
        string _Name = "";
        public string Name
        {
            get { return _Name; }
            set { _Name = value; _LastModifyTime = DateTime.Now; }
        }
        //bool isInterger = false;//because telerik updown control can not support interger value binding to value propery correctly, only double type can work
        object _Value = "";
        public object Value//limit in simple type
        {
            get { return _Value; }
            set {
                if (value == null)
                {
                    _EditMode = ParameterEditMode.str;//default
                    _LastModifyTime = DateTime.Now;
                    _Value = null;
                    return;
                }
                if (value is byte)
                {
                    _Value = Convert.ToDouble(value);
                    _EditMode = ParameterEditMode.int8;
                    MaxValue = byte.MaxValue;
                    MinValue = byte.MinValue;
                    _LastModifyTime = DateTime.Now;
                    return;
                }
                if (value is short)
                {
                    _Value = Convert.ToDouble(value);
                    _EditMode = ParameterEditMode.int16;
                    MaxValue = short.MaxValue;
                    MinValue = short.MinValue;
                    _LastModifyTime = DateTime.Now;
                    return;
                }
                if (value is int)
                {
                    _Value = Convert.ToDouble(value);
                    _EditMode = ParameterEditMode.int32;
                    MaxValue = int.MaxValue;
                    MinValue = int.MinValue;
                    _LastModifyTime = DateTime.Now;
                    return;
                }
                if (value is long)
                {
                    _Value = Convert.ToDouble(value);
                    _EditMode = ParameterEditMode.int64;
                    MaxValue = long.MaxValue;
                    MinValue = long.MinValue;
                    _LastModifyTime = DateTime.Now;
                    return;
                }
                if (value is sbyte)
                {
                    _Value = Convert.ToDouble(value);
                    _EditMode = ParameterEditMode.usint8;
                    MaxValue = sbyte.MaxValue;
                    MinValue = sbyte.MinValue;
                    _LastModifyTime = DateTime.Now;
                    return;
                }
                if (value is ushort)
                {
                    _Value = Convert.ToDouble(value);
                    _EditMode = ParameterEditMode.usint16;
                    MaxValue = ushort.MaxValue;
                    MinValue = ushort.MinValue;
                    _LastModifyTime = DateTime.Now;
                    return;
                }
                if (value is uint)
                {
                    _Value = Convert.ToDouble(value);
                    _EditMode = ParameterEditMode.usint32;
                    MaxValue = uint.MaxValue;
                    MinValue = uint.MinValue;
                    _LastModifyTime = DateTime.Now;
                    return;
                }
                if (value is ulong)
                {
                    _Value = Convert.ToDouble(value);
                    _EditMode = ParameterEditMode.usint64;
                    MaxValue = ulong.MaxValue;
                    MinValue = ulong.MinValue;
                    _LastModifyTime = DateTime.Now;
                    return;
                }
                if (value is float)
                {
                    _Value = value;
                    _EditMode = ParameterEditMode.dec32;
                    MaxValue = float.MaxValue;
                    MinValue = float.MinValue;
                    _LastModifyTime = DateTime.Now;
                    return;
                }
                if (value is double)
                {
                    _Value = value;
                    _EditMode = ParameterEditMode.dec64;
                    MaxValue = double.MaxValue;
                    MinValue = double.MinValue;
                    _LastModifyTime = DateTime.Now;
                    return;
                }
                if (value is decimal)
                {
                    _Value = value;
                    _EditMode = ParameterEditMode.dec;
                    MaxValue =Convert.ToDouble( decimal.MaxValue);
                    MinValue =Convert.ToDouble( decimal.MinValue);
                    _LastModifyTime = DateTime.Now;
                    return;
                }
                if (value is bool)
                {
                    _Value = value;
                    _EditMode = ParameterEditMode.boolean;
                    _LastModifyTime = DateTime.Now;
                    return;
                }
                if (value is char)
                {
                    _Value = value;
                    _EditMode = ParameterEditMode.cha;
                    MaxValue = char.MaxValue;
                    MinValue = char.MinValue;
                    _LastModifyTime = DateTime.Now;
                    return;
                }
                if (value is DateTime)
                {
                    _Value = value;
                    _EditMode = ParameterEditMode.time;
                    MaxTime = DateTime.MaxValue;
                    MinTime = DateTime.MinValue;
                    _LastModifyTime = DateTime.Now;
                    return;
                }
                if (value is string)
                {
                    _Value = value;
                    _EditMode = ParameterEditMode.str;
                    _LastModifyTime = DateTime.Now;
                    return;
                }
                if(value.GetType().IsSubclassOf(typeof(Enum))){
                    _Value = value;
                    _EditMode = ParameterEditMode.selection;
                    _LastModifyTime = DateTime.Now;
                    return;
                }
                _Value = value;
                _LastModifyTime = DateTime.Now;

            }
        }

        string _Memo = "";
        public string Memo
        {
            get { return _Memo; }
            set { _Memo = value; _LastModifyTime = DateTime.Now; }
        }

        DateTime _LastModifyTime = DateTime.Now;
        public DateTime LastModifyTime
        {
            get { return _LastModifyTime; }
            set { _LastModifyTime = value; }
        }

        public string DefaultValue { get; set; }
        public void Default()
        {
            if (string.IsNullOrEmpty(DefaultValue))
                return;
            if (Value == null)
                Value = DefaultValue;
            else
                Value =CommonLib.CommonProc.GetBasicTypeValue(Value.GetType(), DefaultValue);
        }
        public void SetValue(string name, object value)
        {
            Name = name;
            LastModifyTime = DateTime.Now;
            Value = value;
            if(value!=null)
                DefaultValue = Value.ToString();
        }

        public Parameter GetData()
        {
            var copy = new Parameter();
            copy.Name = Name;
            copy.Memo = Memo;
            copy.DefaultValue = DefaultValue;
            copy.Value = Value;
            return copy;
        }

        public void SetData(Parameter obj)
        {
            if (obj == null) return;
            Name = obj.Name;
            Memo = obj.Memo;
            LastModifyTime = obj.LastModifyTime;
            DefaultValue = obj.DefaultValue;
            Value = obj.Value;
        }

        [JsonIgnore]
        public double MinValue { get; set; }
        [JsonIgnore]
        public double MaxValue { get; set; }
        [JsonIgnore]
        public DateTime MinTime { get; set; }
        [JsonIgnore]
        public DateTime MaxTime { get; set; }
        [JsonIgnore]
        public int MaxLength { get; set; }
        [JsonIgnore]
        public List<object> ValidValueList
        {
            get
            {
                if (Value == null) return null;
                if (Value.GetType().IsSubclassOf(typeof(Enum)))
                {
                    return Enum.GetValues(Value.GetType()).Cast<object>().ToList();
                }
                if (Value.GetType() == typeof(Boolean))
                {
                    return new List<object>() { true,false};
                }
                return null;
            }
        }
        [JsonIgnore]
        ParameterEditMode _EditMode = ParameterEditMode.nul;
        public ParameterEditMode EditMode
        {
            get
            {
                if (Value == null) return ParameterEditMode.nul;
                return _EditMode;
            }
        }

        public static void LoadInfoFromParameterList(object targetObject, List<Parameter> parameterList)
        {
            parameterList.ForEach(v =>
            {
                var f = targetObject.GetType().GetProperty(v.Name);
                if (f != null && f.CanWrite)
                {
                    CommonProc.SetProperty(targetObject, f, v.Value);
                }
            });
        }
        public static List<Parameter> SaveInfoToParameterList(object targetObject)
        {
            var ParameterList = new List<Parameter>();
            var fl = targetObject.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(v => v.GetCustomAttribute(typeof(ParameterOperation)) != null);
            foreach (var f in fl)
            {
                var attr = f.GetCustomAttribute(typeof(ParameterOperation)) as ParameterOperation;
                var p = new Parameter() ;
                p.SetValue(f.Name, f.GetValue(targetObject));
                InitDefaultValue(p,attr);
                ParameterList.Add(p);
            }
            return ParameterList;
            
        }
        static void InitDefaultValue(Parameter p,ParameterOperation attr)
        {
            if (attr != null) {
                if(!string.IsNullOrEmpty(attr.Memo))
                    p.Memo = attr.Memo;
                if (!string.IsNullOrEmpty(attr.Default))
                    p.DefaultValue = attr.Default;
                if (attr.MinValue != null)
                    p.MinValue = attr.MinValue.Value;
                if (attr.MaxValue != null)
                    p.MaxValue = attr.MaxValue.Value;
                if (attr.MinTime != null)
                    p.MinTime = attr.MinTime.Value;
                if (attr.MaxTime != null)
                    p.MaxTime = attr.MaxTime.Value;
            }
            if (string.IsNullOrEmpty(p.Memo))
                p.Memo = GetDefaultMemo(p);
        }
        static string GetDefaultMemo(Parameter p)
        {
            try
            {
                string s = "";
                if (p==null||p.Value == null) return s;
                if (p.Value.GetType() == typeof(int))
                    s= int.MinValue.ToString()+"-"+int.MinValue.ToString()+",";
                else if (p.Value.GetType() == typeof(short))
                    s= short.MinValue.ToString() + "-" + short.MinValue.ToString() + ",";
                else if (p.Value.GetType() == typeof(long))
                    s= long.MinValue.ToString() + "-" + long.MinValue.ToString() + ",";
                else if (p.Value.GetType() == typeof(byte))
                    s= byte.MinValue.ToString() + "-" + byte.MinValue.ToString() + ",";
                else if (p.Value.GetType() == typeof(uint))
                    s= uint.MinValue.ToString() + "-" + uint.MinValue.ToString() + ",";
                else if (p.Value.GetType() == typeof(ushort))
                    s= ushort.MinValue.ToString() + "-" + ushort.MinValue.ToString() + ",";
                else if (p.Value.GetType() == typeof(ulong))
                    s= ulong.MinValue.ToString() + "-" + ulong.MinValue.ToString() + ",";
                else if (p.Value.GetType() == typeof(sbyte))
                    s= sbyte.MinValue.ToString() + "-" + sbyte.MinValue.ToString() + ",";
                else if (p.Value.GetType() == typeof(double))
                    s= double.MinValue.ToString() + "-" + double.MinValue.ToString() + ",";
                else if (p.Value.GetType() == typeof(float))
                    s= float.MinValue.ToString() + "-" + float.MinValue.ToString() + ",";
                else if (p.Value.GetType() == typeof(decimal))
                    s= decimal.MinValue.ToString() + "-" + decimal.MinValue.ToString() + ",";
                else if (p.Value.GetType() == typeof(DateTime))
                    s= DateTime.MinValue.ToString() + "-" + DateTime.MinValue.ToString() + ",";
                else if (p.Value.GetType() == typeof(bool))
                    s= "True or False,";
                else if (p.Value.GetType() == typeof(char))
                    s= char.MinValue.ToString() + "-" + char.MinValue.ToString() + ",";
                else if (p.Value.GetType().IsSubclassOf(typeof(Enum)))
                    s= CommonProc.GetListString<string>(Enum.GetNames(p.Value.GetType()).ToList()) + ",";
                else if(p.Value is string)
                {
                    if (string.IsNullOrEmpty(p.Value.ToString().Trim()))
                    {
                        return "";
                    }
                }
                s+= " default is "+p.DefaultValue;
                return s;
            }
            catch(Exception ex)
            {
                LogSupport.Error(ex);
                return "";
            }
        }
    }
    public enum ParameterEditMode
    {
        int8,int16,int32,int64,usint8,usint16,usint32,usint64,dec32,dec64,dec,time,str,cha,boolean,nul,selection
    }
    public class ParameterOperation : Attribute
    {
        public string Memo { get; set; }
        public string Default { get; set; }
        public double? MinValue { get; set; }
        public double? MaxValue { get; set; }
        public DateTime? MinTime { get; set; }
        public DateTime? MaxTime { get; set; }
    }

}

