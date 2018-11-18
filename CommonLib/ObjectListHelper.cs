using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib
{
    public class ObjectListHelper
    {
        public static List<string> GetValidTypePropertyList(Type t,Type IgnoreAttributeClass=null)
        {
            var sl = new List<string>();
            var apl = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if(IgnoreAttributeClass!=null)
                apl = apl.Where(v => v.GetCustomAttribute(IgnoreAttributeClass) == null).ToArray();
            foreach (var p in apl)
            {
                sl.Add(p.Name);
            }
            return sl;
        }

        public static string GetValidTypePropertyCommarString(Type t, Type IgnoreAttributeClass = null)
        {
            var sl = GetValidTypePropertyList(t, IgnoreAttributeClass);
            var s = "";
            sl.ForEach(v => s += (v + ","));
            return s;
        }
    }
}
