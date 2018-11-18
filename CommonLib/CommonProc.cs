using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Win32;
using Newtonsoft.Json;
using System.Reflection;
using System.Windows.Forms;

namespace CommonLib
{
    //support serialize function 
    //last modified 2017-5-17
    public class CommonProc
    {
        public static readonly string CurrentDebug = "Weight Strategy";
        public static readonly double EPSILON = 0.00000000000000000000001;
        private static readonly JsonSerializer jsonSerializer = JsonSerializer.Create(
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
        public static void SaveObjToFile(object o, string url)
        {
            if(File.Exists(url)) File.Delete(url);
            using (var tw = File.CreateText(url))
            using (var jtw = new JsonTextWriter(tw) { Formatting = Newtonsoft.Json.Formatting.Indented })
            {
                jsonSerializer.Serialize(jtw, o);
            }
        }
        public static void SaveStringToFile(string s, string filter = null)
        {
            try
            {
                SaveFileDialog f = new SaveFileDialog();
                if (filter == null)
                    f.Filter = "Text Files(*.txt)|*.txt|All Files(*.*)|*.*";
                else
                    f.Filter = filter;
                if (f.ShowDialog()== DialogResult.OK)
                {
                    using (var tw = File.CreateText(f.FileName))
                        tw.Write(s);
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public static T LoadFromFile<T>(string filter = null)
        {
            OpenFileDialog f = new OpenFileDialog();
            if (filter == null)
                f.Filter = "Text Files(*.txt)|*.txt|All Files(*.*)|*.*";
            else
                f.Filter = filter;
            try
            {
                if (f.ShowDialog()== DialogResult.OK)
                {
                    return LoadObjFromFile<T>(f.FileName);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("load fail");

            }
            return default(T);
        }
        public static T LoadObjFromFile<T>(string url)
        {
            try
            {

                    var s = File.ReadAllText(url);
                    return ConvertStringToObject<T>(s);


            }
            catch (Exception e)
            {

                throw e;
            }
            //return default(T);
        }


        public static T ConvertStringToObject<T>(string s)
        {
            using (var tr = new StringReader(s))
            {
                using (var jtr = new JsonTextReader(tr))
                {
                    var obj = jsonSerializer.Deserialize(jtr, typeof(T));
                    if (obj != null)
                        return (T)obj;
                }
            }
            return default(T);
        }
        public static object ConvertStringToObject(string s)
        {
            using (var tr = new StringReader(s))
            {
                using (var jtr = new JsonTextReader(tr))
                {
                    var obj = jsonSerializer.Deserialize(jtr);
                    if (obj != null)
                        return obj;
                }
            }
            return null;
        }
        public static object ConvertStringToObject(string s,Type t)
        {
            using (var tr = new StringReader(s))
            {
                using (var jtr = new JsonTextReader(tr))
                {
                    var obj = jsonSerializer.Deserialize(jtr,t);
                    if (obj != null)
                        return obj;
                }
            }
            return null;
        }
        public static T Copy<T>(T o)
        {
            string s = ConvertObjectToString(o);
            return ConvertStringToObject<T>(s);
        }
        public static Type FindType(string typeName)
        {
            if(string.IsNullOrEmpty(typeName))
                return null;
            if(typeName.Contains(","))
                typeName=typeName.Substring(0,typeName.IndexOf(","));
            foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var t = assembly.GetType(typeName, false, true);
                if (t != null)
                    return t;
            }
            return null;
        }
        public static string ConvertObjectToString(object o)
        {
            if (o == null) return null;
            if (o is string) return o as string;
            //if (o is Enum) return o.ToString(); 
            string s;
            using (var tw = new StringWriter())
            using (var jtw = new JsonTextWriter(tw) { Formatting = Newtonsoft.Json.Formatting.None })
            {
                jsonSerializer.Serialize(jtw, o);
                s = tw.ToString();
            }
            return s;
        }

        public static void TrimDoubleList(List<double> l,bool trimLeft=true,bool trimRight=true)
        {
            int si = 0, ei = 0;
            for(int i = 0; i < l.Count; i++)
            {
                if (Math.Abs(l[i]) > EPSILON)
                {
                    si = i;
                    break;
                }
            }
            for(int i = l.Count - 1; i >= 0; i--)
            {
                if (Math.Abs(l[i])>EPSILON)
                {
                    
                    break;
                }
                ei++;
            }

            if(trimLeft)
            {
                for (int i = 0; i < si; i++)
                    l.RemoveAt(0);
            }
            if (trimRight)
                for (int i = 0; i < ei; i++)
                    l.RemoveAt(l.Count - 1);

            
        }

        public static void SynchroniseList<T>(IEnumerable<T> source,ICollection<T> target)
        {
            lock (source)
            {
                var l = target.Except(source).ToList();
                foreach (var v in l)
                    target.Remove(v);
                foreach (var v in source)
                    if (!target.Contains(v)) target.Add(v);
            }
        }

        public static double[] GetMA(double[] data,int n)
        {
            if (n <= 0) return data;
            double[] result = new double[data.Length];
            double[] top = new double[n];
            if (data.Length < n) return result;
            Array.Copy(data, top, n);

            double sum = top.Sum();
            result[n - 1] = sum / n;
            for (int i = n; i < data.Length; i++)
            {
                sum = sum + data[i] - data[i - n];
                result[i] = sum / n;
            }
            return result;
        }

        public static Dictionary<string, string> GetParameterDictionary(string[] args)
        {//every parameter starts with / or - and use : seperate name and value. like /targetTime:2014-5-23 /reportFormat:pdf /mailTo:yinxiao.liu@prcm.com
            var dl = new Dictionary<string, string>();
            foreach (var s in args)
            {
                var ts = s.Trim();
                if (ts.StartsWith("/") || ts.StartsWith("-"))
                {
                    if (ts.Contains(":"))
                    {
                        var ns = ts.Substring(1,ts.IndexOf(":", StringComparison.Ordinal)-1).Trim();
                        var vs = ts.Substring(ts.IndexOf(":",StringComparison.Ordinal)+1).Trim();
                        dl.Add(ns,vs);
                    }
                }

            }
            return dl;
        }

        public static DateTime? GetWeekMonday(DateTime t)
        {
            if (t.DayOfWeek == DayOfWeek.Monday) return t;
            if (t.DayOfWeek == DayOfWeek.Tuesday) return t - TimeSpan.FromDays(1);
            if (t.DayOfWeek == DayOfWeek.Wednesday) return t - TimeSpan.FromDays(2);
            if (t.DayOfWeek == DayOfWeek.Thursday) return t - TimeSpan.FromDays(3);
            if (t.DayOfWeek == DayOfWeek.Friday) return t - TimeSpan.FromDays(4);
            if (t.DayOfWeek == DayOfWeek.Saturday) return t - TimeSpan.FromDays(5);
            if (t.DayOfWeek == DayOfWeek.Sunday) return t - TimeSpan.FromDays(6);
            return null;
        }
        public static object GetBasicTypeValue(Type t,object value)
        {
            if (t == typeof(bool))
                return Convert.ToBoolean(value);
            if (t == typeof(byte))
                return Convert.ToByte(value);
            if (t == typeof(short))
                return Convert.ToInt16(value);
            if (t == typeof(int))
                return Convert.ToInt32(value);
            if (t == typeof(long))
                return Convert.ToInt64(value);
            if (t == typeof(sbyte))
                return Convert.ToSByte(value);
            if (t == typeof(ushort))
                return Convert.ToUInt16(value);
            if (t == typeof(uint))
                return Convert.ToUInt32(value);
            if (t == typeof(ulong))
                return Convert.ToUInt64(value);
            if (t == typeof(float))
                return Convert.ToSingle(value);
            if (t == typeof(double))
                return Convert.ToDouble(value);
            if (t == typeof(decimal))
                return Convert.ToDecimal(value);
            if (t == typeof(DateTime))
                return Convert.ToDateTime(value);
            if (t == typeof(string))
                return Convert.ToString(value);
            if (t == typeof(char))
                return Convert.ToChar(value);
            if (t.IsSubclassOf(typeof(Enum)))
                return Enum.Parse(t, Convert.ToString(value));

            return value;
        }
        public static bool SetProperty(object targetObject, PropertyInfo property, object value)
        {
            try {
                    property.SetValue(targetObject, GetBasicTypeValue(property.PropertyType, value));
                    return true;

            }
            catch
            {
                return false;
            }
        }

        public static string GetListString<T>(List<T> list, string splitStr = ",", bool keepLastSplitString=false,Func<T, string> toStringFunc = null)
        {
            if (list == null) return "";
            if (toStringFunc == null)
                toStringFunc = (t) => { return t.ToString(); };
            string s = "";
            list.ForEach(v =>
            {
                s += toStringFunc(v) + splitStr;
            });
            if (!keepLastSplitString)
            {
                s = s.Substring(0, s.Length - splitStr.Length);
            }
            return s;
        }

        public static string GetSpellCode(string CnStr)//get first chinese py for chinese string
        {

            string strTemp = "";

            int iLen = CnStr.Length;

            int i = 0;

            for (i = 0; i <= iLen - 1; i++)
            {

                strTemp += GetCharSpellCode(CnStr.Substring(i, 1));

            }

            return strTemp;

        }
        private static string GetCharSpellCode(string CnChar)
        {

            long iCnChar;

            byte[] ZW = System.Text.Encoding.Default.GetBytes(CnChar);

            //如果是字母，则直接返回

            if (ZW.Length == 1)
            {

                return CnChar.ToUpper();

            }

            else {

                // get the array of byte from the single char

                int i1 = (short)(ZW[0]);

                int i2 = (short)(ZW[1]);

                iCnChar = i1 * 256 + i2;

            }

            // iCnChar match the constant

            if ((iCnChar >= 45217) && (iCnChar <= 45252))
            {

                return "A";

            }

            else if ((iCnChar >= 45253) && (iCnChar <= 45760))
            {

                return "B";

            }
            else if ((iCnChar >= 45761) && (iCnChar <= 46317))
            {

                return "C";

            }
            else if ((iCnChar >= 46318) && (iCnChar <= 46825))
            {

                return "D";

            }
            else if ((iCnChar >= 46826) && (iCnChar <= 47009))
            {

                return "E";

            }
            else if ((iCnChar >= 47010) && (iCnChar <= 47296))
            {

                return "F";

            }
            else if ((iCnChar >= 47297) && (iCnChar <= 47613))
            {

                return "G";

            }
            else if ((iCnChar >= 47614) && (iCnChar <= 48118))
            {

                return "H";

            }
            else if ((iCnChar >= 48119) && (iCnChar <= 49061))
            {

                return "J";

            }
            else if ((iCnChar >= 49062) && (iCnChar <= 49323))
            {

                return "K";

            }
            else if ((iCnChar >= 49324) && (iCnChar <= 49895))
            {

                return "L";

            }
            else if ((iCnChar >= 49896) && (iCnChar <= 50370))
            {

                return "M";

            }
            else if ((iCnChar >= 50371) && (iCnChar <= 50613))
            {

                return "N";

            }
            else if ((iCnChar >= 50614) && (iCnChar <= 50621))
            {

                return "O";

            }
            else if ((iCnChar >= 50622) && (iCnChar <= 50905))
            {

                return "P";

            }
            else if ((iCnChar >= 50906) && (iCnChar <= 51386))
            {

                return "Q";

            }
            else if ((iCnChar >= 51387) && (iCnChar <= 51445))
            {

                return "R";

            }
            else if ((iCnChar >= 51446) && (iCnChar <= 52217))
            {

                return "S";

            }
            else if ((iCnChar >= 52218) && (iCnChar <= 52697))
            {

                return "T";

            }
            else if ((iCnChar >= 52698) && (iCnChar <= 52979))
            {

                return "W";

            }
            else if ((iCnChar >= 52980) && (iCnChar <= 53640))
            {

                return "X";

            }
            else if ((iCnChar >= 53689) && (iCnChar <= 54480))
            {

                return "Y";

            }
            else if ((iCnChar >= 54481) && (iCnChar <= 55289))
            {

                return "Z";

            }
            else

                return ("?");

        }
    }
}
