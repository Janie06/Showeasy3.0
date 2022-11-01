using EasyNet.DBUtility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;

namespace EasyNet.Common
{
    public class DbEntityUtils
    {
        public static string GetTableName(Type classType, DbOperateType type)
        {
            var sTableName = string.Empty;
            var sEntityName = string.Empty;

            sEntityName = classType.FullName;

            var attr = classType.GetCustomAttributes(false);
            if (attr.Length == 0) return sTableName;

            foreach (object classAttr in attr)
            {
                if (classAttr is SqlSugar.SugarTable)
                {
                    var tableAttr = classAttr as SqlSugar.SugarTable;
                    sTableName = tableAttr.TableName;
                }
            }

            if (string.IsNullOrEmpty(sTableName) && (type == DbOperateType.INSERT || type == DbOperateType.UPDATE || type == DbOperateType.DELETE))
            {
                throw new Exception("實體類:" + sEntityName + "的屬性配置[Table(name=\"tablename\")]錯誤或未配置");
            }

            return sTableName;
        }

        public static string GetPrimaryKey(object attribute, DbOperateType type)
        {
            var strPrimary = string.Empty;
            var attr = attribute as SqlSugar.SugarColumn;
            if (type == DbOperateType.INSERT)
            {
                if (!attr.IsIdentity)
                {
                    strPrimary = Guid.NewGuid().ToString();
                }
            }
            else
            {
                strPrimary = attr.ColumnName;
            }

            return strPrimary;
        }

        public static string GetColumnName(object attribute)
        {
            var columnName = string.Empty;
            if (attribute is SqlSugar.SugarColumn)
            {
                var columnAttr = attribute as SqlSugar.SugarColumn;
                columnName = columnAttr.ColumnName;
            }

            return columnName;
        }

        public static TableInfo GetTableInfo(object entity, DbOperateType dbOpType, PropertyInfo[] properties)
        {
            var breakForeach = false;
            var strPrimaryKey = string.Empty;
            var tableInfo = new TableInfo();
            var type = entity.GetType();

            tableInfo.TableName = GetTableName(type, dbOpType);
            if (dbOpType == DbOperateType.COUNT)
            {
                return tableInfo;
            }

            foreach (PropertyInfo property in properties)
            {
                object propvalue = null;
                var columnName = string.Empty;
                var propName = columnName = property.Name;

                propvalue = ReflectionHelper.GetPropertyValue(entity, property);

                var propertyAttrs = property.GetCustomAttributes(false);

                if (columnName == "RowIndex") { continue; }
                for (int i = 0; i < propertyAttrs.Length; i++)
                {
                    var propertyAttr = propertyAttrs[i];
                    if (DbEntityUtils.IsCaseColumn(propertyAttr, dbOpType))
                    {
                        breakForeach = true; break;
                    }

                    var tempVal = GetColumnName(propertyAttr);
                    columnName = string.IsNullOrEmpty(tempVal) ? propName : tempVal;

                    if (propertyAttr is SqlSugar.SugarColumn)
                    {
                        var oId = new IdInfo
                        {
                            Key = columnName
                        };
                        if (dbOpType == DbOperateType.INSERT || dbOpType == DbOperateType.DELETE)
                        {
                            var idAttr = propertyAttr as SqlSugar.SugarColumn;
                            tableInfo.Strategy = idAttr.IsIdentity;
                            oId.Strategy = idAttr.IsIdentity;
                            if (CommonUtils.IsNullOrEmpty(propvalue))
                            {
                                strPrimaryKey = DbEntityUtils.GetPrimaryKey(propertyAttr, dbOpType);
                                if (!string.IsNullOrEmpty(strPrimaryKey))
                                    propvalue = strPrimaryKey;
                            }
                        }
                        oId.Value = propvalue;

                        if (tableInfo.Id.Key == null)
                        {
                            tableInfo.Id = oId;
                        }

                        tableInfo.Ids.Add(oId);
                        tableInfo.Keycolumns.Put(propName, columnName);
                        tableInfo.PropToColumn.Put(propName, columnName);
                        breakForeach = true;
                    }
                }

                //if (breakForeach && dbOpType == DbOperateType.DELETE) break;
                if (breakForeach) { breakForeach = false; continue; }
                tableInfo.Columns.Put(columnName, propvalue);
                tableInfo.PropToColumn.Put(propName, columnName);
            }

            if (dbOpType == DbOperateType.UPDATE)
            {
                tableInfo.Columns.Put(tableInfo.Id.Key, tableInfo.Id.Value);
            }

            return tableInfo;
        }

        public static List<T> ToList<T>(IDataReader sdr, TableInfo tableInfo, PropertyInfo[] properties) where T : new()
        {
            var list = new List<T>();

            while (sdr.Read())
            {
                var entity = new T();
                foreach (PropertyInfo property in properties)
                {
                    if (property.Name == EasyNetGlobalConstWord.ROWINDEX) continue;

                    var name = tableInfo.PropToColumn[property.Name].ToString();
                    if (tableInfo.TableName == string.Empty)
                    {
                        if (DbEntityUtils.IsCaseColumn(property, DbOperateType.SELECT)) continue;
                        ReflectionHelper.SetPropertyValue(entity, property, sdr[name]);
                        continue;
                    }

                    ReflectionHelper.SetPropertyValue(entity, property, sdr[name]);
                }
                list.Add(entity);
            }

            return list;
        }

        public static List<T> ToList<T>(IDataReader sdr) where T : new()
        {
            var list = new List<T>();
            var properties = ReflectionHelper.GetProperties(new T().GetType());

            while (sdr.Read())
            {
                var entity = new T();
                foreach (PropertyInfo property in properties)
                {
                    var name = property.Name;

                    if (name == EasyNetGlobalConstWord.ROWINDEX) continue;

                    ReflectionHelper.SetPropertyValue(entity, property, sdr[name]);
                }
                list.Add(entity);
            }

            return list;
        }

        public static List<T> ToListForQuery<T>(IDataReader sdr) where T : new()
        {
            var list = new List<T>();
            var properties = ReflectionHelper.GetProperties(new T().GetType());

            while (sdr.Read())
            {
                var entity = new T();

                for (int index = 0; index < sdr.FieldCount; index++)
                {
                    var name = sdr.GetName(index);
                    var property = ReflectionHelper.GetProperty(properties, name);
                    if (property == null) continue;
                    ReflectionHelper.SetPropertyValue(entity, property, sdr.GetValue(index));
                }
                list.Add(entity);
            }

            return list;
        }

        public static List<Dictionary<string, object>> ToListForQuery(IDataReader sdr, PropertyInfo[] properties)
        {
            var list = new List<Dictionary<string, object>>();

            while (sdr.Read())
            {
                var dic = new Dictionary<string, object>();

                for (int index = 0; index < sdr.FieldCount; index++)
                {
                    var name = sdr.GetName(index);
                    var property = ReflectionHelper.GetProperty(properties, name);
                    if (property == null) continue;
                    dic.Add(property.Name, sdr.GetValue(index));
                }
                list.Add(dic);
            }

            return list;
        }

        public static string GetQuerySql(TableInfo tableInfo, DbCondition condition)
        {
            var sbColumns = new StringBuilder();

            tableInfo.Columns.Put(tableInfo.Id.Key, tableInfo.Id.Value);
            foreach (String key in tableInfo.Columns.Keys)
            {
                var nKey = DbKeywords.FormatColumnName(key.Trim());
                sbColumns.Append(nKey).Append(",");
            }

            if (sbColumns.Length > 0) sbColumns.Remove(sbColumns.ToString().Length - 1, 1);

            var sSqls = String.Empty;
            if (String.IsNullOrEmpty(condition.queryString))
            {
                sSqls = "SELECT {0} FROM {1}";
                sSqls = string.Format(sSqls, sbColumns.ToString(), GetFullTableName(tableInfo));
                sSqls += condition.ToString();
            }
            else
            {
                sSqls = condition.ToString();
            }

            sSqls = sSqls.ToUpper();

            return sSqls;
        }

        public static string GetQueryAllSql(TableInfo tableInfo)
        {
            var sbColumns = new StringBuilder();

            tableInfo.Columns.Put(tableInfo.Id.Key, tableInfo.Id.Value);
            foreach (String key in tableInfo.Columns.Keys)
            {
                var nKey = DbKeywords.FormatColumnName(key.Trim());
                sbColumns.Append(nKey).Append(",");
            }

            if (sbColumns.Length > 0) sbColumns.Remove(sbColumns.ToString().Length - 1, 1);

            var sSqls = "SELECT {0} FROM {1}";
            sSqls = string.Format(sSqls, sbColumns.ToString(), GetFullTableName(tableInfo));

            return sSqls;
        }

        public static string GetQueryByIdSql(TableInfo tableInfo)
        {
            var sbColumns = new StringBuilder();

            //if (tableInfo.Columns.ContainsKey(tableInfo.Id.Key))
            //    tableInfo.Columns[tableInfo.Id.Key] = tableInfo.Id.Value;
            //else
            //    tableInfo.Columns.Put(tableInfo.Id.Key, tableInfo.Id.Value);

            foreach (String key in tableInfo.PropToColumn.Keys)
            {
                var nKey = DbKeywords.FormatColumnName(key.Trim());
                sbColumns.Append(nKey).Append(",");
            }

            if (sbColumns.Length > 0) sbColumns.Remove(sbColumns.ToString().Length - 1, 1);

            var sSqls = "SELECT {0} FROM {1} WHERE {2} = " + AdoHelper.DbParmChar + "{2}";
            sSqls = string.Format(sSqls, sbColumns.ToString(), GetFullTableName(tableInfo), tableInfo.Id.Key);

            return sSqls;
        }

        public static string GetQueryByFieldSql(TableInfo tableInfo, string field)
        {
            var sbColumns = new StringBuilder();

            foreach (String key in tableInfo.PropToColumn.Keys)
            {
                var nKey = DbKeywords.FormatColumnName(key.Trim());
                sbColumns.Append(nKey).Append(",");
            }

            if (sbColumns.Length > 0) sbColumns.Remove(sbColumns.ToString().Length - 1, 1);

            var sSqls = "SELECT {0} FROM {1} WHERE {2} = " + AdoHelper.DbParmChar + "{2}";
            sSqls = string.Format(sSqls, sbColumns.ToString(), GetFullTableName(tableInfo), field);

            return sSqls;
        }

        public static string GetQueryByObjSql(TableInfo tableInfo, ref Dictionary<string, object> wh, Dictionary<string, object> sort = null)
        {
            var sbColumns = new StringBuilder();
            var sWheres = new StringBuilder();
            var sSorts = new StringBuilder();

            foreach (String key in tableInfo.PropToColumn.Keys)
            {
                var nKey = DbKeywords.FormatColumnName(key.Trim());
                sbColumns.Append(nKey).Append(",");
            }
            if (wh != null)
            {
                var newWh = new Dictionary<string, object>();
                var pm_wh = new ParamMap();
                pm_wh.Paramters(wh);
                foreach (string key in pm_wh.Keys)
                {
                    if (key != EasyNetGlobalConstWord.SORT)
                    {
                        var sParmChar = new StringBuilder();
                        var sKey = "";
                        object sVal = "";
                        var nKey = DbKeywords.FormatColumnName(key.Trim());
                        switch (nKey)
                        {
                            case "_OR_":
                                sParmChar = GetWhsOrChid(key, pm_wh, ref sKey, ref sVal);
                                var oOr = sVal as Object[];
                                foreach (Object jo in oOr)
                                {
                                    var dic = jo as Dictionary<string, object>;
                                    foreach (string keyNew in dic.Keys)
                                    {
                                        if (!newWh.ContainsKey(keyNew))
                                        {
                                            newWh.Add(keyNew, dic[keyNew]);
                                        }
                                    }
                                }
                                break;

                            case "_AND_":
                                sParmChar = GetWhsAndChid(key, pm_wh, ref sKey, ref sVal);
                                if (sVal != null && sVal.ToString() != "")
                                {
                                    var oAnd = sVal as Object[];
                                    foreach (Object jo in oAnd)
                                    {
                                        if (jo != null)
                                        {
                                            var dic = jo as Dictionary<string, object>;
                                            foreach (string keyNew in dic.Keys)
                                            {
                                                if (!newWh.ContainsKey(keyNew))
                                                {
                                                    newWh.Add(keyNew, dic[keyNew]);
                                                }
                                            }
                                        }
                                    }
                                }
                                break;

                            default:
                                sParmChar = GetWhsAnd(key, pm_wh, ref sKey, ref sVal);
                                newWh.Add(sKey, sVal);
                                break;
                        }
                        if (sParmChar.Length > 0)
                        {
                            sWheres.Append(sParmChar).Append(" AND ");
                        }
                    }
                }
                wh = newWh;
            }

            if (sort != null)
            {
                foreach (string key in sort.Keys)
                {
                    var nKey = DbKeywords.FormatColumnName(key.Trim());
                    sSorts.Append(nKey).Append(" " + sort[nKey]).Append(",");
                }
            }

            if (sbColumns.Length > 0) sbColumns.Remove(sbColumns.ToString().Length - 1, 1);
            if (sWheres.Length > 0) sWheres.Remove(sWheres.ToString().Length - 4, 4);
            if (sSorts.Length > 0) sSorts.Remove(sSorts.ToString().Length - 1, 1);

            var sSqls = "SELECT {0} FROM {1}" + ((sWheres.Length == 0) ? "" : " WHERE {2} ") + ((sSorts.Length == 0) ? "" : " ORDER BY {3} ");
            sSqls = string.Format(sSqls, sbColumns.ToString(), GetFullTableName(tableInfo), sWheres.ToString(), sSorts.ToString());

            //if (sWheres.Length == 0) sSqls.Remove(sSqls.ToString().Length - 8, 8);

            return sSqls;
        }

        public static string GetQueryCountSql(TableInfo tableInfo)
        {
            var sbColumns = new StringBuilder();

            var sSqls = "SELECT COUNT(0) FROM {1} ";
            sSqls = string.Format(sSqls, sbColumns.ToString(), GetFullTableName(tableInfo));

            foreach (String key in tableInfo.Columns.Keys)
            {
                var nKey = DbKeywords.FormatColumnName(key.Trim());
                sbColumns.Append(nKey).Append("=").Append(AdoHelper.DbParmChar).Append(key);
            }

            if (sbColumns.Length > 0)
            {
                sSqls += " WHERE " + sbColumns.ToString();
            }

            return sSqls;
        }

        public static string GetQueryCountSql(TableInfo tableInfo, DbCondition condition)
        {
            var sSqls = "SELECT COUNT(0) FROM {0}";
            sSqls = string.Format(sSqls, GetFullTableName(tableInfo));
            sSqls += condition.ToString();

            return sSqls;
        }

        public static string GetQueryByPropertySql(TableInfo tableInfo)
        {
            var sbColumns = new StringBuilder();

            tableInfo.Columns.Put(tableInfo.Id.Key, tableInfo.Id.Value);
            foreach (String key in tableInfo.Columns.Keys)
            {
                var nKey = DbKeywords.FormatColumnName(key.Trim());
                sbColumns.Append(nKey).Append(",");
            }

            if (sbColumns.Length > 0) sbColumns.Remove(sbColumns.ToString().Length - 1, 1);

            var sSqls = "SELECT {0} FROM {1} WHERE {2} = " + AdoHelper.DbParmChar + "{2}";
            sSqls = string.Format(sSqls, sbColumns.ToString(), GetFullTableName(tableInfo), tableInfo.Id.Key);

            return sSqls;
        }

        public static string GetQueryByPage(TableInfo tableInfo, ref ParamMap pm)
        {
            var sbColumns = new StringBuilder();
            var sbWhs = new StringBuilder();

            //tableInfo.Columns.Put(tableInfo.Id.Key, tableInfo.Id.Value);
            foreach (String key in tableInfo.PropToColumn.Keys)
            {
                var nKey = DbKeywords.FormatColumnName(key.Trim());
                sbColumns.Append(nKey).Append(",");
            }
            if (pm.Keys.Count > 0)
            {
                var newPm = new ParamMap();
                var BRemoveOr = false;
                var BRemoveAnd = false;
                foreach (string key in pm.Keys)
                {
                    if (key != EasyNetGlobalConstWord.SORT)
                    {
                        var sParmChar = new StringBuilder();
                        var sKey = "";
                        object sVal = "";
                        if (key == "pageStart" || key == "pageEnd")
                        {
                            continue;
                        }
                        switch (key)
                        {
                            case "_OR_":
                                sParmChar = GetWhsOrChid(key, pm, ref sKey, ref sVal);
                                var oOr = sVal as Object[];
                                foreach (Object jo in oOr)
                                {
                                    var dic = jo as Dictionary<string, object>;
                                    foreach (string keyNew in dic.Keys)
                                    {
                                        if (!newPm.ContainsKey(keyNew))
                                        {
                                            newPm.Put(keyNew, dic[keyNew]);
                                        }
                                    }
                                }
                                BRemoveOr = true;
                                break;

                            case "_AND_":
                                sParmChar = GetWhsAndChid(key, pm, ref sKey, ref sVal);
                                if (sVal != null && sVal.ToString() != "")
                                {
                                    var oAnd = sVal as Object[];
                                    foreach (Object jo in oAnd)
                                    {
                                        if (jo != null)
                                        {
                                            var dic = jo as Dictionary<string, object>;
                                            foreach (string keyNew in dic.Keys)
                                            {
                                                if (!newPm.ContainsKey(keyNew))
                                                {
                                                    newPm.Put(keyNew, dic[keyNew]);
                                                }
                                            }
                                        }
                                    }
                                }
                                BRemoveAnd = true;
                                break;

                            default:
                                sParmChar = GetWhsAnd(key, pm, ref sKey, ref sVal);
                                newPm.Put(sKey, sVal);
                                break;
                        }
                        if (sParmChar.Length > 0)
                        {
                            sbWhs.Append(sParmChar).Append(" AND ");
                        }
                    }
                }

                if (BRemoveOr)
                {
                    pm.Remove("_OR_");
                }

                if (BRemoveAnd)
                {
                    pm.Remove("_AND_");
                }

                foreach (String key in newPm.Keys)
                {
                    pm.Put(key, newPm[key]);
                }
                //pm = newPm;
            }

            if (sbColumns.Length > 0) sbColumns.Remove(sbColumns.ToString().Length - 1, 1);

            var sSqls = "SELECT {0} FROM {1}";

            if (sbWhs.Length > 0)
            {
                sbWhs.Remove(sbWhs.ToString().Length - 5, 5);
                sSqls += " WHERE {2}";
            }

            sSqls = string.Format(sSqls, sbColumns.ToString(), GetFullTableName(tableInfo), sbWhs);

            return sSqls;
        }

        public static string GetAutoSql()
        {
            var autoSQL = "";
            if (AdoHelper.DbType == DatabaseType.SQLSERVER)
            {
                autoSQL = " select scope_identity() as AutoId ";
            }

            if (AdoHelper.DbType == DatabaseType.ACCESS)
            {
                autoSQL = " select @@IDENTITY as AutoId ";
            }

            if (AdoHelper.DbType == DatabaseType.MYSQL)
            {
                autoSQL = " ;select @@identity ";
            }

            return autoSQL;
        }

        public static string GetFullTableName(TableInfo tbinfo)
        {
            var sTableName = "";
            if (AdoHelper.DbType == DatabaseType.SQLSERVER)
            {
                sTableName = tbinfo.TableName;
            }

            if (AdoHelper.DbType == DatabaseType.ACCESS)
            {
            }

            if (AdoHelper.DbType == DatabaseType.MYSQL)
            {
            }

            return sTableName;
        }

        public static string GetInsertSql(TableInfo tableInfo)
        {
            var sbColumns = new StringBuilder();
            var sbValues = new StringBuilder();
            foreach (IdInfo Id in tableInfo.Ids)
            {
                if (!Id.Strategy)
                {
                    if (!Id.Strategy && tableInfo.Id.Value == null)
                    {
                        Id.Value = Guid.NewGuid().ToString();
                    }

                    if (Id.Value != null)
                    {
                        tableInfo.Columns.Put(Id.Key, Id.Value);
                    }
                }
            }

            foreach (String key in tableInfo.Columns.Keys)
            {
                var value = tableInfo.Columns[key];
                if (!string.IsNullOrEmpty(key.Trim()) && value != null)
                {
                    var nKey = DbKeywords.FormatColumnName(key.Trim());
                    sbColumns.Append(nKey).Append(",");
                    sbValues.Append(AdoHelper.DbParmChar).Append(key).Append(",");
                }
            }

            if (sbColumns.Length > 0 && sbValues.Length > 0)
            {
                sbColumns.Remove(sbColumns.ToString().Length - 1, 1);
                sbValues.Remove(sbValues.ToString().Length - 1, 1);
            }

            var sSqls = "INSERT INTO {0}({1}) VALUES({2})";
            sSqls = string.Format(sSqls, GetFullTableName(tableInfo), sbColumns.ToString(), sbValues.ToString());

            //if (tableInfo.Strategy == GenerationType.INDENTITY && (AdoHelper.DbType == DatabaseType.SQLSERVER || AdoHelper.DbType == DatabaseType.MYSQL))
            //{
            //    string autoSql = DbEntityUtils.GetAutoSql();
            //    sSqls = sSqls + autoSql;
            //}

            return sSqls;
        }

        public static string GetUpdateSql(TableInfo tableInfo)
        {
            var sbBody = new StringBuilder();

            foreach (String key in tableInfo.Columns.Keys)
            {
                var value = tableInfo.Columns[key];
                if (IsCaseColumn(tableInfo, key)) continue;
                if (!string.IsNullOrEmpty(key.Trim()) && value != null)
                {
                    var nKey = DbKeywords.FormatColumnName(key.Trim());
                    sbBody.Append(nKey).Append("=").Append(AdoHelper.DbParmChar + key).Append(",");
                }
            }

            if (sbBody.Length > 0) sbBody.Remove(sbBody.ToString().Length - 1, 1);

            tableInfo.Columns.Put(tableInfo.Id.Key, tableInfo.Id.Value);

            var sSqls = "update {0} set {1} where {2} =" + AdoHelper.DbParmChar + tableInfo.Id.Key;
            sSqls = string.Format(sSqls, GetFullTableName(tableInfo), sbBody.ToString(), tableInfo.Id.Key);

            return sSqls;
        }

        public static string GetDeleteByIdSql(TableInfo tableInfo)
        {
            var sSql = "delete from {0} where {1}";
            var sWheres = new StringBuilder();

            foreach (IdInfo oId in tableInfo.Ids)
            {
                sWheres.Append(oId.Key).Append(" = " + AdoHelper.DbParmChar + oId.Key).Append(" and ");
            }
            if (sWheres.Length > 0) sWheres.Remove(sWheres.ToString().Length - 4, 4);

            sSql = string.Format(sSql, GetFullTableName(tableInfo), sWheres);

            return sSql;
        }

        public static string GetDeleteByFieldSql(TableInfo tableInfo, string FieldName)
        {
            var sSqls = "delete from {0} where {1} =" + AdoHelper.DbParmChar + FieldName;
            sSqls = string.Format(sSqls, GetFullTableName(tableInfo), FieldName);

            return sSqls;
        }

        public static string GetInsertByObjSql(TableInfo tableInfo, Dictionary<string, object> val)
        {
            var sbColumns = new StringBuilder();
            var sbValues = new StringBuilder();

            foreach (IdInfo Id in tableInfo.Ids)
            {
                var sGuid = Guid.NewGuid().ToString();

                if (Id.Key != null && Id.Strategy)
                {
                    continue;
                }

                if (val.ContainsKey(Id.Key))
                {
                    if (val[Id.Key] == null || val[Id.Key].ToString() == "")
                    {
                        val[Id.Key] = sGuid;
                    }
                }
                else
                {
                    val.Add(Id.Key, sGuid);
                }
            }

            foreach (String key in val.Keys)
            {
                var nKey = DbKeywords.FormatColumnName(key.Trim());
                if (tableInfo.PropToColumn.ContainsKey(key))
                {
                    sbColumns.Append(nKey).Append(",");
                    sbValues.Append(AdoHelper.DbParmChar).Append(key).Append(",");
                }
            }

            if (sbColumns.Length > 0 && sbValues.Length > 0)
            {
                sbColumns.Remove(sbColumns.ToString().Length - 1, 1);
                sbValues.Remove(sbValues.ToString().Length - 1, 1);
            }

            var sSqls = "INSERT INTO {0}({1}) VALUES({2})";
            sSqls = string.Format(sSqls, GetFullTableName(tableInfo), sbColumns.ToString(), sbValues.ToString());
            return sSqls;
        }

        public static string GetUpdateByObjSql(TableInfo tableInfo, ref Dictionary<string, object> val, Dictionary<string, object> wh)
        {
            var dic = new Dictionary<string, object>();
            var sBody = new StringBuilder();
            var sWheres = new StringBuilder();

            foreach (String key in val.Keys)
            {
                if (IsCaseColumn(tableInfo, key) || wh.ContainsKey(key)) continue;

                var nKey = DbKeywords.FormatColumnName(key.Trim());
                sBody.Append(nKey).Append("=").Append(AdoHelper.DbParmChar + key).Append(",");
                dic.Add(key, val[key]);
            }
            foreach (String key in wh.Keys)
            {
                var nKey = DbKeywords.FormatColumnName(key.Trim());
                sWheres.Append(nKey).Append(" = " + AdoHelper.DbParmChar + nKey).Append(" and ");
            }

            if (sBody.Length > 0) sBody.Remove(sBody.ToString().Length - 1, 1);
            if (sWheres.Length > 0) sWheres.Remove(sWheres.ToString().Length - 4, 4);

            var sSql = "update {0} set {1} where {2}";
            sSql = string.Format(sSql, GetFullTableName(tableInfo), sBody, sWheres);
            val = dic;
            return sSql;
        }

        public static string GetDeleteByObjSql(TableInfo tableInfo, object parms)
        {
            var sWheres = new StringBuilder();
            var childparms = parms as Dictionary<string, object>;
            var pm_wh = new ParamMap();
            pm_wh.Paramters(childparms);
            foreach (String key in childparms.Keys)
            {
                var sParmChar = new StringBuilder();
                var sKey = DbKeywords.FormatColumnName(key.Trim());
                object sVal = null;
                //sWheres.Append(nKey).Append(" = " + AdoHelper.DbParmChar + nKey).Append(" and ");
                sParmChar = GetWhsAnd(key, pm_wh, ref sKey, ref sVal);
                if (sParmChar.Length > 0)
                {
                    sWheres.Append(sParmChar).Append(" AND ");
                }
            }
            if (sWheres.Length > 0) sWheres.Remove(sWheres.ToString().Length - 4, 4);

            //string sSqls = "SELECT count(0) FROM {0} WHERE {1}" + System.Environment.NewLine;
            var sSqls = "delete from {0} where {1}";
            sSqls = string.Format(sSqls, GetFullTableName(tableInfo), sWheres);

            return sSqls;
        }

        public static void SetParameters(Map columns, params IDbDataParameter[] parms)
        {
            var i = 0;
            foreach (string key in columns.Keys)
            {
                if (!string.IsNullOrEmpty(key.Trim()))
                {
                    var value = columns[key];
                    if (value == null) value = DBNull.Value;
                    parms[i].ParameterName = key;
                    parms[i].Value = value;
                    i++;
                }
            }
        }

        public static bool IsCaseColumn(object attribute, DbOperateType dbOperateType)
        {
            if (attribute is SqlSugar.SugarColumn)
            {
                var columnAttr = attribute as SqlSugar.SugarColumn;
                if (columnAttr.IsIgnore)
                {
                    return true;
                }
                if (!columnAttr.IsOnlyIgnoreInsert && dbOperateType == DbOperateType.INSERT)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool IsCaseColumn(PropertyInfo property, DbOperateType dbOperateType)
        {
            var isBreak = false;
            var propertyAttrs = property.GetCustomAttributes(false);
            foreach (object propertyAttr in propertyAttrs)
            {
                if (DbEntityUtils.IsCaseColumn(propertyAttr, dbOperateType))
                {
                    isBreak = true; break;
                }
            }

            return isBreak;
        }

        public static bool IsCaseColumn(TableInfo tableInfo, string col)
        {
            var isBreak = false;
            do
            {
                if (col == "RowIndex" || col == "CreateUser" || col == "CreateDate" || col == tableInfo.Id.Key)
                {
                    isBreak = true; break;
                }
                foreach (IdInfo Id in tableInfo.Ids)
                {
                    if (Id.Key == col)
                    {
                        isBreak = true; break;
                    }
                }
            }
            while (false);

            return isBreak;
        }

        private static StringBuilder GetWhsAnd(string sKey, ParamMap pm, ref string r_sKey, ref object r_sVal, string dkey = null)
        {
            var sParmChar = new StringBuilder();
            do
            {
                var nKey = sKey.Trim();
                var pmKey = nKey;
                if (dkey != null)
                {
                    pmKey = dkey.Trim();
                }
                var sValue = pm[pmKey].ToString().Trim();
                var sColType = "";
                if (nKey.StartsWith("INCLUDNULL_"))
                {
                    sColType = "includenull";
                    nKey = nKey.Replace("INCLUDNULL_", "");
                }
                else if (nKey.StartsWith("INCLUDNULLINT_"))
                {
                    sColType = "includenullint";
                    nKey = nKey.Replace("INCLUDNULLINT_", "");
                }
                else if (nKey.StartsWith("ISNULL_"))
                {
                    sColType = "isnull";
                    nKey = nKey.Replace("ISNULL_", "");
                }
                else if (nKey.StartsWith("ISBLANK_"))
                {
                    sColType = "isblank";
                    nKey = nKey.Replace("ISBLANK_", "");
                }
                else if (nKey.StartsWith("CHARINDEX_"))
                {
                    sColType = "_charindex";
                    nKey = nKey.Replace("CHARINDEX_", "");
                }
                else if (nKey.StartsWith("_CHARINDEX_"))
                {
                    sColType = "charindex_";
                    nKey = nKey.Replace("_CHARINDEX_", "");
                }
                else if (nKey.StartsWith("IN_"))
                {
                    sColType = "in";
                    nKey = nKey.Replace("IN_", "");
                }
                if (nKey.IndexOf("+") > -1)
                {
                    sColType = "multicolumn";
                    nKey = nKey.Split('+')[0].ToString().Trim();
                }
                nKey = DbKeywords.FormatColumnName(nKey);
                var sCn = " = ";

                if (-1 != sValue.IndexOf("|"))
                {
                }
                else if (sValue.StartsWith("=="))
                {
                }
                else if (sValue.StartsWith(">="))
                {
                    sCn = " >= ";
                    sValue = sValue.Replace(">=", "").Trim();
                    sColType = "gt_eq";
                }
                else if (sValue.StartsWith("<="))
                {
                    sCn = " <= ";
                    sValue = sValue.Replace("<=", "").Trim();
                    sColType = "lt_eq";
                }
                else if (sValue.StartsWith(">>="))
                {
                    sCn = " >= ";
                    sValue = sValue.Replace(">>=", "").Trim();
                    sColType = "agt_eq";
                }
                else if (sValue.StartsWith("<<="))
                {
                    sCn = " <= ";
                    sValue = sValue.Replace("<<=", "").Trim();
                    sColType = "alt_eq";
                }
                else if (sValue.StartsWith("<>") || sValue.StartsWith("!="))
                {
                    sCn = " <> ";
                    sValue = sValue.Replace("<>", "").Replace("!=", "").Trim();
                }
                else if (sValue.StartsWith(">"))
                {
                    sCn = " > ";
                    sValue = sValue.Replace(">", "").Trim();
                }
                else if (sValue.StartsWith("<"))
                {
                    sCn = " < ";
                    sValue = sValue.Replace("<", "").Trim();
                }
                else if (-1 != sValue.IndexOf(".."))
                {
                }
                else if (sValue.StartsWith("%"))
                {
                    sCn = " LIKE ";
                }

                if (sCn == " <> ")
                {
                    sParmChar.Append(nKey + sCn + AdoHelper.DbParmChar + pmKey);
                }
                else if (sColType == "isnull")
                {
                    sParmChar.Append(nKey + " IS NULL ");
                }
                else if (sColType == "isblank")
                {
                    sParmChar.Append("ISNULL(" + nKey + ",'')" + " = " + AdoHelper.DbParmChar + pmKey);
                }
                else if (sValue != "")
                {
                    switch (sColType)
                    {
                        case "gt_eq":
                        case "lt_eq":
                            sParmChar.Append("(" + nKey + sCn + AdoHelper.DbParmChar + pmKey + " OR ISNULL(" + nKey + ",'')=''" + ")");

                            break;

                        case "agt_eq":
                        case "alt_eq":
                            sParmChar.Append("(" + nKey + sCn + AdoHelper.DbParmChar + pmKey + ")");

                            break;

                        case "includenull":
                            sParmChar.Append("ISNULL(" + nKey + ",'')" + sCn + AdoHelper.DbParmChar + pmKey);

                            break;

                        case "includenullint":
                            sParmChar.Append("ISNULL(" + nKey + ",0)" + sCn + AdoHelper.DbParmChar + pmKey);

                            break;

                        case "multicolumn":
                            sParmChar.Append(sKey + sCn + AdoHelper.DbParmChar + pmKey);

                            break;

                        case "_charindex":
                            sParmChar.Append("CHARINDEX(" + AdoHelper.DbParmChar + pmKey + "," + nKey + ") > 0");

                            break;

                        case "charindex_":
                            sParmChar.Append("CHARINDEX(" + nKey + "," + AdoHelper.DbParmChar + pmKey + ") > 0");

                            break;

                        case "in":
                            sParmChar.Append(nKey + " IN (" + AdoHelper.DbParmChar + pmKey + ")");

                            break;

                        default:
                            sParmChar.Append(nKey + sCn + AdoHelper.DbParmChar + pmKey);

                            break;
                    }
                }
                r_sKey = pmKey;
                r_sVal = sValue;
            }
            while (false);
            return sParmChar;
        }

        private static StringBuilder GetWhsOrChid(string sKey, ParamMap pm, ref string r_sKey, ref object r_oVal)
        {
            var sParmChar = new StringBuilder();
            sParmChar.Append("(");
            do
            {
                var nKey = sKey.Trim();
                var oVal = pm[sKey];
                var saPm = oVal as Object[];
                var oValNew = new Object[saPm.Length];
                var iIndex = 0;
                foreach (Object jo in saPm)
                {
                    var dic = jo as Dictionary<string, object>;
                    var dicNew = new Dictionary<string, object>();
                    var pm_wh = new ParamMap();
                    pm_wh.Paramters(dic);
                    sParmChar.Append("(");
                    foreach (string key in dic.Keys)
                    {
                        var sKeyNew = "";
                        object sValNew = "";
                        var nKeyNew = DbKeywords.FormatColumnName(key.Trim());
                        var sPmChar = GetWhsAnd(key, pm_wh, ref sKeyNew, ref sValNew);
                        if (sPmChar.Length > 0)
                        {
                            sParmChar.Append(sPmChar + " AND ");
                        }
                        dicNew.Add(sKeyNew, sValNew);
                    }
                    sParmChar.Remove(sParmChar.Length - 4, 4);
                    sParmChar.Append(") OR ");
                    oValNew[iIndex] = dicNew;
                    iIndex++;
                }
                sParmChar.Remove(sParmChar.Length - 3, 3);
                sParmChar.Append(")");
                r_sKey = nKey;
                r_oVal = oValNew;
            }
            while (false);
            return sParmChar;
        }

        private static StringBuilder GetWhsAndChid(string sKey, ParamMap pm, ref string r_sKey, ref object r_oVal)
        {
            var sParmChar = new StringBuilder();
            sParmChar.Append("(");
            do
            {
                var nKey = sKey.Trim();
                var oVal = pm[sKey];
                var saPm = oVal as Object[];
                var oValNew = new Object[saPm.Length];
                var iIndex = 0;
                foreach (Object jo in saPm)
                {
                    var dic = jo as Dictionary<string, object>;
                    var dicNew = new Dictionary<string, object>();
                    var pm_wh = new ParamMap();
                    var key = dic["key"].ToString().Trim();
                    var name = dic["name"].ToString().Trim();
                    var value = dic["value"];
                    pm_wh.Add(name, value);
                    var sKeyNew = "";
                    object sValNew = "";
                    var nKeyNew = DbKeywords.FormatColumnName(key);
                    var sPmChar = GetWhsAnd(key, pm_wh, ref sKeyNew, ref sValNew, name);
                    dicNew.Add(sKeyNew, sValNew);
                    if (sPmChar.Length == 0)
                    {
                        continue;
                    }
                    sParmChar.Append(sPmChar + " AND ");
                    oValNew[iIndex] = dicNew;
                    iIndex++;
                }
                if (sParmChar.Length == 1) { sParmChar = new StringBuilder(); break; }
                sParmChar.Remove(sParmChar.Length - 4, 4);
                sParmChar.Append(")");
                r_sKey = nKey;
                r_oVal = oValNew;
            }
            while (false);
            return sParmChar;
        }
    }
}