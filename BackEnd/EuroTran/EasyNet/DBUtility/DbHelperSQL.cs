using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace EasyNet.DBUtility
{
    public abstract class DbHelperSQL
    {
        //資料庫連接字符串(web.config來配置)，可以動態改變connectionString支持多資料庫.
        private static string _connectionString = AdoHelper.ConnectionString;

        protected DbHelperSQL()
        {
        }

        #region 公用方法

        /// <summary>
        /// 判斷是否存在某表的某個欄位
        /// </summary>
        /// <param name="tableName">表名稱</param>
        /// <param name="columnName">欄位名稱</param>
        /// <returns>是否存在</returns>
        public static bool ColumnExists(string tableName, string columnName)
        {
            var sql = "select count(1) from syscolumns where [id]=object_id('" + tableName + "') and [name]='" + columnName + "'";
            var res = GetSingle(sql);
            if (res == null)
            {
                return false;
            }
            return Convert.ToInt32(res) > 0;
        }

        public static int GetMaxID(string FieldName, string TableName)
        {
            var strsql = "select max(" + FieldName + ")+1 from " + TableName;
            var obj = DbHelperSQL.GetSingle(strsql);
            return obj == null ? 1 : int.Parse(obj.ToString());
        }

        public static bool Exists(string strSql)
        {
            var obj = DbHelperSQL.GetSingle(strSql);
            int cmdresult;
            cmdresult = (Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)) ? 0 : int.Parse(obj.ToString());
            return cmdresult == 0 ? false : true;
        }

        /// <summary>
        /// 表是否存在
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static bool TabExists(string TableName)
        {
            var strsql = "select count(*) from sysobjects where id = object_id(N'[" + TableName + "]') and OBJECTPROPERTY(id, N'IsUserTable') = 1";
            //string strsql = "SELECT count(*) FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[" + TableName + "]') AND type in (N'U')";
            var obj = DbHelperSQL.GetSingle(strsql);
            int cmdresult;
            cmdresult = (Object.Equals(obj, null)) || (Object.Equals(obj, DBNull.Value)) ? 0 : int.Parse(obj.ToString());
            return cmdresult == 0 ? false : true;
        }

        public static bool Exists(string strSql, params SqlParameter[] cmdParms)
        {
            var obj = DbHelperSQL.GetSingle(strSql, cmdParms);
            int cmdresult;
            cmdresult = (Object.Equals(obj, null)) || (Object.Equals(obj, DBNull.Value)) ? 0 : int.Parse(obj.ToString());
            return cmdresult == 0 ? false : true;
        }

        #endregion 公用方法

        #region 執行簡單SQL語句

        /// <summary>
        /// 執行SQL語句，返回影響的資料數
        /// </summary>
        /// <param name="SQLString">SQL語句</param>
        /// <returns>影響的資料數</returns>
        public static int ExecuteSql(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        var rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SqlException e)
                    {
                        connection.Close();
                        throw new Exception("some reason to rethrow", e);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
        }

        public static int ExecuteSqlByTime(string SQLString, int Times)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        var rows = cmd.ExecuteNonQuery();
                        return rows;
                    }
                    catch (SqlException e)
                    {
                        connection.Close();
                        throw new Exception("some reason to rethrow", e);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 執行多条SQL語句，實現資料庫事務。
        /// </summary>
        /// <param name="SQLStringList">多条SQL語句</param>
        public static int ExecuteSqlTran(List<String> SQLStringList)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                var cmd = new SqlCommand
                {
                    Connection = connection
                };
                var tx = connection.BeginTransaction();
                cmd.Transaction = tx;
                try
                {
                    var count = 0;
                    foreach (var strsql in SQLStringList)
                    {
                        if (strsql.Trim().Length > 1)
                        {
                            cmd.CommandText = strsql;
                            count += cmd.ExecuteNonQuery();
                        }
                    }
                    tx.Commit();
                    return count;
                }
                catch (Exception)
                {
                    tx.Rollback();
                    return 0;
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 執行带一個存储過程参數的的SQL語句。
        /// </summary>
        /// <param name="SQLString">SQL語句</param>
        /// <param name="content">参數内容,比如一個欄位是格式複雜的文章，有特殊符號，可以通過這個方式添加</param>
        /// <returns>影響的資料數</returns>
        public static int ExecuteSql(string SQLString, string content)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand(SQLString, connection);
                var myParameter = new SqlParameter("@content", SqlDbType.NText)
                {
                    Value = content
                };
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    var rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (SqlException e)
                {
                    throw new Exception("some reason to rethrow", e);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 執行带一個存储過程参數的的SQL語句。
        /// </summary>
        /// <param name="SQLString">SQL語句</param>
        /// <param name="content">参數内容,比如一個欄位是格式複雜的文章，有特殊符號，可以通過這個方式添加</param>
        /// <returns>影響的資料數</returns>
        public static object ExecuteSqlGet(string SQLString, string content)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand(SQLString, connection);
                var myParameter = new SqlParameter("@content", SqlDbType.NText)
                {
                    Value = content
                };
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    var obj = cmd.ExecuteScalar();
                    return (Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)) ? null : obj;
                }
                catch (SqlException e)
                {
                    throw new Exception("some reason to rethrow", e);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 向資料庫里插入圖像格式的欄位(和上面情況類似的另一種實例)
        /// </summary>
        /// <param name="strSQL">SQL語句</param>
        /// <param name="fs">圖像字節,資料庫的欄位類型为image的情況</param>
        /// <returns>影響的資料數</returns>
        public static int ExecuteSqlInsertImg(string strSQL, byte[] fs)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand(strSQL, connection);
                var myParameter = new SqlParameter("@fs", SqlDbType.Image)
                {
                    Value = fs
                };
                cmd.Parameters.Add(myParameter);
                try
                {
                    connection.Open();
                    var rows = cmd.ExecuteNonQuery();
                    return rows;
                }
                catch (SqlException e)
                {
                    throw new Exception("some reason to rethrow", e);
                }
                finally
                {
                    cmd.Dispose();
                    connection.Close();
                }
            }
        }

        /// <summary>
        /// 執行一条计算查詢結果語句，返回查詢結果{Object}。
        /// </summary>
        /// <param name="SQLString">计算查詢結果語句</param>
        /// <returns>查詢結果{Object}</returns>
        public static object GetSingle(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        var obj = cmd.ExecuteScalar();
                        return (Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)) ? null : obj;
                    }
                    catch (SqlException e)
                    {
                        connection.Close();
                        throw new Exception("some reason to rethrow", e);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
        }

        public static object GetSingle(string SQLString, int Times)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(SQLString, connection))
                {
                    try
                    {
                        connection.Open();
                        cmd.CommandTimeout = Times;
                        var obj = cmd.ExecuteScalar();
                        return (Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)) ? null : obj;
                    }
                    catch (SqlException e)
                    {
                        connection.Close();
                        throw new Exception("some reason to rethrow", e);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 執行查詢語句，返回SqlDataReader ( 注意：調用該方法後，一定要對SqlDataReader進行Close )
        /// </summary>
        /// <param name="strSQL">查詢語句</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader ExecuteReader(string strSQL)
        {
            var connection = new SqlConnection(_connectionString);
            using (var cmd = new SqlCommand(strSQL, connection))
            {
                try
                {
                    connection.Open();
                    var myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    return myReader;
                }
                catch (SqlException e)
                {
                    throw new Exception("some reason to rethrow", e);
                }
            }
        }

        /// <summary>
        /// 執行查詢語句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查詢語句</param>
        /// <returns>DataSet</returns>
        public static DataSet Query(string SQLString)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var ds = new DataSet();
                try
                {
                    connection.Open();
                    using (var command = new SqlDataAdapter(SQLString, connection))
                    {
                        command.Fill(ds, nameof(ds));
                    }
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
                return ds;
            }
        }

        public static DataSet Query(string SQLString, int Times)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var ds = new DataSet();
                try
                {
                    connection.Open();
                    using (var command = new SqlDataAdapter(SQLString, connection))
                    {
                        command.SelectCommand.CommandTimeout = Times;
                        command.Fill(ds, nameof(ds));
                    }
                }
                catch (SqlException ex)
                {
                    throw new Exception(ex.Message);
                }
                finally
                {
                    connection.Close();
                }
                return ds;
            }
        }

        #endregion 執行簡單SQL語句

        #region 執行带参數的SQL語句

        /// <summary>
        /// 執行SQL語句，返回影響的資料數
        /// </summary>
        /// <param name="SQLString">SQL語句</param>
        /// <param name="cmdParms">todo: describe cmdParms parameter on ExecuteSql</param>
        /// <returns>影響的資料數</returns>
        public static int ExecuteSql(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        var rows = cmd.ExecuteNonQuery();
                        cmd.Parameters.Clear();
                        return rows;
                    }
                    catch (SqlException e)
                    {
                        throw new Exception("some reason to rethrow", e);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 執行多条SQL語句，實現資料庫事務。
        /// </summary>
        /// <param name="SQLStringList">SQL語句的哈希表（key为sql語句，value是該語句的SqlParameter[]）</param>
        public static void ExecuteSqlTran(Hashtable SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    var cmd = new SqlCommand();
                    try
                    {
                        //循環
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            var cmdText = myDE.Key.ToString();
                            var cmdParms = (SqlParameter[])myDE.Value;
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            var val = cmd.ExecuteNonQuery();
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 執行多条SQL語句，實現資料庫事務。
        /// </summary>
        /// <param name="cmdList">todo: describe cmdList parameter on ExecuteSqlTran</param>
        public static int ExecuteSqlTran(List<CommandInfo> cmdList)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    var cmd = new SqlCommand();
                    try
                    {
                        var count = 0;
                        //循環
                        foreach (CommandInfo myDE in cmdList)
                        {
                            var cmdText = myDE.CommandText;
                            var cmdParms = (SqlParameter[])myDE.Parameters;
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);

                            if (myDE.EffentNextType == EffentNextType.WhenHaveContine || myDE.EffentNextType == EffentNextType.WhenNoHaveContine)
                            {
                                if (myDE.CommandText.ToLower().IndexOf("count(") == -1)
                                {
                                    trans.Rollback();
                                    return 0;
                                }

                                var obj = cmd.ExecuteScalar();
                                var isHave = false;
                                if (obj == null && obj == DBNull.Value)
                                {
                                    isHave = false;
                                }
                                isHave = Convert.ToInt32(obj) > 0;

                                if (myDE.EffentNextType == EffentNextType.WhenHaveContine && !isHave)
                                {
                                    trans.Rollback();
                                    return 0;
                                }
                                if (myDE.EffentNextType == EffentNextType.WhenNoHaveContine && isHave)
                                {
                                    trans.Rollback();
                                    return 0;
                                }
                                continue;
                            }
                            var val = cmd.ExecuteNonQuery();
                            count += val;
                            if (myDE.EffentNextType == EffentNextType.ExcuteEffectRows && val == 0)
                            {
                                trans.Rollback();
                                return 0;
                            }
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                        return count;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 執行多条SQL語句，實現資料庫事務。
        /// </summary>
        /// <param name="SQLStringList">SQL語句的哈希表（key为sql語句，value是該語句的SqlParameter[]）</param>
        public static void ExecuteSqlTranWithIndentity(List<CommandInfo> SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    var cmd = new SqlCommand();
                    try
                    {
                        var indentity = 0;
                        //循環
                        foreach (CommandInfo myDE in SQLStringList)
                        {
                            var cmdText = myDE.CommandText;
                            var cmdParms = (SqlParameter[])myDE.Parameters;
                            foreach (SqlParameter q in cmdParms)
                            {
                                if (q.Direction == ParameterDirection.InputOutput)
                                {
                                    q.Value = indentity;
                                }
                            }
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            var val = cmd.ExecuteNonQuery();
                            foreach (SqlParameter q in cmdParms)
                            {
                                if (q.Direction == ParameterDirection.Output)
                                {
                                    indentity = Convert.ToInt32(q.Value);
                                }
                            }
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 執行多条SQL語句，實現資料庫事務。
        /// </summary>
        /// <param name="SQLStringList">SQL語句的哈希表（key为sql語句，value是該語句的SqlParameter[]）</param>
        public static void ExecuteSqlTranWithIndentity(Hashtable SQLStringList)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    var cmd = new SqlCommand();
                    try
                    {
                        var indentity = 0;
                        //循環
                        foreach (DictionaryEntry myDE in SQLStringList)
                        {
                            var cmdText = myDE.Key.ToString();
                            var cmdParms = (SqlParameter[])myDE.Value;
                            foreach (SqlParameter q in cmdParms)
                            {
                                if (q.Direction == ParameterDirection.InputOutput)
                                {
                                    q.Value = indentity;
                                }
                            }
                            PrepareCommand(cmd, conn, trans, cmdText, cmdParms);
                            var val = cmd.ExecuteNonQuery();
                            foreach (SqlParameter q in cmdParms)
                            {
                                if (q.Direction == ParameterDirection.Output)
                                {
                                    indentity = Convert.ToInt32(q.Value);
                                }
                            }
                            cmd.Parameters.Clear();
                        }
                        trans.Commit();
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary>
        /// 執行一条计算查詢結果語句，返回查詢結果{Object}。
        /// </summary>
        /// <param name="SQLString">计算查詢結果語句</param>
        /// <param name="cmdParms">todo: describe cmdParms parameter on GetSingle</param>
        /// <returns>查詢結果{Object}</returns>
        public static object GetSingle(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                using (SqlCommand cmd = new SqlCommand())
                {
                    try
                    {
                        PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                        var obj = cmd.ExecuteScalar();
                        cmd.Parameters.Clear();
                        return (Object.Equals(obj, null)) || (Object.Equals(obj, System.DBNull.Value)) ? null : obj;
                    }
                    catch (SqlException e)
                    {
                        throw new Exception("some reason to rethrow", e);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                }
            }
        }

        /// <summary>
        /// 執行查詢語句，返回SqlDataReader ( 注意：調用該方法後，一定要對SqlDataReader進行Close )
        /// </summary>
        /// <param name="SQLString">todo: describe SQLString parameter on ExecuteReader</param>
        /// <param name="cmdParms">todo: describe cmdParms parameter on ExecuteReader</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader ExecuteReader(string SQLString, params SqlParameter[] cmdParms)
        {
            var connection = new SqlConnection(_connectionString);
            var cmd = new SqlCommand();
            try
            {
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                var myReader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return myReader;
            }
            catch (SqlException e)
            {
                throw new Exception("some reason to rethrow", e);
            }
        }

        /// <summary>
        /// 執行查詢語句，返回DataSet
        /// </summary>
        /// <param name="SQLString">查詢語句</param>
        /// <param name="cmdParms">todo: describe cmdParms parameter on Query</param>
        /// <returns>DataSet</returns>
        public static DataSet Query(string SQLString, params SqlParameter[] cmdParms)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand();
                PrepareCommand(cmd, connection, null, SQLString, cmdParms);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    var ds = new DataSet();
                    try
                    {
                        da.Fill(ds, nameof(ds));
                        cmd.Parameters.Clear();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception(ex.Message);
                    }
                    finally
                    {
                        cmd.Dispose();
                        connection.Close();
                    }
                    return ds;
                }
            }
        }

        private static void PrepareCommand(SqlCommand cmd, SqlConnection conn, SqlTransaction trans, string cmdText, SqlParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();
            cmd.Connection = conn;
            cmd.CommandText = cmdText;
            if (trans != null)
                cmd.Transaction = trans;
            if (cmdText.StartsWith("OSP_"))
                cmd.CommandType = CommandType.StoredProcedure;//cmdType;
            else
                cmd.CommandType = CommandType.Text;//cmdType;
            if (cmdParms != null)
            {
                foreach (SqlParameter parameter in cmdParms)
                {
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    cmd.Parameters.Add(parameter);
                }
            }
        }

        #endregion 執行带参數的SQL語句

        #region 存储過程操作

        /// <summary>
        /// 執行存储過程，返回SqlDataReader ( 注意：調用該方法後，一定要對SqlDataReader進行Close )
        /// </summary>
        /// <param name="storedProcName">存储過程名</param>
        /// <param name="parameters">存储過程参數</param>
        /// <returns>SqlDataReader</returns>
        public static SqlDataReader RunProcedure(string storedProcName, IDataParameter[] parameters)
        {
            var connection = new SqlConnection(_connectionString);
            SqlDataReader returnReader;
            connection.Open();
            var command = BuildQueryCommand(connection, storedProcName, parameters);
            command.CommandType = CommandType.StoredProcedure;
            returnReader = command.ExecuteReader(CommandBehavior.CloseConnection);
            return returnReader;
        }

        /// <summary>
        /// 執行存储過程
        /// </summary>
        /// <param name="storedProcName">存储過程名</param>
        /// <param name="parameters">存储過程参數</param>
        /// <param name="tableName">DataSet結果中的表名</param>
        /// <returns>DataSet</returns>
        public static DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var dataSet = new DataSet();
                connection.Open();
                using (var sqlDA = new SqlDataAdapter
                {
                    SelectCommand = BuildQueryCommand(connection, storedProcName, parameters)
                })
                {
                    sqlDA.Fill(dataSet, tableName);
                    connection.Close();
                    return dataSet;
                }
            }
        }

        public static DataSet RunProcedure(string storedProcName, IDataParameter[] parameters, string tableName, int Times)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var dataSet = new DataSet();
                connection.Open();
                using (var sqlDA = new SqlDataAdapter
                {
                    SelectCommand = BuildQueryCommand(connection, storedProcName, parameters)
                })
                {
                    sqlDA.SelectCommand.CommandTimeout = Times;
                    sqlDA.Fill(dataSet, tableName);
                    connection.Close();
                    return dataSet;
                }
            }
        }

        /// <summary>
        /// 构建 SqlCommand 對象(用來返回一個結果集，而不是一個整數值)
        /// </summary>
        /// <param name="connection">資料庫連接</param>
        /// <param name="storedProcName">存储過程名</param>
        /// <param name="parameters">存储過程参數</param>
        /// <returns>SqlCommand</returns>
        private static SqlCommand BuildQueryCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            var command = new SqlCommand(storedProcName, connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            foreach (SqlParameter parameter in parameters)
            {
                if (parameter != null)
                {
                    // 檢查未分配值的輸出参數,将其分配以DBNull.Value.
                    if ((parameter.Direction == ParameterDirection.InputOutput || parameter.Direction == ParameterDirection.Input) &&
                        (parameter.Value == null))
                    {
                        parameter.Value = DBNull.Value;
                    }
                    command.Parameters.Add(parameter);
                }
            }

            return command;
        }

        /// <summary>
        /// 執行存储過程，返回影響的行數
        /// </summary>
        /// <param name="storedProcName">存储過程名</param>
        /// <param name="parameters">存储過程参數</param>
        /// <param name="rowsAffected">影響的行數</param>
        /// <returns></returns>
        public static int RunProcedure(string storedProcName, IDataParameter[] parameters, out int rowsAffected)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                int result;
                connection.Open();
                var command = BuildIntCommand(connection, storedProcName, parameters);
                rowsAffected = command.ExecuteNonQuery();
                result = (int)command.Parameters["ReturnValue"].Value;
                //Connection.Close();
                return result;
            }
        }

        /// <summary>
        /// 創建 SqlCommand 對象實例(用來返回一個整數值)
        /// </summary>
        /// <param name="storedProcName">存储過程名</param>
        /// <param name="parameters">存储過程参數</param>
        /// <param name="connection">todo: describe connection parameter on BuildIntCommand</param>
        /// <returns>SqlCommand 對象實例</returns>
        private static SqlCommand BuildIntCommand(SqlConnection connection, string storedProcName, IDataParameter[] parameters)
        {
            var command = BuildQueryCommand(connection, storedProcName, parameters);
            command.Parameters.Add(new SqlParameter("ReturnValue",
                SqlDbType.Int, 4, ParameterDirection.ReturnValue,
                false, 0, 0, string.Empty, DataRowVersion.Default, null));
            return command;
        }

        #endregion 存储過程操作
    }
}