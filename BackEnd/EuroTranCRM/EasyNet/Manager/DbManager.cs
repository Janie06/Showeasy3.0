using EasyNet.Common;
using EasyNet.DBUtility;
using Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace EasyNet.Manager
{
    public class DbManager
    {
        private IDbTransaction m_Transaction;

        private DbManager()
        {
        }

        public static DbManager PriviteInstance()
        {
            var m = new DbManager();
            return m;
        }

        public static DbManager GetCurrentManager()
        {
            var m = ManagerFactory.GetManager();
            return m;
        }

        public static DbManager NewManager()
        {
            var m = new DbManager();
            return m;
        }

        public void BeginTransaction()
        {
            m_Transaction = DbFactory.CreateDbTransaction();
        }

        public void Commit()
        {
            if (m_Transaction != null)
            {
                if (m_Transaction.Connection.State != ConnectionState.Closed)
                {
                    m_Transaction.Commit();
                }
            }
        }

        public void Rollback()
        {
            if (m_Transaction != null)
            {
                if (m_Transaction.Connection.State != ConnectionState.Closed)
                {
                    m_Transaction.Rollback();
                }
            }
        }

        private IDbTransaction GetTransaction()
        {
            if (m_Transaction != null) return m_Transaction;

            return DbFactory.CreateDbTransaction();
        }

        private void Commit(IDbTransaction transaction)
        {
            if (m_Transaction == null && transaction != null)
            {
                if (transaction.Connection.State != ConnectionState.Closed)
                {
                    transaction.Commit();
                }
            }
        }

        private static void Rollback(IDbTransaction transaction)
        {
            if (transaction != null)
            {
                if (transaction.Connection.State != ConnectionState.Closed)
                {
                    transaction.Rollback();
                }
            }
        }

        #region 將實體資料修改到資料庫

        public static int ExecuteSqlTran(Object param)
        {
            var val = 0;
            var lstCommandInfo = new List<CommandInfo>();
            CommandInfo oCommandInfo = null;
            try
            {
                var oActions = param as Dictionary<string, object>;

                foreach (string key in oActions.Keys)
                {
                    if (oActions[key].GetType() == typeof(Object[]))
                    {
                        var saEntity = oActions[key] as Object[];
                        foreach (Object jo in saEntity)
                        {
                            oCommandInfo = new CommandInfo();
                            var oEntity = jo as Dictionary<string, object>;
                            var parameters = new SqlParameter[oEntity.Keys.Count];
                            var iIndx = 0;
                            foreach (string pmkey in oEntity.Keys)
                            {
                                var sp = new SqlParameter
                                {
                                    ParameterName = "@" + pmkey,
                                    Value = oEntity[pmkey]
                                };
                                parameters[iIndx] = sp;
                                iIndx++;
                            }
                            oCommandInfo.Parameters = parameters;
                            oCommandInfo.CommandText = Entity.SqlCommand.GetSqlCommand(key);
                            lstCommandInfo.Add(oCommandInfo);
                        }
                    }
                    else
                    {
                        oCommandInfo = new CommandInfo();
                        var oEntity = oActions[key] as Dictionary<string, object>;
                        var parameters = new SqlParameter[oEntity.Keys.Count];
                        var iIndx = 0;
                        foreach (string pmkey in oEntity.Keys)
                        {
                            var sp = new SqlParameter
                            {
                                ParameterName = "@" + pmkey,
                                Value = oEntity[pmkey]
                            };
                            parameters[iIndx] = sp;
                            iIndx++;
                        }
                        oCommandInfo.Parameters = parameters;
                        oCommandInfo.CommandText = Entity.SqlCommand.GetSqlCommand(key);
                        lstCommandInfo.Add(oCommandInfo);
                    }
                }
                if (AdoHelper.DbType == DatabaseType.ACCESS)
                {
                }
                else
                {
                    val = DbHelperSQL.ExecuteSqlTran(lstCommandInfo);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }

            return val;
        }

        #endregion 將實體資料修改到資料庫

        #region 批量根據參數新增資料

        public int Insert(Object obj)
        {
            IDbTransaction transaction = null;
            var iVal = 0;
            try
            {
                //獲取資料庫連接，如果開啟了事務，從事務中獲取
                transaction = GetTransaction();

                var oEntity = obj as Dictionary<string, object>;

                iVal = ExecuteInsert(oEntity, transaction);

                if (iVal < 0)
                {
                    Rollback(transaction);
                }
                else
                {
                    Commit(transaction);
                }
            }
            catch (Exception e)
            {
                Rollback(transaction);
                throw new Exception(e.Message, e);
            }

            return iVal;
        }

        #endregion 批量根據參數新增資料

        #region 批量根據參數修改資料

        public int Update(Object obj)
        {
            IDbTransaction transaction = null;
            var iVal = 0;
            try
            {
                //獲取資料庫連接，如果開啟了事務，從事務中獲取
                transaction = GetTransaction();
                var oEntity = obj as Dictionary<string, object>;

                iVal = ExecuteUpdate(oEntity, transaction);

                if (iVal < 0)
                {
                    Rollback(transaction);
                }
                else
                {
                    Commit(transaction);
                }
            }
            catch (Exception e)
            {
                Rollback(transaction);
                throw new Exception(e.Message, e);
            }

            return iVal;
        }

        #endregion 批量根據參數修改資料

        #region 批量根據參數刪除資料

        public int Delete(Object obj)
        {
            IDbTransaction transaction = null;
            var iVal = 0;
            try
            {
                //JObject jo = oEntity[key] as JObject;
                //JObject jo = (JObject)JsonConvert.DeserializeObject(oEntity[key]);
                //JObject jo = (JObject)JsonConvert.SerializeObject(oEntity[key]);
                //JObject jo = (JObject)JsonConvert.SerializeObject(oEntity[key], Formatting.Indented);
                //JObject jo = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(oEntity[key], Formatting.Indented));

                //獲取資料庫連接，如果開啟了事務，從事務中獲取
                transaction = GetTransaction();

                var oEntity = obj as Dictionary<string, object>;

                iVal = ExecuteDelete(oEntity, transaction);

                Commit(transaction);
            }
            catch (Exception e)
            {
                Rollback(transaction);
                throw new Exception(e.Message, e);
            }

            return iVal;
        }

        #endregion 批量根據參數刪除資料

        #region 批量根據參數修改資料(MasterDteil)

        public int UpdateTran(Object obj)
        {
            IDbTransaction transaction = null;
            var iVal = 0;
            try
            {
                //獲取資料庫連接，如果開啟了事務，從事務中獲取
                transaction = GetTransaction();

                var oActions = obj as Dictionary<string, object>;

                foreach (string key in oActions.Keys)
                {
                    var oEntity = oActions[key] as Dictionary<string, object>;
                    switch (key)
                    {
                        case EasyNetGlobalConstWord.DEL:
                            iVal += ExecuteDelete(oEntity, transaction);

                            break;

                        case EasyNetGlobalConstWord.ADD:
                            iVal += ExecuteInsert(oEntity, transaction);

                            break;

                        case EasyNetGlobalConstWord.UPD:
                            iVal += ExecuteUpdate(oEntity, transaction);

                            break;

                        default:
                            break;
                    }
                }

                if (iVal < 0)
                {
                    Rollback(transaction);
                }
                else
                {
                    Commit(transaction);
                }
            }
            catch (Exception e)
            {
                Rollback(transaction);
                throw new Exception(e.Message, e);
            }

            return iVal;
        }

        #endregion 批量根據參數修改資料(MasterDteil)

        #region 通過自訂SQL語句查詢記錄數

        public static int Count(string strSql, ParamMap param)
        {
            var count = 0;
            try
            {
                strSql = strSql.ToLower();
                //String columns = SQLBuilderHelper.fetchColumns(strSql);

                if (AdoHelper.DbType == DatabaseType.ACCESS)
                {
                    strSql = SQLBuilderHelper.BuilderSQL(strSql, param.ToDbParameters());
                }

                count = Convert.ToInt32(AdoHelper.ExecuteScalar(AdoHelper.ConnectionString, CommandType.Text, strSql, param.ToDbParameters()));
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }

            return count;
        }

        #endregion 通過自訂SQL語句查詢記錄數

        #region 查询List數據

        public static Object QueryList(string sql, Object obj)
        {
            var oRel = new Object();
            IDataReader sdr = null;
            var entity = new Object();
            var sKey = "";
            try
            {
                do
                {
                    var oEntity = obj as Dictionary<string, object>;
                    Dictionary<string, object> wh = null;
                    Dictionary<string, object> sort = null;
                    if (oEntity.Count == 0)
                    {
                        oRel = QueryBySql(sql, obj, "list");
                        break;
                    }
                    sKey = oEntity.Keys.First();
                    entity = EntityHelper.GetEntity(sKey);

                    if (entity.ToString() == "")
                    {
                        oRel = QueryBySql(sql, obj, "list");
                        break;
                    }
                    var properties = ReflectionHelper.GetProperties(entity.GetType());
                    var tableInfo = DbEntityUtils.GetTableInfo(entity, DbOperateType.SELECT, properties);

                    wh = oEntity[sKey] as Dictionary<string, object>;

                    if (oEntity.Keys.Contains(EasyNetGlobalConstWord.SORT))
                    {
                        sort = oEntity[EasyNetGlobalConstWord.SORT] as Dictionary<string, object>;
                    }

                    if (sql == "")
                    {
                        sql = DbEntityUtils.GetQueryByObjSql(tableInfo, ref wh, sort);
                    }

                    var parms = DbFactory.CreateDbParameters(wh.Keys.Count);

                    var idx = 0;
                    foreach (string _key in wh.Keys)
                    {
                        parms[idx].ParameterName = _key;
                        parms[idx].Value = wh[_key];
                        idx++;
                    }

                    sdr = AdoHelper.ExecuteReader(AdoHelper.ConnectionString, CommandType.Text, sql, parms);
                    var list = new List<Object>();
                    var indx = 0;
                    while (sdr.Read())
                    {
                        entity = EntityHelper.GetEntity(sKey);
                        for (int index = 0; index < sdr.FieldCount; index++)
                        {
                            var name = sdr.GetName(index);
                            var property = ReflectionHelper.GetProperty(properties, name);
                            if (property == null) continue;
                            ReflectionHelper.SetPropertyValue(entity, property, sdr.GetValue(index));
                        }

                        indx++;
                        var sValue = ReflectionHelper.GetPropertyValue(entity, EasyNetGlobalConstWord.ROWINDEX);
                        if ((int)sValue == 0)
                        {
                            var property = ReflectionHelper.GetProperty(properties, EasyNetGlobalConstWord.ROWINDEX);
                            ReflectionHelper.SetPropertyValue(entity, property, indx);
                        }

                        list.Add(entity);
                    }
                    oRel = list;
                }
                while (false);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
            finally
            {
                if (sdr != null)
                {
                    sdr.Close();
                    sdr.Dispose();
                }
            }

            return oRel;
        }

        #endregion 查询List數據

        #region 查询List數據筆數

        public static int QueryCount(Object obj)
        {
            var iCount = 0;
            var entity = new Object();
            var sKey = "";
            try
            {
                do
                {
                    var oEntity = obj as Dictionary<string, object>;
                    Dictionary<string, object> wh = null;

                    sKey = oEntity.Keys.First();
                    entity = EntityHelper.GetEntity(sKey);

                    var properties = ReflectionHelper.GetProperties(entity.GetType());
                    var tableInfo = DbEntityUtils.GetTableInfo(entity, DbOperateType.SELECT, properties);

                    wh = oEntity[sKey] as Dictionary<string, object>;

                    var sSql = "";
                    sSql = DbEntityUtils.GetQueryByObjSql(tableInfo, ref wh);

                    sSql = sSql.ToLower();
                    var countSQL = SQLBuilderHelper.BuilderCountSQL(sSql);

                    var parms = DbFactory.CreateDbParameters(wh.Keys.Count);

                    var idx = 0;
                    foreach (string _key in wh.Keys)
                    {
                        parms[idx].ParameterName = _key;
                        parms[idx].Value = wh[_key];
                        idx++;
                    }

                    iCount = Convert.ToInt32(AdoHelper.ExecuteScalar(AdoHelper.ConnectionString, CommandType.Text, countSQL, parms));
                }
                while (false);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }

            return iCount;
        }

        #endregion 查询List數據筆數

        #region 查询單筆數據

        public static Object QueryOne(string sql, Object obj)
        {
            var entity = new Object();
            IDataReader sdr = null;
            try
            {
                do
                {
                    var oEntity = obj as Dictionary<string, object>;
                    var sKey = oEntity.Keys.First();
                    entity = EntityHelper.GetEntity(sKey);

                    if (entity.ToString() == "")
                    {
                        entity = QueryBySql(sql, obj, "one");
                        break;
                    }

                    var properties = ReflectionHelper.GetProperties(entity.GetType());
                    var tableInfo = DbEntityUtils.GetTableInfo(entity, DbOperateType.SELECT, properties);

                    var wh = oEntity[sKey] as Dictionary<string, object>;

                    if (sql == "")
                    {
                        sql = DbEntityUtils.GetQueryByObjSql(tableInfo, ref wh);
                    }
                    var parms = DbFactory.CreateDbParameters(wh.Keys.Count);

                    var idx = 0;
                    foreach (string _key in wh.Keys)
                    {
                        parms[idx].ParameterName = _key;
                        parms[idx].Value = wh[_key];
                        idx++;
                    }

                    sdr = AdoHelper.ExecuteReader(AdoHelper.ConnectionString, CommandType.Text, sql, parms);
                    while (sdr.Read())
                    {
                        for (int index = 0; index < sdr.FieldCount; index++)
                        {
                            var name = sdr.GetName(index);
                            var property = ReflectionHelper.GetProperty(properties, name);
                            if (property == null) continue;
                            ReflectionHelper.SetPropertyValue(entity, property, sdr.GetValue(index));
                        }
                    }
                }
                while (false);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
            finally
            {
                if (sdr != null)
                {
                    sdr.Close();
                    sdr.Dispose();
                }
            }

            return entity;
        }

        #endregion 查询單筆數據

        #region 查询实体对应表的所有数据

        public static List<T> Query<T>(string sql) where T : new()
        {
            var list = new List<T>();
            var properties = ReflectionHelper.GetProperties(new T().GetType());

            //TableInfo tableInfo = DbEntityUtils.GetTableInfo(new T(), DbOperateType.SELECT, properties);

            IDataReader sdr = null;
            try
            {
                sdr = AdoHelper.ExecuteReader(AdoHelper.ConnectionString, CommandType.Text, sql);
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
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
            finally
            {
                if (sdr != null)
                {
                    sdr.Close();
                    sdr.Dispose();
                }
            }

            return list;
        }

        #endregion 查询实体对应表的所有数据

        #region 分頁查詢返回分頁結果

        public PageResult QueryPage(ParamMap param)
        {
            var pageResult = new PageResult();
            var sSql = "";
            var sEntityKey = param.Entity;
            IDataReader sdr = null;
            IDbConnection connection = null;
            try
            {
                connection = GetConnection();
                Dictionary<string, object> sort = null;
                var sSorts = new StringBuilder();
                var closeConnection = GetWillConnectionState();

                var entity = EntityHelper.GetEntity(sEntityKey);

                var properties = ReflectionHelper.GetProperties(entity.GetType());
                var tableInfo = DbEntityUtils.GetTableInfo(entity, DbOperateType.SELECT, properties);

                if (param.ContainsKey(EasyNetGlobalConstWord.SORT))
                {
                    sort = param[EasyNetGlobalConstWord.SORT] as Dictionary<string, object>;
                }

                sSql = DbEntityUtils.GetQueryByPage(tableInfo, ref param);

                sSql = sSql.ToLower();
                var countSQL = SQLBuilderHelper.BuilderCountSQL(sSql);

                if (param.IsPage && !SQLBuilderHelper.IsPage(sSql))
                {
                    sSql = SQLBuilderHelper.BuilderPageSQL(sSql, param.OrderFields, param.OrderType);
                }

                if (sort != null)
                {
                    foreach (string key in sort.Keys)
                    {
                        var nKey = DbKeywords.FormatColumnName(key.Trim());
                        sSorts.Append(nKey).Append(" " + sort[nKey]).Append(",");
                    }
                }

                if (sSorts.Length > 0)
                {
                    sSorts.Remove(sSorts.ToString().Length - 1, 1);
                    sSql += " ORDER BY {0} ";
                    sSql = string.Format(sSql, sSorts.ToString());
                    param.Remove(EasyNetGlobalConstWord.SORT);
                };

                if (AdoHelper.DbType == DatabaseType.ACCESS)
                {
                    sSql = SQLBuilderHelper.BuilderSQL(entity, sSql, param.ToDbParameters());
                    sdr = AdoHelper.ExecuteReader(closeConnection, connection, CommandType.Text, sSql);
                }
                else
                {
                    sdr = AdoHelper.ExecuteReader(closeConnection, connection, CommandType.Text, sSql, param.ToDbParameters());
                }

                var count = Count(countSQL, param);
                var list = DbEntityUtils.ToListForQuery(sdr, properties);

                pageResult.Total = count;
                pageResult.DataList = list;
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
            finally
            {
                if (sdr != null)
                {
                    sdr.Close();
                    sdr.Dispose();
                }
            }

            return pageResult;
        }

        #endregion 分頁查詢返回分頁結果

        #region 分頁查詢返回分頁結果

        public static PageResult QueryPageByPrc(string sPrcName, Object param, bool bCount)
        {
            var pageResult = new PageResult();
            var iCount = 0;
            var ds = new DataSet();
            try
            {
                var dicQueryPm = param as Dictionary<string, object>;
                var dicQueryPmCount = new Dictionary<string, object>();
                var parameters_List = new SqlParameter[dicQueryPm.Keys.Count];
                var parameters_Count = new SqlParameter[dicQueryPm.Keys.Count - 3];

                var iIndx_List = 0;
                var iIndx_Count = 0;
                foreach (string pmkey in dicQueryPm.Keys)
                {
                    if (pmkey == EasyNetGlobalConstWord.PAGEINDEX)
                    {
                        var sp = new SqlParameter
                        {
                            ParameterName = "@" + EasyNetGlobalConstWord.PAGESTART,
                            Value = (int.Parse(dicQueryPm[EasyNetGlobalConstWord.PAGEINDEX].ToString()) - 1) * int.Parse(dicQueryPm[EasyNetGlobalConstWord.PAGESIZE].ToString()) + 1
                        };
                        parameters_List[iIndx_List] = sp;
                    }
                    else if (pmkey == EasyNetGlobalConstWord.PAGESIZE)
                    {
                        var sp = new SqlParameter
                        {
                            ParameterName = "@" + EasyNetGlobalConstWord.PAGEEND,
                            Value = int.Parse(dicQueryPm[EasyNetGlobalConstWord.PAGEINDEX].ToString()) * int.Parse(dicQueryPm[EasyNetGlobalConstWord.PAGESIZE].ToString())
                        };
                        parameters_List[iIndx_List] = sp;
                    }
                    else
                    {
                        var sp = new SqlParameter
                        {
                            ParameterName = "@" + pmkey,
                            Value = dicQueryPm[pmkey]
                        };
                        parameters_List[iIndx_List] = sp;
                        if (pmkey != EasyNetGlobalConstWord.QUERYSORT)
                        {
                            parameters_Count[iIndx_Count] = sp;
                            iIndx_Count++;
                        }
                    }
                    iIndx_List++;
                }

                if (bCount)
                {
                    iCount = (int)DbHelperSQL.GetSingle(sPrcName + nameof(Count), parameters_Count);
                }

                ds = DbHelperSQL.Query(sPrcName, parameters_List);

                pageResult.Total = iCount;
                pageResult.DataList = ds.Tables[0];
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }

            return pageResult;
        }

        #endregion 分頁查詢返回分頁結果

        #region 查询List數據

        public static Object QueryBySql(string sql, Object obj, string flag)
        {
            var oRel = new Object();
            var dic = obj as Dictionary<string, object>;
            try
            {
                var parms = DbFactory.CreateDbParameters(dic.Keys.Count);
                var idx = 0;
                foreach (string key in dic.Keys)
                {
                    parms[idx].ParameterName = key;
                    parms[idx].Value = dic[key];
                    idx++;
                }

                var ds = AdoHelper.DataSet(AdoHelper.ConnectionString, sql.StartsWith("OSP_") ? CommandType.StoredProcedure : CommandType.Text, sql, parms);

                if (flag == "one")
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        var newData = new Dictionary<string, object>();
                        foreach (var col in ds.Tables[0].Columns)
                        {
                            newData.Add(col.ToString(), row[col.ToString()]);
                        }
                        oRel = newData;
                        break;
                    }
                }
                else
                {
                    oRel = ds.Tables[0];
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
            return oRel;
        }

        #endregion 查询List數據

        #region 私有方法

        private static void GetSqlAndParmForAdd(Object obj, TableInfo tbinfo, out string sql, out IDbDataParameter[] parms)
        {
            var idx = 0;
            var dic = obj as Dictionary<string, object>;
            dic = dic.Where(x => !"ExFeild1,ExFeild2,ExFeild3,ExFeild4,ExFeild5,ExFeild6".Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);

            sql = DbEntityUtils.GetInsertByObjSql(tbinfo, dic) + Environment.NewLine;

            parms = DbFactory.CreateDbParameters(dic.Keys.Count);
            foreach (string _key in dic.Keys)
            {
                var sValue = dic[_key];
                var sValNew = sValue;
                if (_key.ToLower() == EasyNetGlobalConstWord.PASSWORD)
                {
                    sValNew = SecurityUtil.Encrypt(sValue.ToString());
                }
                else if (sValue != null && sValue.GetType() == typeof(string) && sValue.ToString().StartsWith(EasyNetGlobalConstWord.SERIALNUMBER))
                {
                    var saPm = sValue.ToString().Split('|');
                    var sMidifyUser = "";
                    if (dic.ContainsKey(EasyNetGlobalConstWord.MODIFYUSER))
                    {
                        sMidifyUser = dic[EasyNetGlobalConstWord.MODIFYUSER].ToString();
                    }
                    sValNew = SerialNumber.GetMaxNumberByType(saPm[1].ToString(), saPm[2].ToString(), SerialNumber.GetMaxNumberType(saPm[3].ToString()), sMidifyUser, int.Parse(saPm[4].ToString()), saPm.Length > 5 ? saPm[5] : "", saPm.Length > 6 ? saPm[6] : "");    //獲取最大編號

                    if (sValue.ToString().StartsWith(EasyNetGlobalConstWord.SERIALNUMBER + "_"))
                    {
                        sValNew = sValNew.ToString() + SerialNumber.Pcheck(sValNew.ToString());
                    }
                    else if (sValue.ToString().StartsWith(EasyNetGlobalConstWord.SERIALNUMBER + "+"))
                    {
                        var saLen = saPm[0].ToString().Split('+');
                        var iLen = int.Parse(saLen[1].ToString());
                        sValNew = sValNew.ToString() + SecurityUtil.GetRandomNumber(iLen);
                    }
                }
                sValue = sValNew;
                parms[idx].ParameterName = _key;
                parms[idx].Value = sValue;
                idx++;
            }
        }

        private static void GetSqlAndParmForUpd(Object obj, TableInfo tbinfo, out string sql, out IDbDataParameter[] parms)
        {
            var idx = 0;
            var dic = obj as Dictionary<string, object>;

            var values = dic["values"] as Dictionary<string, object>;
            var whkeys = dic["keys"] as Dictionary<string, object>;

            sql = DbEntityUtils.GetUpdateByObjSql(tbinfo, ref values, whkeys) + System.Environment.NewLine;
            var dicParms = DbFactory.ApllyDic(values, whkeys);
            parms = DbFactory.CreateDbParameters(dicParms.Keys.Count);
            foreach (string key in dicParms.Keys)
            {
                var sValue = dicParms[key];
                var sValNew = sValue;
                if (sValue != null && sValue.GetType() == typeof(string) && sValue.ToString().StartsWith(EasyNetGlobalConstWord.SERIALNUMBER))
                {
                    var saPm = sValue.ToString().Split('|');
                    var sMidifyUser = "";
                    if (dic.ContainsKey(EasyNetGlobalConstWord.MODIFYUSER))
                    {
                        sMidifyUser = dic[EasyNetGlobalConstWord.MODIFYUSER].ToString();
                    }
                    sValNew = SerialNumber.GetMaxNumberByType(saPm[1].ToString(), saPm[2].ToString(), SerialNumber.GetMaxNumberType(saPm[3].ToString()), sMidifyUser, int.Parse(saPm[4].ToString()), saPm.Length > 5 ? saPm[5] : "", saPm.Length > 6 ? saPm[6] : "");    //獲取最大編號

                    if (sValue.ToString().StartsWith(EasyNetGlobalConstWord.SERIALNUMBER + "_"))
                    {
                        sValNew = sValNew.ToString() + SerialNumber.Pcheck(sValNew.ToString());
                    }
                    else if (sValue.ToString().StartsWith(EasyNetGlobalConstWord.SERIALNUMBER + "+"))
                    {
                        var saLen = saPm[0].ToString().Split('+');
                        var iLen = int.Parse(saLen[1].ToString());
                        sValNew = sValNew.ToString() + SecurityUtil.GetRandomNumber(iLen);
                    }
                }
                sValue = sValNew;
                parms[idx].ParameterName = key;
                parms[idx].Value = sValue;
                idx++;
            }
        }

        private static void GetSqlAndParmForDel(Object owh, TableInfo tbinfo, out string sql, out IDbDataParameter[] parms)
        {
            var idx = 0;
            var dic = owh as Dictionary<string, object>;

            sql = DbEntityUtils.GetDeleteByObjSql(tbinfo, dic) + System.Environment.NewLine;

            parms = DbFactory.CreateDbParameters(dic.Keys.Count);
            foreach (string _key in dic.Keys)
            {
                parms[idx].ParameterName = _key;
                parms[idx].Value = dic[_key];
                idx++;
            }
        }

        private static int ExecuteInsert(Object data, IDbTransaction tran)
        {
            IDbDataParameter[] parms = null;
            var sSql = "";
            var iVal = 0;
            var oEntity = data as Dictionary<string, object>;

            foreach (string key in oEntity.Keys)
            {
                var entity = EntityHelper.GetEntity(key);
                var properties = ReflectionHelper.GetProperties(entity.GetType());
                var tableInfo = DbEntityUtils.GetTableInfo(entity, DbOperateType.DELETE, properties);

                if (oEntity[key].GetType() == typeof(Object[]))
                {
                    var saEntity = oEntity[key] as Object[];
                    foreach (Object jo in saEntity)
                    {
                        GetSqlAndParmForAdd(jo, tableInfo, out sSql, out parms);

                        iVal += Execute(tran, sSql, parms);
                    }
                    if (iVal < 0)
                    {
                        break;
                    }
                }
                else
                {
                    GetSqlAndParmForAdd(oEntity[key], tableInfo, out sSql, out parms);

                    iVal += Execute(tran, sSql, parms);
                }
            }

            return iVal;
        }

        private static int ExecuteUpdate(Object data, IDbTransaction tran)
        {
            IDbDataParameter[] parms = null;
            var sSql = "";
            var iVal = 0;
            var oEntity = data as Dictionary<string, object>;

            foreach (string key in oEntity.Keys)
            {
                var entity = EntityHelper.GetEntity(key);
                var properties = ReflectionHelper.GetProperties(entity.GetType());
                var tableInfo = DbEntityUtils.GetTableInfo(entity, DbOperateType.DELETE, properties);

                if (oEntity[key].GetType() == typeof(Object[]))
                {
                    var saEntity = oEntity[key] as Object[];
                    foreach (Object jo in saEntity)
                    {
                        GetSqlAndParmForUpd(jo, tableInfo, out sSql, out parms);

                        iVal += Execute(tran, sSql, parms);
                    }
                    if (iVal < 0)
                    {
                        break;
                    }
                }
                else
                {
                    GetSqlAndParmForUpd(oEntity[key], tableInfo, out sSql, out parms);

                    iVal += Execute(tran, sSql, parms);
                }
            }

            return iVal;
        }

        private static int ExecuteDelete(Object data, IDbTransaction tran)
        {
            IDbDataParameter[] parms = null;
            var sSql = "";
            var iVal = 0;
            var oEntity = data as Dictionary<string, object>;
            foreach (string key in oEntity.Keys)
            {
                var entity = EntityHelper.GetEntity(key);
                var properties = ReflectionHelper.GetProperties(entity.GetType());
                var tableInfo = DbEntityUtils.GetTableInfo(entity, DbOperateType.DELETE, properties);

                if (oEntity[key].GetType() == typeof(Object[]))
                {
                    var saEntity = oEntity[key] as Object[];
                    foreach (Object jo in saEntity)
                    {
                        GetSqlAndParmForDel(jo, tableInfo, out sSql, out parms);
                        iVal += Execute(tran, sSql, parms);
                    }
                }
                else
                {
                    GetSqlAndParmForDel(oEntity[key], tableInfo, out sSql, out parms);
                    iVal += Execute(tran, sSql, parms);
                }
            }
            return iVal;
        }

        private static int Execute(IDbTransaction transaction, string sql, IDbDataParameter[] parms)
        {
            var iVal = AdoHelper.ExecuteNonQuery(transaction, sql.StartsWith("OSP_") ? CommandType.StoredProcedure : CommandType.Text, sql, parms);
            return iVal;
        }

        private IDbConnection GetConnection(string sKey = null)
        {
            //獲取資料庫連接，如果開啟了事務，從事務中獲取
            IDbConnection connection = null;
            if (m_Transaction != null)
            {
                connection = m_Transaction.Connection;
            }
            else
            {
                connection = DbFactory.CreateDbConnection(AdoHelper.ConnectionString);
            }

            return connection;
        }

        private bool GetWillConnectionState()
        {
            return m_Transaction == null;
        }

        #endregion 私有方法
    }
}