using ClosedXML.Excel;
using CommonLib;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace ReportCommonLib
{
    public class ReportExportHelper
    {
        string _HeaderStyle = "";
        public string HeaderStyle
        {
            get { return _HeaderStyle; }
            set { _HeaderStyle = value; }
        }
        string _ValueStyle = "";
        public string ValueStyle
        {
            get { return _ValueStyle; }
            set { _ValueStyle = value; }
        }

        int _MaxCount = 500;
        public int MaxCount { get { return _MaxCount; } set { _MaxCount = value; } }
        string GetHeader(PropertyInfo p)
        {
            var ha = p.GetCustomAttribute(typeof(ReportOutputAttribute));
            if (ha == null||string.IsNullOrEmpty((ha as ReportOutputAttribute).Header))
            {
                return p.Name;
            }
            else
                return (ha as ReportOutputAttribute).Header;
        }
        string GetValue(PropertyInfo p,object targetObject)
        {
            var v = p.GetValue(targetObject);
            if (v == null) return null;
            var ha = p.GetCustomAttribute(typeof(ReportOutputAttribute));
            if (ha == null || string.IsNullOrEmpty((ha as ReportOutputAttribute).FormatString))
            {

                if (v is DateTime)
                {
                    return Convert.ToDateTime(v).ToShortDateString();
                }
                else if (v is double || v is float || v is decimal)
                {
                    return string.Format("{0:f2}", v);
                }
                else if (v is int || v is long)
                {
                    return string.Format("{0:n0}", v);
                }
                else
                {
                    return v.ToString();
                }

            }
            else
            {
                return string.Format((ha as ReportOutputAttribute).FormatString, v);
            }
        }
        public string GetHTMLTable<T>(List<T> objList)
        {
            var header = new List<string>();
            var apl = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var pl = apl.Where(v => v.GetCustomAttribute(typeof(ReportOutputIgnore)) == null).ToList();
            foreach (var p in pl)
            {
                header.Add(p.Name);
            }

            var s = "";
            s += "<p><table><colgroup><col/><col/></colgroup><tr>";
            foreach (var h in header)
            {
                s += "<th>";
                s += h;
                s += "</th>";
            }
            s += "</tr>";
            int index = 0;
            foreach (var obj in objList)
            {
                if (index >= MaxCount) break;
                s += "<tr>";
                foreach (var h in header)
                {
                    s += "<td>";
                    var vf = typeof(T).GetProperty(h);
                    if (vf != null)
                    {
                        var v= GetValue(vf, obj);
                        if (v != null)
                        {
                            s += v;
                        }
                    }
                    s += "</td>";
                }
                s += "</tr>";
                index++;
            }
            s += "</table></p>";
            return s;
        }
        public string GetHTMLTable(List<object> objList)
        {
            if (objList == null || objList.Count == 0)
                return "";
            var Obj = objList.FirstOrDefault();
            var header = new List<string>();
            var showheader = new List<string>();
            var apl = Obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var pl = apl.Where(v => v.GetCustomAttribute(typeof(ReportOutputIgnore)) == null).ToList();
            foreach (var p in pl)
            {
                header.Add(p.Name);
                showheader.Add(GetHeader(p));

            }

            var s = "";
            s += "<p><table border='0' cellspacing='0' cellpadding='0' style='border-collapse:collapse'><colgroup><col/><col/></colgroup><tr>";
            foreach (var h in showheader)
            {
                s += "<td nowrap='' valign='bottom' style='background:black;padding:0in 5.4pt 0in 5.4pt'><p align='center' style='text-align:center;font-size:11.0pt;font-family:Calibri,sans-serif;'><span style='color:white'>"+h+"<o:p></o:p></span></p>";

                s += "</td>";
            }
            s += "</tr>";
            int index = 0;
            foreach (var obj in objList)
            {
                if (index >= MaxCount) break;
                s += "<tr>";
                foreach (var h in header)
                {
                    s += "<td nowrap='' valign='bottom' style='border:solid #B8CCE4 1.0pt;background:#DCE6F1;padding:0in 5.4pt 0in 5.4pt;height:15.0pt;font-size:11.0pt;font-family:Calibri,sans-serif;'><p align='center'><span style='color:black'>";
                    var vf = Obj.GetType().GetProperty(h);
                    if (vf != null)
                    {
                        var v = GetValue(vf, obj);
                        if (v != null)
                        {
                            s += v;
                        }
                    }
                    s += "</span></p></td>";
                }
                s += "</tr>";
                index++;
            }
            s += "</table></p>";
            return s;
        }

        #region Export CSV
        
        public Dictionary<string,string> LoadMappingFile(string filePath, Encoding code = null)
        {
            if (code == null)
                code = Encoding.UTF8;
            var dic = new Dictionary<string, string>();
            var s = File.ReadAllText(filePath, code);
            var sl = s.Trim().Split('\n');
            sl.ForEach(v =>
            {
                var i = v.IndexOf(":");
                if (i > 0)
                {
                    dic.Add(v.Substring(0, i), v.Substring(i + 1));
                }
            });
            return dic;
        }
        string GetPropertyName(string tag,Dictionary<string,string> mapping = null)
        {
            if (mapping == null)
                return tag;
            if (mapping.ContainsKey(tag))
                return mapping[tag].Trim();
            return null;
        }
        public List<T> LoadFromCsvFile<T>(string filePath, Encoding code= null,char seperateChar=',', Dictionary<string, string> mapping=null,Func<string,string> preProcess=null) where T : new()
        {
            if (code == null)
                code = Encoding.UTF8;
            var tl = new List<T>();
            var s = File.ReadAllText(filePath,code);
            var sl = s.Split('\n');
            if (sl.Length < 2) return tl;
            var pl = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var priorpl = pl.Where(v => v.GetCustomAttribute(typeof(ColumnMapping)) != null).ToList();
            var head = sl[0].Trim();
            var bhl = head.Split(seperateChar);
            var hlist = new List<string>();
            foreach(var h in bhl)
            {
                hlist.Add(GetPropertyName(h, mapping));
            }

            for(int i = 1; i < sl.Length; i++)
            {
                var t = new T();
                var record = sl[i].Trim();
                if (!string.IsNullOrEmpty(record))
                {
                    var cl = record.Split(seperateChar);
                    if (cl.Length == hlist.Count)
                    {
                        for (int j = 0; j < hlist.Count; j++)
                        {
                            if (!string.IsNullOrEmpty(hlist[j]))
                            {
                                var cp = priorpl.FirstOrDefault(c => c.Name == hlist[j] || c.GetCustomAttribute<ColumnMapping>().Title == hlist[j]);
                                if (cp == null)
                                    cp = pl.FirstOrDefault(c => c.Name == hlist[j]);
                                if (cp != null)
                                {
                                    var value = cl[j];
                                    if (preProcess != null)
                                    {
                                        value = preProcess(cl[j]);
                                    }
                                    CommonProc.SetProperty(t, cp, value);
                                }
                            }
                        }
                        tl.Add(t);
                    }
                }
            }
            return tl;
        }
        public void CreateCsvReport<T>(string filePath, List<T> rl, Encoding code = null)
        {
            if (code == null)
                code = Encoding.UTF8;
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
                File.WriteAllText(filePath, GetCVS<T>(rl),code);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public string GetCVS<T>(List<T> objList)
        {
            if (objList == null) return "";
            var sl = ObjectListHelper.GetValidTypePropertyList(typeof(T), typeof(ReportOutputIgnore));
            var s = "";
            sl.ForEach(v => s += (v + ","));
            s += "\r\n";
            objList.ForEach(v =>
            {
                foreach (var p in sl)
                {
                    var value = v.GetType().GetProperty(p).GetValue(v);
                    if (value != null)
                        s += value.ToString() + ",";
                    else
                    {
                        s += ",";
                    }
                }
                s += "\r\n";
            });
            return s;
        }
        #endregion

        #region Excel output
        public void CreateExcelReport<T>(string filePath, Action<Workbook> fiilData, string templatePath = null)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
            if (!string.IsNullOrEmpty(templatePath) && !File.Exists(templatePath))
                throw new Exception("No template file : " + templatePath);
            else
                File.Copy(templatePath, filePath);
            var xlApp = new Microsoft.Office.Interop.Excel.Application();
            Workbook xlWorkBook=null;
            try
            {
                xlApp.DisplayAlerts = false;
                xlWorkBook = xlApp.Application.Workbooks.Open(filePath);

                fiilData(xlWorkBook);

                xlWorkBook.Save();
                xlWorkBook.Close(true, null, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (xlWorkBook != null)
                    xlWorkBook.Close(true, null, null);
                Thread.Sleep(5000);
                if (xlApp != null)
                    xlApp.Quit();
                Thread.Sleep(5000);

            }
        }
        public void CreateExcelReport<T>(string filePath,List<T> rl, string templatePath=null)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
            if (!string.IsNullOrEmpty(templatePath) && !File.Exists(templatePath))
                throw new Exception("No template file : " + templatePath);
            else
                File.Copy(templatePath, filePath);
            var xlApp = new Microsoft.Office.Interop.Excel.Application();
            Workbook xlWorkBook=null;
            try
            {
                xlApp.DisplayAlerts = false;
                xlWorkBook = xlApp.Application.Workbooks.Open(filePath);

                var reportSheet = (Worksheet)xlWorkBook.Worksheets["Sheet"];


                FillInfoToSheet(reportSheet, rl);

                xlWorkBook.Save();
                xlWorkBook.Close(true, null, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (xlWorkBook != null)
                    xlWorkBook.Close(true, null, null);
                Thread.Sleep(5000);
                if (xlApp != null)
                    xlApp.Quit();
                Thread.Sleep(5000);

            }
        }
        public void CreateExcelReport<T>(string filePath,Dictionary<string,List<T>> rl, string templatePath = null)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
            if (!string.IsNullOrEmpty(templatePath))
            {
                if (!File.Exists(templatePath))
                    throw new Exception("No template file : " + templatePath);
                else
                    File.Copy(templatePath, filePath);
            }
            var xlApp = new Microsoft.Office.Interop.Excel.Application();
            Workbook xlWorkBook=null;
            try
            {
                xlApp.DisplayAlerts = false;
                xlWorkBook = xlApp.Application.Workbooks.Add(System.Reflection.Missing.Value);
                var firstSheet= (Worksheet)xlWorkBook.Worksheets[1];
                foreach (var kv in rl)
                {
                    var reportSheet = (Worksheet)xlWorkBook.Worksheets.Add();
                    reportSheet.Name = kv.Key;
                    FillInfoToSheet<T>(reportSheet, kv.Value);
                    reportSheet.Move(firstSheet);
                }
                firstSheet.Delete();
                ((Worksheet)xlWorkBook.Worksheets[1]).Select();
                xlWorkBook.SaveAs(filePath);
                xlWorkBook.Close(true, null, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (xlWorkBook != null)
                    xlWorkBook.Close(true, null, null);
                Thread.Sleep(5000);
                if (xlApp != null)
                    xlApp.Quit();
                Thread.Sleep(5000);

            }
        }
        public void CreateExcelReport(string filePath, Dictionary<string, List<object>> rl, string templatePath = null)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);
            if (!string.IsNullOrEmpty(templatePath))
            {
                if (!File.Exists(templatePath))
                    throw new Exception("No template file : " + templatePath);
                else
                    File.Copy(templatePath, filePath);
            }
            var xlApp = new Microsoft.Office.Interop.Excel.Application();
            Workbook xlWorkBook=null;
            try
            {
                xlApp.DisplayAlerts = false;
                xlWorkBook = xlApp.Application.Workbooks.Add(System.Reflection.Missing.Value);
                var firstSheet = (Worksheet)xlWorkBook.Worksheets[1];
                foreach (var kv in rl)
                {
                    var reportSheet = (Worksheet)xlWorkBook.Worksheets.Add();
                    reportSheet.Name = kv.Key;
                    FillInfoToSheet(reportSheet, kv.Value);
                    reportSheet.Move(firstSheet);
                }
                firstSheet.Delete();
                ((Worksheet)xlWorkBook.Worksheets[1]).Select();
                xlWorkBook.SaveAs(filePath);
                xlWorkBook.Close(true, null, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (xlWorkBook != null)
                    xlWorkBook.Close(true, null, null);
                Thread.Sleep(5000);
                if (xlApp != null)
                    xlApp.Quit();
                Thread.Sleep(5000);

            }
        }
        public void FillInfoToSheet(Worksheet sheet, List<object> vl)
        {
            if (vl == null || vl.Count == 0)
                return;
            var fv = vl.FirstOrDefault();
            if (fv == null)
                return;
            var apl = fv.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var pl = apl.Where(v => v.GetCustomAttribute(typeof(ReportOutputIgnore)) == null).ToList();
            int baseRow = 2;
            int col = 1;
            pl.ForEach(v =>
            {

                (sheet.Cells[1, col++] as Microsoft.Office.Interop.Excel.Range).Value2 =GetHeader(v);
            });
            SetHeaderStyle(sheet.get_Range(sheet.Cells[1, 1] as Microsoft.Office.Interop.Excel.Range,
                        sheet.Cells[1, pl.Count] as Microsoft.Office.Interop.Excel.Range));
            for (int i = 0; i < vl.Count; i++)
            {
                col = 1;
                pl.ForEach(v =>
                {
                    (sheet.Cells[i + baseRow, col++] as Microsoft.Office.Interop.Excel.Range).Value2 = v.GetValue(vl[i]);
                    if (v.GetValue(vl[i]) is DateTime)
                    {
                        (sheet.Cells[i + baseRow, col - 1] as Microsoft.Office.Interop.Excel.Range).NumberFormat = "yyyy-mm-dd";
                    }
                    if (v.GetValue(vl[i]) is double || v.GetValue(vl[i]) is float || v.GetValue(vl[i]) is decimal)
                    {
                        (sheet.Cells[i + baseRow, col - 1] as Microsoft.Office.Interop.Excel.Range).NumberFormat = "#,##0.00";
                    }

                });

                if (i % 2 == 1)
                    SetAlterStyle(sheet.get_Range(sheet.Cells[i + 2, 1] as Microsoft.Office.Interop.Excel.Range,
                        sheet.Cells[i + 2, pl.Count] as Microsoft.Office.Interop.Excel.Range));
            }

        }

        public void FillInfoToSheet<T>(Worksheet sheet, List<T> vl)
        {
            var apl = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var pl = apl.Where(v => v.GetCustomAttribute(typeof(ReportOutputIgnore)) == null).ToList();
            int baseRow = 2;
            int col = 1;
            pl.ForEach(v =>
            {

                (sheet.Cells[1, col++] as Microsoft.Office.Interop.Excel.Range).Value2 = GetHeader(v);
            });
            SetHeaderStyle(sheet.get_Range(sheet.Cells[1, 1] as Microsoft.Office.Interop.Excel.Range,
                        sheet.Cells[1, pl.Count] as Microsoft.Office.Interop.Excel.Range));
            for (int i = 0; i < vl.Count; i++)
            {
                col = 1;
                pl.ForEach(v =>
                {
                    (sheet.Cells[i + baseRow, col++] as Microsoft.Office.Interop.Excel.Range).Value2 = v.GetValue(vl[i]);
                    if (v.GetValue(vl[i]) is DateTime)
                    {
                        (sheet.Cells[i + baseRow, col - 1] as Microsoft.Office.Interop.Excel.Range).NumberFormat = @"yyyy-mm-dd";
                    }
                    if (v.GetValue(vl[i]) is double || v.GetValue(vl[i]) is float || v.GetValue(vl[i]) is decimal)
                    {
                        (sheet.Cells[i + baseRow, col - 1] as Microsoft.Office.Interop.Excel.Range).NumberFormat = "#,##0.00";
                    }
                });

                if (i % 2 == 1)
                    SetAlterStyle(sheet.get_Range(sheet.Cells[i + 2, 1] as Microsoft.Office.Interop.Excel.Range,
                        sheet.Cells[i + 2, pl.Count] as Microsoft.Office.Interop.Excel.Range));
            }

        }
        public void SetHeaderStyle(Microsoft.Office.Interop.Excel.Range r)
        {
            r.Cells.Interior.Color = System.Drawing.Color.Black;
            r.Font.Color = System.Drawing.Color.White;
            r.Font.Bold = true;

        }
        public void SetAlignmentRight(Microsoft.Office.Interop.Excel.Range r)
        {
            r.HorizontalAlignment = XlHAlign.xlHAlignRight;
        }
        public void SetAlterStyle(Microsoft.Office.Interop.Excel.Range r)
        {
            r.Interior.Color = System.Drawing.Color.FromArgb(197, 217, 241);
        }
        public void SetHeaderAlignmentRight(Worksheet sheet, System.Drawing.Point startPoint, int cols, List<string> names)
        {
            for (int i = 0; i < cols; i++)
            {
                var r = (sheet.Cells[startPoint.Y, startPoint.X + i] as Microsoft.Office.Interop.Excel.Range);
                if (r != null && r.Value2 != null && names.Contains(r.Value2.ToString()))
                    SetAlignmentRight(r);
            }
        }
        public System.Data.DataTable GetDataTable(List<Object> vl)
        {
            if (vl == null || vl.Count == 0)
                return null;
            var fv = vl.FirstOrDefault();
            if (fv == null)
                return null; 
            var apl = fv.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var pl = apl.Where(v => v.GetCustomAttribute(typeof(ReportOutputIgnore)) == null).ToList();
            if (pl == null || pl.Count == 0)
                return null;
            var table = new System.Data.DataTable();
            pl.ForEach(v =>
            {
                table.Columns.Add(v.Name, v.PropertyType);
            });
            vl.ForEach(v =>
            {
                var objArray = new object[pl.Count];
                for (int i = 0; i < pl.Count; i++)
                    objArray[i] = pl[i].GetValue(v);
                table.Rows.Add(objArray);
            });

            return table;

        }
        public object[,] GetDataArray(List<Object> vl)
        {
            if (vl == null || vl.Count == 0)
                return null;
            var fv = vl.FirstOrDefault();
            if (fv == null)
                return null;
            var apl = fv.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var pl = apl.Where(v => v.GetCustomAttribute(typeof(ReportOutputIgnore)) == null).ToList();
            if (pl == null || pl.Count == 0)
                return null;
            var dl = new object[vl.Count, pl.Count];
            for(int i=0;i<vl.Count;i++){
                for (int j = 0; j < pl.Count; j++)
                {
                    dl[i, j] = pl[j].GetValue(vl[i]);
                }
            }
            return dl;
        }
        public Range PopulateWorksheet(Worksheet sheet, string[] headers, object[,] data, bool[,] highlighted = null, int startRow = 1, bool lockHeader = true)
        {
            var rowCount = data.GetLength(0);
            var columnCount = data.GetLength(1);
            if (rowCount == 0 || columnCount == 0)
                return null;

            var leftTop = (Range)sheet.Cells[startRow, 1];
            var rightBottom = (Range)sheet.Cells[startRow, headers.Length];
            var range = sheet.Range[leftTop, rightBottom];
            range.Font.Bold = true;
            range.Interior.Color = ColorTranslator.ToOle(Color.LightYellow);
            range.Value2 = headers;

            if (lockHeader)
            {
                sheet.Application.ActiveWindow.SplitRow = 1;
                sheet.Application.ActiveWindow.FreezePanes = true;
            }

            leftTop = sheet.Cells[startRow + 1, 1];
            rightBottom = (Range)sheet.Cells[startRow + rowCount, columnCount];
            range = sheet.Range[leftTop, rightBottom];
            range.Value2 = data;

            if (highlighted != null)
            {
                for (var i = 0; i < rowCount; i++)
                    for (var j = 0; j < columnCount; j++)
                    {
                        if (highlighted[i, j])
                        {
                            ((Range)sheet.Cells[startRow + i + 1, j + 1]).Interior.Color =
                                ColorTranslator.ToOle(Color.Yellow);
                        }
                    }
            }

            sheet.Columns.AutoFit();
            return range;
        }
        public Range PopulateWorksheet(Worksheet sheet, object[,] data, int startRow=1, int startColumn=1)
        {
            var rowCount = data.GetLength(0);
            var columnCount = data.GetLength(1);
            if (rowCount == 0 || columnCount == 0)
                return null;

            
            var datacol=data.GetUpperBound(data.Rank - 1) + 1;
            var datarow = data.Length/datacol;
            var leftTop = (Range)sheet.Cells[startRow, startColumn];
            var rightBottom = (Range)sheet.Cells[startRow+datarow-1, startColumn+datacol-1];

            var range = sheet.Range[leftTop, rightBottom];
            range.Value2 = data;

            sheet.Columns.AutoFit();
            return range;
        }
        public void PopulateWorksheet(string path,string sheetName, object[,] data, int startRow = 1, int startColumn = 1)
        {
            var xlApp = new Microsoft.Office.Interop.Excel.Application();
            Workbook xlWorkBook=null;
            try
            {
                xlApp.DisplayAlerts = false;
                xlWorkBook = xlApp.Application.Workbooks.Open(path);
                var sheet = (Worksheet)xlWorkBook.Worksheets[sheetName];
                PopulateWorksheet(sheet, data, startRow, startColumn);
                xlWorkBook.Save();
                xlWorkBook.Close(true, null, null);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (xlWorkBook != null)
                    xlWorkBook.Close(true, null, null);
                Thread.Sleep(5000);
                if (xlApp != null)
                    xlApp.Quit();
                Thread.Sleep(5000);

            }

        }
        public void SetPivotCollapse(string filePath, int sheetIndex)
        {
            Microsoft.Office.Interop.Excel.Application xlApp = null;
            Workbook xlWorkBook = null;
            try
            {
                xlApp = new Microsoft.Office.Interop.Excel.Application();
                xlWorkBook = xlApp.Application.Workbooks.Open(filePath);
                Thread.Sleep(5000);
                var sheet = xlWorkBook.Worksheets[sheetIndex];
                foreach (var pivot in sheet.PivotTables)
                {
                    foreach (var f in pivot.PivotFields)
                    {
                        var pf=(f as PivotField);
                        if (pf != null)
                        {
                            try
                            {
                                pf.ShowDetail = false;
                            }
                            catch
                            { 
                            }
                        }

                    }
                }

                xlWorkBook.Save();
                

            }
            catch (Exception ex)
            {
                LogSupport.Error(ex);
            }
            finally
            {
                if (xlWorkBook != null)
                    xlWorkBook.Close(true, null, null);
                Thread.Sleep(5000);
                if (xlApp != null)
                    xlApp.Quit();
                Thread.Sleep(5000);
            }

        }
        public void ExportToHtml(string filePath,int sheetIndex, string path)
        {
            Microsoft.Office.Interop.Excel.Application xlApp = null;
            Workbook xlWorkBook = null;
            try
            {
                xlApp = new Microsoft.Office.Interop.Excel.Application();
                xlWorkBook = xlApp.Application.Workbooks.Open(filePath);
                var sheet = xlWorkBook.Worksheets[sheetIndex];
                object format = Microsoft.Office.Interop.Excel.XlFileFormat.xlHtml;
                sheet.SaveAs(path, format);

            }
            catch (Exception ex)
            {
                LogSupport.Error(ex);
            }
            finally
            {
                if (xlWorkBook != null)
                    xlWorkBook.Close(true, null, null);
                Thread.Sleep(5000);
                if (xlApp != null)
                    xlApp.Quit();
                Thread.Sleep(5000);
            }
            
        }
        string GetSubSheetName(string path,string htmlFileName, int sheetIndex)
        {
            return path + htmlFileName + "_files\\sheet00" + sheetIndex.ToString() + ".html";
        }
        public string GetHtmlFormatSheet(string filePath, int sheetIndex)
        {
            string tempFileName = DateTime.Now.Ticks.ToString()+".html";
            string tempPath = System.AppDomain.CurrentDomain.BaseDirectory;
            ExportToHtml(filePath, sheetIndex, tempPath + tempFileName);
            var subSeetPath = GetSubSheetName(tempPath, tempFileName.Substring(0,tempFileName.Length-5), sheetIndex);
            return File.ReadAllText(subSeetPath);
        }
        public string GetMailContent(string templateHtmlFile, string templateExcelFilePath)
        {
            var s = File.ReadAllText(templateHtmlFile);
            var wb = new XLWorkbook(templateExcelFilePath);
            var sheet = wb.Worksheets.FirstOrDefault();
            s = FillCell(sheet, s);

            s = GetRowString(s,sheet);


            return s;
        }

        public string FillCell(IXLWorksheet sheet, string s)
        {
            var rgx = new Regex("#Cell:(?<cell>\\w+\\d+)#");
            var ml=rgx.Matches(s);

            foreach (var m in ml)
            {
                var g = ((System.Text.RegularExpressions.Match)(m)).Groups;
                var cell = sheet.Cell(g[1].ToString());

                s = Regex.Replace(s, m.ToString(), GetCellValue(cell));
            }

            return s;
        }
        public int ColumnToIndex(string columnName)
        {
            if (!Regex.IsMatch(columnName.ToUpper(), @"[A-Z]+")) { throw new Exception("invalid parameter"); }
            int index = 0;
            char[] chars = columnName.ToUpper().ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                index += ((int)chars[i] - (int)'A' + 1) * (int)Math.Pow(26, chars.Length - i - 1);
            }
            return index ;
        }

        public string IndexToColumn(int index)
        {
            if (index < 0) { throw new Exception("invalid parameter"); }
            List<string> chars = new List<string>();
            do
            {
                if (chars.Count > 0) index--;
                chars.Insert(0, ((char)(index % 26 + (int)'A')).ToString());
                index = (int)((index - index % 26) / 26);
            } while (index > 0);
            return String.Join(string.Empty, chars.ToArray());
        }
        public int GetColumnCount(string startCell, IXLWorksheet sheet)
        {
            var startcol = Regex.Match(startCell, "[a-zA-Z]+");
            int i = GetNumber(startCell);
            int j = ColumnToIndex(startcol.ToString());
            for (int c = j; c <= 1000000; c++)
            {
                if (string.IsNullOrEmpty(GetCellValue(sheet.Cell(i, c))) && string.IsNullOrEmpty(GetCellValue(sheet.Cell(i, c + 1))) && string.IsNullOrEmpty(GetCellValue(sheet.Cell(i, c+2))))
                    return c - 1;
            }
            return 0;
        }
        public int GetNumber(string s)
        {
            var n = Regex.Match(s, "[0-9]+");
            var ns = n.ToString();
            if(ns.StartsWith("{"))
                ns=ns.Substring(1,ns.Length-2);
            return Convert.ToInt32(ns);
        }
        public int GetRowCount(string startCell, IXLWorksheet sheet)
        {
            var col=Regex.Match(startCell, "[a-zA-Z]+");
            int i = GetNumber(startCell);
            while (!string.IsNullOrEmpty(sheet.Cell(col + i.ToString()).Value.ToString())||i>1000000)
            {
                i++;
            }
            return i;
        }
        public string GetRowString(string s, IXLWorksheet sheet)
        {
            var pattan = "#HeaderRow:(?<TableStartCell>\\w+\\d+)#(?<commonheader>[\\s\\S]*?)#RecordRow:(?<StartCell>\\w+\\d+)#(?<commonrow>[\\s\\S]*?)#FooterRow#(?<footer>[\\s\\S]*?)#EndRow#";
            var ml = Regex.Matches(s, pattan);
            foreach (var m in ml)
            {
                
                var g = ((System.Text.RegularExpressions.Match)(m)).Groups;
                var col = Regex.Match(g[1].ToString(), "[a-zA-Z]+").ToString();
                var startcolumn = ColumnToIndex(col);
                var endcolumn = GetHeaderColumns(g[1].ToString(), g[2].ToString(), sheet);
                int hstart = GetNumber(g[1].ToString());
                int rstart = GetNumber(g[3].ToString());
                int hend=rstart-1;
                
                int end = GetRowCount(g[3].ToString(), sheet);

                var fs = g[5].ToString();
                if(fs!=""){
                    end = end - 1;
                }
                var hs = FillRows(g[2].ToString(), hstart, hend, startcolumn, endcolumn, sheet);
                var rs = FillRows(g[4].ToString(), rstart, end, startcolumn, endcolumn, sheet);
                if(!string.IsNullOrEmpty(fs))
                    fs = FillFooterRow(sheet, end+1,startcolumn,endcolumn, fs);
                s = Regex.Replace(s, pattan, hs+rs + fs);

            }
            return s;
        }

        public int GetHeaderColumns(string headerStart, string recordStart, IXLWorksheet sheet)
        {
            var startCell = Regex.Match(headerStart, "[a-z|A-Z]+").ToString();
            var start = GetNumber(headerStart);
            var rstart = GetNumber(recordStart);
            int w = 0;
            for (int i = start; i < rstart; i++)
            {
                var tw = GetColumnCount(startCell+i.ToString(), sheet);
                if (tw > w)
                    w = tw;
            }
            return w;
        }
        public string FillRows(string rowString, int startRow, int endRow, int startColumn, int endColumn, IXLWorksheet sheet)
        {
            string RowString = "";
            var pattern = "<td[\\s\\S]*?>(?<value>#Value#)[\\s\\S]*?</td>";

            var m = Regex.Match(rowString, pattern);
            var tdString = m.Groups[0].ToString();
            for (int i = startRow; i <= endRow; i++)
            {
                string tds = "";
                for (int j = startColumn; j <= endColumn; j++)
                {
                    var cell = sheet.Cell(i, j);
                    var s = Regex.Replace(tdString, "#Value#", GetCellValue(cell));
                    if (Regex.IsMatch(s, "#align#"))
                    {
                        if (cell.Style.Alignment.Horizontal == XLAlignmentHorizontalValues.Center)
                            s = Regex.Replace(s, "#align#", "align='center'");
                        if (cell.Style.Alignment.Horizontal == XLAlignmentHorizontalValues.Left)
                            s = Regex.Replace(s, "#align#", "align='left'");
                        if (cell.Style.Alignment.Horizontal == XLAlignmentHorizontalValues.Right)
                            s = Regex.Replace(s, "#align#", "align='right'");
                        if (cell.Style.Alignment.Horizontal == XLAlignmentHorizontalValues.Justify)
                            s = Regex.Replace(s, "#align#", "align='justify'");
                        if (cell.Style.Alignment.Horizontal == XLAlignmentHorizontalValues.General)
                            s = Regex.Replace(s, "#align#", "align='right'");
                    }
                    if (Regex.IsMatch(s, "#width#"))
                    {
                        var c=sheet.Column(j);
                        var w = c.Width.ToString();
                        s = Regex.Replace(s, "#width#", " width='"+w+"' ");

                    }
                    tds += s;
                }
                RowString += Regex.Replace(rowString, "<td[\\s\\S]*?</td>", tds);
            }
            return RowString;

        }
        public string GetCellValue(IXLCell cell)
        {
            string v = "";
            if (cell == null) return v;
            
            if (!string.IsNullOrEmpty(cell.ValueCached))
                v = cell.ValueCached;
            else
            {
                var rt = cell.RichText;
                if (rt != null)
                    v = Convert.ToString(rt.Text);
                else
                    v = cell.Value.ToString();
            }
            return v;
        }
        public string FillRow(IXLWorksheet sheet, int start,int end,string matchString)
        {
            var index = matchString.IndexOf("<tr");
            matchString = matchString.Substring(index);

            var rgx = new Regex("#Col:(?<col>\\w+)#");
            var ml = rgx.Matches(matchString);
            
            var s = "";

            for (int i = start; i < end; i++)
            {
                var rs = matchString;
                foreach (var m in ml)
                {
                    var g = ((System.Text.RegularExpressions.Match)(m)).Groups;
                    var cell = sheet.Cell(g[1].ToString()+(i).ToString());                    
                    rs = Regex.Replace(rs, m.ToString(), GetCellValue(cell));
                }
                s += rs;
            }

            return s;
        }
        public string FillFooterRow(IXLWorksheet sheet, int end,int startcol,int endcol, string matchString)
        {
            return FillRows(matchString, end, end, startcol, endcol, sheet);
        }

        public void FillList(string templateFilePath,string tableName, List<string> list)
        {
            var wb = new XLWorkbook(templateFilePath);
            var sh = wb.Worksheets.FirstOrDefault();

            var dataTable = wb.NamedRange(tableName);

            list.Clear();

            for (int i = 2; i < 1000000; i++)
            {
                var r = dataTable.Ranges.FirstOrDefault().Row(i);
                if (r != null && r.Cell(1).Value != null && !string.IsNullOrEmpty(r.Cell(1).Value.ToString()))
                {
                    list.Add((r.Cell(1).Value).ToString());
                }
                else
                    return;
            }
        }
        #endregion

        #region Pdf Export //need telerik

        #endregion
    }
    public class ReportOutputAttribute : Attribute
    {
        public string Header { get; set; }
        public Color HeaderFore { get; set; }
        public Color HeaderBachground { get; set; }
        public Color ValueFore { get; set; }
        public Color ValueBackground { get; set; }
        public string FormatString { get; set; }
    }
    public class ReportOutputIgnore : Attribute
    {

    }
    public class ColumnMapping : Attribute
    {
        public string Title { get; set; }
        public ValueType targetType { get; set; }
    }
    public enum ValueType
    {
        valueInt,value,str,datetime,tf
    }
}
