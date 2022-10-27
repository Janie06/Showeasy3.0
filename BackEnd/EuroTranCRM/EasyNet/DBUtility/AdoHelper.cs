using EasyNet.Common;
using System;
using System.Collections;
using System.Data;
using System.Text;

namespace EasyNet.DBUtility
{
    public class AdoHelper
    {
        //獲取資料庫類型
        private static string strDbType = CommonUtils.GetConfigValueByKey("dbType").ToUpper();

        //將資料庫類型轉換成枚舉類型
        public static DatabaseType DbType = DatabaseTypeEnumParse<DatabaseType>(strDbType);

        public static string DbHost = CommonUtils.GetConfigValueByKey("DbHost");
        public static string DbPort = CommonUtils.GetConfigValueByKey("DbPort");
        public static string DbName = CommonUtils.GetConfigValueByKey("DbName");
        public static string DbUser = CommonUtils.GetConfigValueByKey("DbUser");
        public static string DbPassword = CommonUtils.GetConfigValueByKey("DbPassword");
        public static string DbMinPoolSize = CommonUtils.GetConfigValueByKey("DbMinPoolSize");
        public static string DbMaxPoolSize = CommonUtils.GetConfigValueByKey("DbMaxPoolSize");
        public static string DbCharset = CommonUtils.GetConfigValueByKey("DbCharset");

        //獲取資料庫連接字串
        public static string ConnectionString = GetConnectionString("ConnectionString");

        //獲取資料庫具名引數符號，比如@(SQLSERVER)、:(ORACLE)
        public static string DbParmChar = DbFactory.CreateDbParmCharacter();

        private static Hashtable parmCache = Hashtable.Synchronized(new Hashtable());

        /// <summary>
        ///通過提供的參數，執行無結果集的資料庫操作命令
        /// 並返回執行資料庫操作所影響的行數。
        /// </summary>
        /// <param name="connectionString">資料庫連接字串</param>
        /// <param name="commandParameters">執行命令所需的參數陣列</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteNonQuery</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteNonQuery</param>
        /// <returns>返回通過執行命令所影響的行數</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText, params IDbDataParameter[] commandParameters)
        {
            var cmd = DbFactory.CreateDbCommand();

            using (IDbConnection conn = DbFactory.CreateDbConnection(connectionString))
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                var val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return val;
            }
        }

        /// <summary>
        ///通過提供的參數，執行無結果集的資料庫操作命令
        /// 並返回執行資料庫操作所影響的行數。
        /// </summary>
        /// <param name="connectionString">資料庫連接字串</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteNonQuery</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteNonQuery</param>
        /// <returns>返回通過執行命令所影響的行數</returns>
        public static int ExecuteNonQuery(string connectionString, CommandType cmdType, string cmdText)
        {
            var cmd = DbFactory.CreateDbCommand();

            using (IDbConnection conn = DbFactory.CreateDbConnection(connectionString))
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, null);
                var val = cmd.ExecuteNonQuery();
                cmd.Parameters.Clear();
                return val;
            }
        }

        /// <summary>
        ///通過提供的參數，執行無結果集返回的資料庫操作命令
        ///並返回執行資料庫操作所影響的行數。
        /// </summary>
        /// <param name="commandParameters">執行命令所需的參數陣列</param>
        /// <param name="connection">todo: describe connection parameter on ExecuteNonQuery</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteNonQuery</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteNonQuery</param>
        /// <remarks>
        /// e.g.:
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <returns>返回通過執行命令所影響的行數</returns>
        public static int ExecuteNonQuery(IDbConnection connection, CommandType cmdType, string cmdText, params IDbDataParameter[] commandParameters)
        {
            var cmd = DbFactory.CreateDbCommand();

            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            var val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        ///通過提供的參數，執行無結果集返回的資料庫操作命令
        ///並返回執行資料庫操作所影響的行數。
        /// </summary>
        /// <param name="connection">todo: describe connection parameter on ExecuteNonQuery</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteNonQuery</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteNonQuery</param>
        /// <remarks>
        /// e.g.:
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <returns>返回通過執行命令所影響的行數</returns>
        public static int ExecuteNonQuery(IDbConnection connection, CommandType cmdType, string cmdText)
        {
            var cmd = DbFactory.CreateDbCommand();

            PrepareCommand(cmd, connection, null, cmdType, cmdText, null);
            var val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        ///通過提供的參數，執行無結果集返回的資料庫操作命令
        ///並返回執行資料庫操作所影響的行數。
        /// </summary>
        /// <param name="trans">sql事務物件</param>
        /// <param name="commandParameters">執行命令所需的參數陣列</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteNonQuery</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteNonQuery</param>
        /// <remarks>
        /// e.g.:
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <returns>返回通過執行命令所影響的行數</returns>
        public static int ExecuteNonQuery(IDbTransaction trans, CommandType cmdType, string cmdText, params IDbDataParameter[] commandParameters)
        {
            var val = 0;
            var cmd = DbFactory.CreateDbCommand();

            if (trans == null || trans.Connection == null)
            {
                using (IDbConnection conn = DbFactory.CreateDbConnection(AdoHelper.ConnectionString))
                {
                    PrepareCommand(cmd, conn, trans, cmdType, cmdText, commandParameters);
                    val = cmd.ExecuteNonQuery();
                }
            }
            else
            {
                PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
                val = cmd.ExecuteNonQuery();
            }

            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        ///通過提供的參數，執行無結果集返回的資料庫操作命令
        ///並返回執行資料庫操作所影響的行數。
        /// </summary>
        /// <param name="trans">sql事務物件</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteNonQuery</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteNonQuery</param>
        /// <remarks>
        /// e.g.:
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <returns>返回通過執行命令所影響的行數</returns>
        public static int ExecuteNonQuery(IDbTransaction trans, CommandType cmdType, string cmdText)
        {
            var cmd = DbFactory.CreateDbCommand();
            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, null);
            var val = cmd.ExecuteNonQuery();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        /// 使用提供的參數，執行有結果集返回的資料庫操作命令 並返回SqlDataReader對象
        /// </summary>
        /// <param name="connectionString">資料庫連接字串</param>
        /// <param name="commandParameters">執行命令所需的參數陣列</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteReader</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteReader</param>
        /// <remarks>
        /// e.g.: SqlDataReader r = ExecuteReader(connString, CommandType.StoredProcedure,
        /// "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <returns>返回SqlDataReader對象</returns>
        public static IDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText, params IDbDataParameter[] commandParameters)
        {
            var cmd = DbFactory.CreateDbCommand();
            var conn = DbFactory.CreateDbConnection(connectionString);

            //我們在這裡使用一個 try/catch,因為如果PrepareCommand方法拋出一個異常，我們想在捕獲代碼裡面關閉
            //connection連線物件，因為異常發生datareader將不會存在，所以commandBehaviour.CloseConnection
            //將不會執行。
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                var rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return rdr;
            }
            catch
            {
                conn.Close();
                cmd.Dispose();
                throw;
            }
        }

        /// <summary>
        /// 使用提供的參數，執行有結果集返回的資料庫操作命令 並返回SqlDataReader對象
        /// </summary>
        /// <param name="commandParameters">執行命令所需的參數陣列</param>
        /// <param name="trans">todo: describe trans parameter on ExecuteReader</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteReader</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteReader</param>
        /// <remarks>
        /// e.g.: SqlDataReader r = ExecuteReader(connString, CommandType.StoredProcedure,
        /// "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <returns>返回SqlDataReader對象</returns>
        public static IDataReader ExecuteReader(IDbTransaction trans, CommandType cmdType, string cmdText, params IDbDataParameter[] commandParameters)
        {
            var cmd = DbFactory.CreateDbCommand();
            var conn = trans.Connection;

            //我們在這裡使用一個 try/catch,因為如果PrepareCommand方法拋出一個異常，我們想在捕獲代碼裡面關閉
            //connection連線物件，因為異常發生datareader將不會存在，所以commandBehaviour.CloseConnection
            //將不會執行。
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                var rdr = cmd.ExecuteReader();
                cmd.Parameters.Clear();
                return rdr;
            }
            catch
            {
                conn.Close();
                cmd.Dispose();
                throw;
            }
        }

        /// <summary>
        /// 使用提供的參數，執行有結果集返回的資料庫操作命令 並返回SqlDataReader對象
        /// </summary>
        /// <param name="commandParameters">執行命令所需的參數陣列</param>
        /// <param name="closeConnection">todo: describe closeConnection parameter on ExecuteReader</param>
        /// <param name="connection">todo: describe connection parameter on ExecuteReader</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteReader</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteReader</param>
        /// <remarks>
        /// e.g.: SqlDataReader r = ExecuteReader(connString, CommandType.StoredProcedure,
        /// "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <returns>返回SqlDataReader對象</returns>
        public static IDataReader ExecuteReader(bool closeConnection, IDbConnection connection, CommandType cmdType, string cmdText, params IDbDataParameter[] commandParameters)
        {
            var cmd = DbFactory.CreateDbCommand();
            var conn = connection;

            //我們在這裡使用一個 try/catch,因為如果PrepareCommand方法拋出一個異常，我們想在捕獲代碼裡面關閉
            //connection連線物件，因為異常發生datareader將不會存在，所以commandBehaviour.CloseConnection
            //將不會執行。
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                var rdr = closeConnection ? cmd.ExecuteReader(CommandBehavior.CloseConnection) : cmd.ExecuteReader();
                cmd.Parameters.Clear();
                return rdr;
            }
            catch
            {
                conn.Close();
                cmd.Dispose();
                throw;
            }
        }

        /// <summary>
        ///使用提供的參數，執行有結果集返回的資料庫操作命令
        /// 並返回SqlDataReader對象
        /// </summary>
        /// <param name="connectionString">資料庫連接字串</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteReader</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteReader</param>
        /// <returns>返回SqlDataReader對象</returns>
        public static IDataReader ExecuteReader(string connectionString, CommandType cmdType, string cmdText)
        {
            var cmd = DbFactory.CreateDbCommand();
            var conn = DbFactory.CreateDbConnection(connectionString);

            //我們在這裡使用一個 try/catch,因為如果PrepareCommand方法拋出一個異常，我們想在捕獲代碼裡面關閉
            //connection連線物件，因為異常發生datareader將不會存在，所以commandBehaviour.CloseConnection
            //將不會執行。
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, null);
                var rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                cmd.Parameters.Clear();
                return rdr;
            }
            catch (Exception ex)
            {
                conn.Close();
                cmd.Dispose();
                throw new Exception("some reason to rethrow", ex);
            }
        }

        /// <summary>
        ///使用提供的參數，執行有結果集返回的資料庫操作命令
        /// 並返回SqlDataReader對象
        /// </summary>
        /// <param name="trans">todo: describe trans parameter on ExecuteReader</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteReader</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteReader</param>
        /// <returns>返回SqlDataReader對象</returns>
        public static IDataReader ExecuteReader(IDbTransaction trans, CommandType cmdType, string cmdText)
        {
            var cmd = DbFactory.CreateDbCommand();
            var conn = trans.Connection;

            //我們在這裡使用一個 try/catch,因為如果PrepareCommand方法拋出一個異常，我們想在捕獲代碼裡面關閉
            //connection連線物件，因為異常發生datareader將不會存在，所以commandBehaviour.CloseConnection
            //將不會執行。
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, null);
                var rdr = cmd.ExecuteReader();
                cmd.Parameters.Clear();
                return rdr;
            }
            catch (Exception ex)
            {
                conn.Close();
                cmd.Dispose();
                throw new Exception("some reason to rethrow", ex);
            }
        }

        /// <summary>
        ///使用提供的參數，執行有結果集返回的資料庫操作命令
        /// 並返回SqlDataReader對象
        /// </summary>
        /// <param name="closeConnection">todo: describe closeConnection parameter on ExecuteReader</param>
        /// <param name="connection">todo: describe connection parameter on ExecuteReader</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteReader</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteReader</param>
        /// <returns>返回SqlDataReader對象</returns>
        public static IDataReader ExecuteReader(bool closeConnection, IDbConnection connection, CommandType cmdType, string cmdText)
        {
            var cmd = DbFactory.CreateDbCommand();
            var conn = connection;

            //我們在這裡使用一個 try/catch,因為如果PrepareCommand方法拋出一個異常，我們想在捕獲代碼裡面關閉
            //connection連線物件，因為異常發生datareader將不會存在，所以commandBehaviour.CloseConnection
            //將不會執行。
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, null);
                var rdr = closeConnection ? cmd.ExecuteReader(CommandBehavior.CloseConnection) : cmd.ExecuteReader();
                cmd.Parameters.Clear();
                return rdr;
            }
            catch (Exception ex)
            {
                conn.Close();
                cmd.Dispose();
                throw new Exception("some reason to rethrow", ex);
            }
        }

        /// <summary>
        /// 查詢資料填充到資料集DataSet中
        /// </summary>
        /// <param name="connectionString">資料庫連接字串</param>
        /// <param name="cmdType">執行命令的類型（存儲過程或T-SQL，等等）</param>
        /// <param name="cmdText">命令文本</param>
        /// <param name="commandParameters">參數陣列</param>
        /// <returns>資料集DataSet對象</returns>
        public static DataSet DataSet(string connectionString, CommandType cmdType, string cmdText, params IDbDataParameter[] commandParameters)
        {
            var ds = new DataSet();
            var cmd = DbFactory.CreateDbCommand();
            var conn = DbFactory.CreateDbConnection(connectionString);
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, commandParameters);
                var sda = DbFactory.CreateDataAdapter(cmd);
                sda.Fill(ds);
                return ds;
            }
            catch
            {
                conn.Close();
                cmd.Dispose();
                throw;
            }
            finally
            {
                conn.Close();
                cmd.Dispose();
            }
        }

        /// <summary>
        /// 查詢資料填充到資料集DataSet中
        /// </summary>
        /// <param name="connectionString">資料庫連接字串</param>
        /// <param name="cmdType">執行命令的類型（存儲過程或T-SQL，等等）</param>
        /// <param name="cmdText">命令文本</param>
        /// <returns>資料集DataSet對象</returns>
        public static DataSet DataSet(string connectionString, CommandType cmdType, string cmdText)
        {
            var ds = new DataSet();
            var cmd = DbFactory.CreateDbCommand();
            var conn = DbFactory.CreateDbConnection(connectionString);
            try
            {
                PrepareCommand(cmd, conn, null, cmdType, cmdText, null);
                var sda = DbFactory.CreateDataAdapter(cmd);
                sda.Fill(ds);
                return ds;
            }
            catch
            {
                conn.Close();
                cmd.Dispose();
                throw;
            }
            finally
            {
                conn.Close();
                cmd.Dispose();
            }
        }

        /// <summary>
        /// 依靠資料庫連接字串connectionString, 使用所提供參數，執行返回首行首列命令
        /// </summary>
        /// <param name="connectionString">資料庫連接字串</param>
        /// <param name="commandParameters">執行命令所需的參數陣列</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteScalar</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteScalar</param>
        /// <remarks>
        /// e.g.: Object obj = ExecuteScalar(connString, CommandType.StoredProcedure,
        /// "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <returns>返回一個物件，使用Convert.To{Type}將該對象轉換成想要的資料類型。</returns>
        public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText, params IDbDataParameter[] commandParameters)
        {
            var cmd = DbFactory.CreateDbCommand();

            using (IDbConnection connection = DbFactory.CreateDbConnection(connectionString))
            {
                PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
                var val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
        }

        /// <summary>
        /// 依靠資料庫連接字串connectionString, 使用所提供參數，執行返回首行首列命令
        /// </summary>
        /// <param name="connectionString">資料庫連接字串</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteScalar</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteScalar</param>
        /// <remarks>
        /// e.g.: Object obj = ExecuteScalar(connString, CommandType.StoredProcedure,
        /// "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <returns>返回一個物件，使用Convert.To{Type}將該對象轉換成想要的資料類型。</returns>
        public static object ExecuteScalar(string connectionString, CommandType cmdType, string cmdText)
        {
            var cmd = DbFactory.CreateDbCommand();

            using (IDbConnection connection = DbFactory.CreateDbConnection(connectionString))
            {
                PrepareCommand(cmd, connection, null, cmdType, cmdText, null);
                var val = cmd.ExecuteScalar();
                cmd.Parameters.Clear();
                return val;
            }
        }

        /// <summary>
        ///依靠資料庫連接字串connectionString,
        /// 使用所提供參數，執行返回首行首列命令
        /// </summary>
        /// <param name="commandParameters">執行命令所需的參數陣列</param>
        /// <param name="connection">todo: describe connection parameter on ExecuteScalar</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteScalar</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteScalar</param>
        /// <remarks>
        /// e.g.:
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <returns>返回一個物件，使用Convert.To{Type}將該對象轉換成想要的資料類型。</returns>
        public static object ExecuteScalar(IDbConnection connection, CommandType cmdType, string cmdText, params IDbDataParameter[] commandParameters)
        {
            var cmd = DbFactory.CreateDbCommand();

            PrepareCommand(cmd, connection, null, cmdType, cmdText, commandParameters);
            var val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        ///依靠資料庫連接字串connectionString,
        /// 使用所提供參數，執行返回首行首列命令
        /// </summary>
        /// <param name="connection">todo: describe connection parameter on ExecuteScalar</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteScalar</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteScalar</param>
        /// <remarks>
        /// e.g.:
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <returns>返回一個物件，使用Convert.To{Type}將該對象轉換成想要的資料類型。</returns>
        public static object ExecuteScalar(IDbConnection connection, CommandType cmdType, string cmdText)
        {
            var cmd = DbFactory.CreateDbCommand();

            PrepareCommand(cmd, connection, null, cmdType, cmdText, null);
            var val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        ///依靠資料庫連接字串connectionString,
        /// 使用所提供參數，執行返回首行首列命令
        /// </summary>
        /// <param name="conn">資料庫連線物件</param>
        /// <param name="trans">todo: describe trans parameter on ExecuteScalar</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteScalar</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteScalar</param>
        /// <remarks>
        /// e.g.:
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <returns>返回一個物件，使用Convert.To{Type}將該對象轉換成想要的資料類型。</returns>
        public static object ExecuteScalar(IDbConnection conn, IDbTransaction trans, CommandType cmdType, string cmdText)
        {
            var cmd = DbFactory.CreateDbCommand();

            PrepareCommand(cmd, conn, trans, cmdType, cmdText, null);
            var val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        ///依靠資料庫連接字串connectionString,
        /// 使用所提供參數，執行返回首行首列命令
        /// </summary>
        /// <param name="commandParameters">執行命令所需的參數陣列</param>
        /// <param name="trans">todo: describe trans parameter on ExecuteScalar</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteScalar</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteScalar</param>
        /// <remarks>
        /// e.g.:
        ///  Object obj = ExecuteScalar(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <returns>返回一個物件，使用Convert.To{Type}將該對象轉換成想要的資料類型。</returns>
        public static object ExecuteScalar(IDbTransaction trans, CommandType cmdType, string cmdText, params IDbDataParameter[] commandParameters)
        {
            var cmd = DbFactory.CreateDbCommand();

            PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
            var val = cmd.ExecuteScalar();
            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        ///通過提供的參數，執行無結果集返回的資料庫操作命令
        ///並返回執行資料庫操作所影響的行數。
        /// </summary>
        /// <param name="trans">sql事務物件</param>
        /// <param name="commandParameters">執行命令所需的參數陣列</param>
        /// <param name="cmdType">todo: describe cmdType parameter on ExecuteQuery</param>
        /// <param name="cmdText">todo: describe cmdText parameter on ExecuteQuery</param>
        /// <remarks>
        /// e.g.:
        ///  int result = ExecuteNonQuery(connString, CommandType.StoredProcedure, "PublishOrders", new SqlParameter("@prodid", 24));
        /// </remarks>
        /// <returns>返回通過執行命令所影響的行數</returns>
        public static object ExecuteQuery(IDbTransaction trans, CommandType cmdType, string cmdText, params IDbDataParameter[] commandParameters)
        {
            object val = null;
            var cmd = DbFactory.CreateDbCommand();

            if (trans == null || trans.Connection == null)
            {
                using (IDbConnection conn = DbFactory.CreateDbConnection(AdoHelper.ConnectionString))
                {
                    PrepareCommand(cmd, conn, trans, cmdType, cmdText, commandParameters);
                    val = cmd.ExecuteScalar();
                }
            }
            else
            {
                PrepareCommand(cmd, trans.Connection, trans, cmdType, cmdText, commandParameters);
                val = cmd.ExecuteScalar();
            }

            cmd.Parameters.Clear();
            return val;
        }

        /// <summary>
        /// add parameter array to the cache
        /// </summary>
        /// <param name="cacheKey">Key to the parameter cache</param>
        /// <param name="commandParameters">todo: describe commandParameters parameter on CacheParameters</param>
        public static void CacheParameters(string cacheKey, params IDbDataParameter[] commandParameters)
        {
            parmCache[cacheKey] = commandParameters;
        }

        /// <summary>
        /// 查詢緩存參數
        /// </summary>
        /// <param name="cacheKey">使用緩存名稱查找值</param>
        /// <returns>緩存參數陣列</returns>
        public static IDbDataParameter[] GetCachedParameters(string cacheKey)
        {
            var cachedParms = (IDbDataParameter[])parmCache[cacheKey];

            if (cachedParms == null)
                return null;

            var clonedParms = new IDbDataParameter[cachedParms.Length];

            for (int i = 0, j = cachedParms.Length; i < j; i++)
                clonedParms[i] = (IDbDataParameter)((ICloneable)cachedParms[i]).Clone();

            return clonedParms;
        }

        /// <summary>
        /// 為即將執行準備一個命令
        /// </summary>
        /// <param name="cmd">SqlCommand對象</param>
        /// <param name="conn">SqlConnection對象</param>
        /// <param name="trans">IDbTransaction對象</param>
        /// <param name="cmdType">執行命令的類型（存儲過程或T-SQL，等等）</param>
        /// <param name="cmdText">存儲過程名稱或者T-SQL命令列, e.g. Select * from Products</param>
        /// <param name="cmdParms">SqlParameters to use in the command</param>
        private static void PrepareCommand(IDbCommand cmd, IDbConnection conn, IDbTransaction trans, CommandType cmdType, string cmdText, IDbDataParameter[] cmdParms)
        {
            if (conn.State != ConnectionState.Open)
                conn.Open();

            cmd.Connection = conn;
            cmd.CommandText = cmdText;

            if (trans != null)
                cmd.Transaction = trans;

            cmd.CommandType = cmdType;

            if (cmdParms != null)
            {
                foreach (IDbDataParameter parm in cmdParms)
                    cmd.Parameters.Add(parm);
            }
        }

        /// <summary>
        /// 根據傳入的Key獲取設定檔中 相應Key的資料庫連接字串
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static string GetConnectionString(string Key)
        {
            try
            {
                var connectionString = CommonUtils.GetConfigValueByKey(Key);

                if (!String.IsNullOrEmpty(connectionString)) return connectionString;

                var sDbHost = CommonUtils.GetConfigValueByKey(nameof(DbHost));
                var sDbPort = CommonUtils.GetConfigValueByKey(nameof(DbPort));
                var sDbName = CommonUtils.GetConfigValueByKey(nameof(DbName));
                var sDbUser = CommonUtils.GetConfigValueByKey(nameof(DbUser));
                var sDbPassword = CommonUtils.GetConfigValueByKey(nameof(DbPassword));
                var sDbMinPoolSize = CommonUtils.GetConfigValueByKey(nameof(DbMinPoolSize));
                var sDbMaxPoolSize = CommonUtils.GetConfigValueByKey(nameof(DbMaxPoolSize));
                var sDbCharset = CommonUtils.GetConfigValueByKey(nameof(DbCharset));

                var sb = new StringBuilder();
                sb.Append("Data Source=").Append(sDbHost).Append(";");

                if (!String.IsNullOrEmpty(sDbPort))
                {
                    sb.Append("port=").Append(sDbPort).Append(";");
                }

                sb.Append("User ID=").Append(sDbUser).Append(";");
                sb.Append("Password=").Append(sDbPassword).Append(";");
                sb.Append("DataBase=").Append(sDbName).Append(";");

                if (!String.IsNullOrEmpty(sDbMinPoolSize))
                {
                    sb.Append("Min Pool Size=").Append(sDbMinPoolSize).Append(";");
                }

                if (!String.IsNullOrEmpty(sDbMinPoolSize))
                {
                    sb.Append("Max Pool Size=").Append(sDbMaxPoolSize).Append(";");
                }

                if (!String.IsNullOrEmpty(sDbCharset))
                {
                    sb.Append("charset=").Append(sDbCharset).Append(";");
                }

                return sb.ToString();
            }
            catch
            {
                throw new Exception("web.config檔appSettings中資料庫連接字串未配置或配置錯誤，必須為Key=\"connectionString\"");
            }
        }

        /// <summary>
        /// 用於資料庫類型的字串枚舉轉換
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value"></param>
        /// <returns></returns>
        public static T DatabaseTypeEnumParse<T>(string value)
        {
            try
            {
                return CommonUtils.EnumParse<T>(value);
            }
            catch
            {
                throw new Exception("資料庫類型\"" + value + "\"錯誤，請檢查！");
            }
        }
    }
}