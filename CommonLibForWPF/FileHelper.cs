using CommonLib;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CommonLibForWPF
{
    public class FileHelper
    {
        public static void SaveToFile(object o, string filter = null)
        {
            try
            {
                SaveFileDialog f = new SaveFileDialog();
                if (filter == null)
                    f.Filter = "Text Files(*.txt)|*.txt|All Files(*.*)|*.*";
                else
                    f.Filter = filter;
                if (f.ShowDialog().Value)
                {
                    CommonProc.SaveObjToFile(o, f.FileName);
                }
            }
            catch (Exception ex)
            {

                throw ex;
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
                if (f.ShowDialog().Value)
                {
                    return CommonProc.LoadObjFromFile<T>(f.FileName);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("load fail");
                LogSupport.Error(e);
            }
            return default(T);
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
                if (f.ShowDialog().Value)
                {
                    using (var tw = File.CreateText(f.FileName))
                        tw.Write(s);
                }
            }
            catch (Exception ex)
            {
                LogSupport.Error(ex);
                throw ex;
            }
        }

        public static string LoadStringFromFile(string filter = null)
        {
            OpenFileDialog f = new OpenFileDialog();
            if (filter == null)
                f.Filter = "Text Files(*.txt)|*.txt|All Files(*.*)|*.*";
            else
                f.Filter = filter;
            try
            {
                if (f.ShowDialog().Value)
                {
                    var s = File.ReadAllText(f.FileName);
                    return s;

                }
            }
            catch (Exception e)
            {
                MessageBox.Show("load fail");
                LogSupport.Error(e);
            }
            return null;
        }

    }
}
