using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using System.Data.SqlClient;
using System.Configuration;
using CommonLib;
using System.Reflection;
using System.Data;
using System.Data.Common;

namespace ReportCommonLib
{
    public class DatabaseDataProvider
    {
        public Func<DbConnection> GetDatabaseConnection { get; set; }
        public DbConnection GetConnection(string connectionStr = null)
        {
            if (connectionStr != null)
                return new SqlConnection(connectionStr);
            if (GetDatabaseConnection != null)
                return GetDatabaseConnection();
            throw new Exception("No valid database connection creater for db access!");
        }

        int _TimeoutMs = 3000;
        public int TimeoutMs
        {
            get
            {
                if (ConfigurationManager.AppSettings["TimeoutMs"] != null)
                {
                    var str = ConfigurationManager.AppSettings["TimeoutMs"].ToString();
                    if (string.IsNullOrEmpty(str))
                        return _TimeoutMs;
                    try
                    {
                        var ts = Convert.ToInt32(str);
                        _TimeoutMs = ts;
                    }
                    catch
                    { }
                }
                return _TimeoutMs;

            }
            set { _TimeoutMs = value; }
        }

        public List<T> GetQueryResult<T>(string sql, string connectionStr)
        {

            using (var conn = GetConnection(connectionStr))
            {
                conn.Open();
                return conn.Query<T>(sql, new { }, null, true, TimeoutMs).ToList();
            }
        }

        public List<T> GetQueryResult<T>(string sql, string connectionStr, object parameterArray)
        {

            using (var conn = GetConnection(connectionStr))
            {
                conn.Open();
                return conn.Query<T>(sql, parameterArray, null, true, TimeoutMs).ToList();
            }
        }
        public bool DeleteBatch(string connectionStr, string tableName, List<string> ColumnList, List<object> objectList, object parameterArray = null, bool useTransaction = false)
        {
            var result = true;
            var operationCount = objectList.Count / MaxModifyPageSize + 1;
            for (int i = 0; i < operationCount; i++)
            {

                var operateList = objectList.Skip(i * MaxModifyPageSize).Take(MaxModifyPageSize).ToList();
                if (operateList.Count > 0)
                {
                    var sql = CreateDeleteSql(tableName, operateList, ColumnList);
                    result = DoSql(connectionStr, sql, parameterArray, useTransaction) && result;
                }
            }
            return result;


        }

        public bool DeleteBatch(string connectionStr, string tableName, string uidName, List<object> objectList, object parameterArray = null, bool useTransaction = false)
        {
            var sql = CreateDeleteSql(tableName, objectList, uidName);
            IDbTransaction tran = null;
            var result = false;
            int flag = 0;
            using (var conn = GetConnection(connectionStr))
            {
                conn.Open();
                if (useTransaction)
                    tran = conn.BeginTransaction();
                try
                {
                    flag = conn.Execute(sql, parameterArray, tran, TimeoutMs);
                }
                catch (Exception ex)
                {
                    LogSupport.Error(ex);
                    if (tran != null)
                        tran.Rollback();
                    return false;
                }
                if (tran != null)
                {
                    try
                    {
                        tran.Commit();
                        result = true;
                    }
                    catch
                    {
                        tran.Rollback();
                    }
                }
                else if (flag > 0)
                    result = true;
                return result;
            }

        }
        int _MaxModifyPageSize = 500;
        public int MaxModifyPageSize
        {
            get { return _MaxModifyPageSize; }
            set { _MaxModifyPageSize = value; }
        }
        bool DoSql(string connectionStr, string sql, object parameterArray = null, bool useTransaction = false)
        {
            if (string.IsNullOrEmpty(sql))
                throw new Exception("no valid sql!");

            IDbTransaction tran = null;
            var result = false;
            int flag = 0;
            using (var conn = GetConnection(connectionStr))
            {
                conn.Open();
                if (useTransaction)
                    tran = conn.BeginTransaction();

                flag = conn.Execute(sql, parameterArray, tran, TimeoutMs);
                if (tran != null)
                {
                    try
                    {
                        tran.Commit();
                        result = true;
                    }
                    catch
                    {
                        tran.Rollback();
                    }
                }
                else if (flag > 0)
                    result = true;
                return result;
            }

        }

        public bool InsertBatch(string connectionStr, string tableName, List<object> objectList, object parameterArray = null, bool useTransaction = false)
        {
            var result = true;
            var operationCount = objectList.Count / MaxModifyPageSize + 1;
            for (int i = 0; i < operationCount; i++)
            {

                var operateList = objectList.Skip(i * MaxModifyPageSize).Take(MaxModifyPageSize).ToList();
                if (operateList.Count > 0)
                {
                    var sql = CreateInertSql(tableName, operateList);
                    result = DoSql(connectionStr, sql, parameterArray, useTransaction) && result;
                }
            }
            return result;


        }
        public string CreateUpdateSql(string tbName, object data, string conditions, Type dataType = null)
        {
            if (data == null) return null;
            if (dataType == null)
                dataType = data.GetType();
            if (string.IsNullOrEmpty(conditions)) return null;
            StringBuilder sql = new StringBuilder();
            sql.Append(string.Format("Update {0} ", tbName));

            var sl = ObjectListHelper.GetValidTypePropertyList(dataType, typeof(ReportOutputIgnore));
            var vs = "";

            vs += "(";
            for (int i = 0; i < sl.Count; i++)
            {
                var cs = sl[i];
                if (IsColumnNameNeedBracket(cs))
                    cs = "[" + cs + "]";
                var s = cs + "=";
                var valueString = "";
                var value = dataType.GetProperty(sl[i]).GetValue(data);
                if (value == null)
                    valueString = valueString + GetDefaultValue(data.GetType().GetProperty(sl[i]));
                else
                    valueString = value.ToString();
                if (IsNeedQuotation(data.GetType().GetProperty(sl[i])))
                    valueString = "'" + valueString + "'";

                s = s + valueString;
                if (i != sl.Count - 1)
                {
                    s += ",";
                }
                vs += s;
            }

            vs += ")";


            sql.Append(vs);
            conditions = conditions.Trim();
            if (!conditions.StartsWith("where", StringComparison.CurrentCultureIgnoreCase))
                conditions = " where " + conditions;
            else
                conditions = " " + conditions;
            sql.Append(conditions);
            return sql.ToString();

        }

        public string CreateInertSql(string tbName, object data, Type dataType = null)
        {
            if (data == null) return null;
            if (dataType == null)
                dataType = data.GetType();
            StringBuilder sql = new StringBuilder();
            sql.Append(string.Format("INSERT INTO {0}(", tbName));
            var sl = ObjectListHelper.GetValidTypePropertyList(dataType, typeof(ReportOutputIgnore));
            var hs = "";
            sl.ForEach(v => hs += (v + ","));

            hs = FixKeyWordColumn(hs);
            sql.Append(hs);

            sql.Append(")");
            sql.Append(" VALUES ");

            var vs = "";

            vs += "(";
            for (int i = 0; i < sl.Count; i++)
            {
                var valueString = "";
                var value = dataType.GetProperty(sl[i]).GetValue(data);
                if (value == null)
                    valueString = GetDefaultValue(data.GetType().GetProperty(sl[i]));
                else
                    valueString = value.ToString();
                if (IsNeedQuotation(data.GetType().GetProperty(sl[i])))
                    valueString = "'" + valueString + "'";
                if (i != sl.Count - 1)
                {
                    valueString += ",";
                }
                vs += valueString;
            }

            vs += ")";


            sql.Append(vs);
            return sql.ToString();

        }
        public string CreateDeleteSql(string tbName, List<object> ObjectList, List<string> ColumnList)
        {
            if (ObjectList == null || ObjectList.Count == 0) return null;
            var o = ObjectList.FirstOrDefault();
            var objType = o.GetType();

            //bool needQuotation = IsNeedQuotation(objType.GetProperty(uidName));
            var sql = "";
            var s = "Delete from " + tbName;
            if (ColumnList == null || ColumnList.Count == 0)
                return s;

            ObjectList.ForEach(v =>
            {
                List<string> cs = new List<string>();
                ColumnList.ForEach(c =>
                {
                    var valueString = "";
                    var value = v.GetType().GetProperty(c).GetValue(v);
                    if (value == null)
                        valueString = GetDefaultValue(v.GetType().GetProperty(c));
                    else
                        valueString = value.ToString();

                    //vs = vs + uidName + "=";
                    if (IsNeedQuotation(objType.GetProperty(c)))
                        valueString = "'" + valueString + "'";
                    cs.Add(c + "=" + valueString);
                });
                var conditionString = "";
                cs.ForEach(c =>
                {
                    conditionString = conditionString + c + " and ";
                });
                conditionString = conditionString.Substring(0, conditionString.Length - 5);
                sql = sql + s + " where " + conditionString + ";";
            });


            return sql;

        }

        public string CreateDeleteSql(string tbName, List<object> ObjectList, string uidName)
        {
            if (ObjectList == null || ObjectList.Count == 0) return null;
            var o = ObjectList.FirstOrDefault();
            var objType = o.GetType();
            StringBuilder sql = new StringBuilder();
            bool needQuotation = IsNeedQuotation(objType.GetProperty(uidName));

            sql.Append(string.Format("Delete from {0} ", tbName));

            sql.Append(" where ");

            var vs = uidName + " in (";
            ObjectList.ForEach(v =>
            {
                var valueString = "";
                var value = v.GetType().GetProperty(uidName).GetValue(v);
                if (value == null)
                    valueString = GetDefaultValue(v.GetType().GetProperty(uidName));
                else
                    valueString = value.ToString();

                //vs = vs + uidName + "=";
                if (needQuotation)
                    vs = vs + "'" + value + "'";

                vs = vs + ",";



            });
            vs = vs.Substring(0, vs.Length - 1);
            vs += ")";
            sql.Append(vs);
            return sql.ToString();

        }

        public string CreateInertSql(string tbName, List<object> ObjectList)
        {
            if (ObjectList == null || ObjectList.Count == 0) return null;
            var o = ObjectList.FirstOrDefault();
            var objType = o.GetType();
            StringBuilder sql = new StringBuilder();


            sql.Append(string.Format("INSERT INTO {0}(", tbName));
            var sl = ObjectListHelper.GetValidTypePropertyList(objType, typeof(ReportOutputIgnore));
            var hs = "";
            sl.ForEach(v => hs += (v + ","));
            hs = FixKeyWordColumn(hs);
            sql.Append(hs);

            sql.Append(")");
            sql.Append(" VALUES ");
            var vs = "";
            ObjectList.ForEach(v =>
            {
                try
                {
                    vs += "(";
                    for (int i = 0; i < sl.Count; i++)
                    {
                        var valueString = "";
                        var value = v.GetType().GetProperty(sl[i]).GetValue(v);
                        if (value == null)
                            valueString = GetDefaultValue(v.GetType().GetProperty(sl[i]));
                        else
                            valueString = value.ToString();
                        if (IsNeedQuotation(v.GetType().GetProperty(sl[i])))
                            valueString = "'" + valueString + "'";
                        if (i != sl.Count - 1)
                        {
                            valueString += ",";
                        }
                        vs += valueString;
                    }

                    vs += "),";
                }
                catch (Exception ex)
                {
                    LogSupport.Error(ex);
                }
            });

            sql.Append(vs.Substring(0, vs.Length - 1));
            return sql.ToString();

        }
        public string GetDefaultValue(PropertyInfo property)
        {
            if (property.PropertyType == typeof(string)) return "";
            if (property.PropertyType == typeof(DateTime)) return "1900-1-1";
            return "0";
        }
        public bool IsNeedQuotation(PropertyInfo property)
        {
            if (property.PropertyType == typeof(string)) return true;
            if (property.PropertyType == typeof(DateTime)) return true;
            return false;
        }
        List<string> KeyWordsList = new List<string>() { "Open", "Close", "Key", "Time", "Date" };
        public bool IsColumnNameNeedBracket(string columnName)
        {
            if (KeyWordsList.Any(v => v.Equals(columnName, StringComparison.CurrentCultureIgnoreCase)))
                return true;
            return false;
        }
        public string FixKeyWordColumn(string sourceString)
        {
            var s = sourceString;
            if (!s.StartsWith(","))
                s = "," + s;
            if (!s.EndsWith(","))
                s = s + ",";
            KeyWordsList.ForEach(v =>
            {
                if (s.Contains("," + v + ","))
                    s = s.Replace("," + v + ",", ",[" + v + "],");
            });
            return s.Substring(1, s.Length - 2);
        }
    }
}
