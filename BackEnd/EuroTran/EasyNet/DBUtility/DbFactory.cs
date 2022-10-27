using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.OracleClient;
using System.Data.SqlClient;

namespace EasyNet.DBUtility
{
    public class DbFactory
    {
        /// <summary>
        /// 根據設定檔中所配置的資料庫類型 來獲取命令參數中的參數符號oracle為":",sqlserver為"@"
        /// </summary>
        /// <returns></returns>
        public static string CreateDbParmCharacter()
        {
            var character = string.Empty;

            switch (AdoHelper.DbType)
            {
                case DatabaseType.SQLSERVER:
                    character = "@";
                    break;

                case DatabaseType.ORACLE:
                    character = ":";
                    break;

                case DatabaseType.MYSQL:
                    character = "?";
                    break;

                case DatabaseType.ACCESS:
                    character = "@";
                    break;

                default:
                    throw new Exception("數據庫類型目前不支持！");
            }

            return character;
        }

        /// <summary>
        /// 根據設定檔中所配置的資料庫類型和傳入的 資料庫連結字串來創建相應資料庫連線物件
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public static IDbConnection CreateDbConnection(string connectionString)
        {
            IDbConnection conn = null;
            switch (AdoHelper.DbType)
            {
                case DatabaseType.SQLSERVER:
                    conn = new SqlConnection(connectionString);
                    break;

                case DatabaseType.MYSQL:
                    conn = new MySqlConnection(connectionString);
                    break;

                case DatabaseType.ACCESS:
                    conn = new OleDbConnection(connectionString);
                    break;

                default:
                    throw new Exception("數據庫類型目前不支持！");
            }

            return conn;
        }

        /// <summary>
        /// 根據設定檔中所配置的資料庫類型 來創建相應資料庫命令物件
        /// </summary>
        /// <returns></returns>
        public static IDbCommand CreateDbCommand()
        {
            IDbCommand cmd = null;
            switch (AdoHelper.DbType)
            {
                case DatabaseType.SQLSERVER:
                    cmd = new SqlCommand();
                    break;

                case DatabaseType.MYSQL:
                    cmd = new MySqlCommand();
                    break;

                case DatabaseType.ACCESS:
                    cmd = new OleDbCommand();
                    break;

                default:
                    throw new Exception("數據庫類型目前不支持！");
            }

            return cmd;
        }

        /// <summary>
        /// 根據設定檔中所配置的資料庫類型 來創建相應資料庫適配器物件
        /// </summary>
        /// <returns></returns>
        public static IDbDataAdapter CreateDataAdapter()
        {
            IDbDataAdapter adapter = null;
            switch (AdoHelper.DbType)
            {
                case DatabaseType.SQLSERVER:
                    adapter = new SqlDataAdapter();
                    break;

                case DatabaseType.MYSQL:
                    adapter = new MySqlDataAdapter();
                    break;

                case DatabaseType.ACCESS:
                    adapter = new OleDbDataAdapter();
                    break;

                default:
                    throw new Exception("數據庫類型目前不支持！");
            }

            return adapter;
        }

        /// <summary>
        /// 根據設定檔中所配置的資料庫類型 和傳入的命令物件來創建相應資料庫適配器物件
        /// </summary>
        /// <param name="cmd">todo: describe cmd parameter on CreateDataAdapter</param>
        /// <returns></returns>
        public static IDbDataAdapter CreateDataAdapter(IDbCommand cmd)
        {
            IDbDataAdapter adapter = null;
            switch (AdoHelper.DbType)
            {
                case DatabaseType.SQLSERVER:
                    adapter = new SqlDataAdapter((SqlCommand)cmd);
                    break;

                case DatabaseType.MYSQL:
                    adapter = new MySqlDataAdapter((MySqlCommand)cmd);
                    break;

                case DatabaseType.ACCESS:
                    adapter = new OleDbDataAdapter((OleDbCommand)cmd);
                    break;

                default: throw new Exception("數據庫類型目前不支持！");
            }

            return adapter;
        }

        /// <summary>
        /// 根據設定檔中所配置的資料庫類型 來創建相應資料庫的參數物件
        /// </summary>
        /// <returns></returns>
        public static IDbDataParameter CreateDbParameter()
        {
            IDbDataParameter param = null;
            switch (AdoHelper.DbType)
            {
                case DatabaseType.SQLSERVER:
                    param = new SqlParameter();
                    break;

                case DatabaseType.ORACLE:
                    param = new OracleParameter();
                    break;

                case DatabaseType.MYSQL:
                    param = new MySqlParameter();
                    break;

                case DatabaseType.ACCESS:
                    param = new OleDbParameter();
                    break;

                default:
                    throw new Exception("數據庫類型目前不支持！");
            }

            return param;
        }

        /// <summary>
        /// 根據設定檔中所配置的資料庫類型 來創建相應資料庫命令物件
        /// </summary>
        /// <param name="paramName">todo: describe paramName parameter on CreateDbParameter</param>
        /// <param name="value">todo: describe value parameter on CreateDbParameter</param>
        /// <returns></returns>
        public static IDbDataParameter CreateDbParameter(string paramName, object value)
        {
            if (AdoHelper.DbType == DatabaseType.ACCESS)
            {
                paramName = "@" + paramName;
            }

            var param = DbFactory.CreateDbParameter();
            param.ParameterName = paramName;
            param.Value = value;

            return param;
        }

        /// <summary>
        /// 根據設定檔中所配置的資料庫類型 來創建相應資料庫命令物件
        /// </summary>
        /// <param name="paramName">todo: describe paramName parameter on CreateDbParameter</param>
        /// <param name="value">todo: describe value parameter on CreateDbParameter</param>
        /// <param name="dbType">todo: describe dbType parameter on CreateDbParameter</param>
        /// <returns></returns>
        public static IDbDataParameter CreateDbParameter(string paramName, object value, DbType dbType)
        {
            if (AdoHelper.DbType == DatabaseType.ACCESS)
            {
                paramName = "@" + paramName;
            }

            var param = DbFactory.CreateDbParameter();
            param.DbType = dbType;
            param.ParameterName = paramName;
            param.Value = value;

            return param;
        }

        /// <summary>
        /// 根據設定檔中所配置的資料庫類型 來創建相應資料庫命令物件
        /// </summary>
        /// <param name="paramName">todo: describe paramName parameter on CreateDbParameter</param>
        /// <param name="value">todo: describe value parameter on CreateDbParameter</param>
        /// <param name="direction">todo: describe direction parameter on CreateDbParameter</param>
        /// <returns></returns>
        public static IDbDataParameter CreateDbParameter(string paramName, object value, ParameterDirection direction)
        {
            if (AdoHelper.DbType == DatabaseType.ACCESS)
            {
                paramName = "@" + paramName;
            }

            var param = DbFactory.CreateDbParameter();
            param.Direction = direction;
            param.ParameterName = paramName;
            param.Value = value;

            return param;
        }

        /// <summary>
        /// 根據設定檔中所配置的資料庫類型 來創建相應資料庫命令物件
        /// </summary>
        /// <param name="paramName">todo: describe paramName parameter on CreateDbParameter</param>
        /// <param name="value">todo: describe value parameter on CreateDbParameter</param>
        /// <param name="size">todo: describe size parameter on CreateDbParameter</param>
        /// <param name="direction">todo: describe direction parameter on CreateDbParameter</param>
        /// <returns></returns>
        public static IDbDataParameter CreateDbParameter(string paramName, object value, int size, ParameterDirection direction)
        {
            if (AdoHelper.DbType == DatabaseType.ACCESS)
            {
                paramName = "@" + paramName;
            }

            var param = DbFactory.CreateDbParameter();
            param.Direction = direction;
            param.ParameterName = paramName;
            param.Value = value;
            param.Size = size;

            return param;
        }

        /// <summary>
        /// CreateDbParameters
        /// </summary>
        /// <param name="paramName">todo: describe paramName parameter on CreateDbOutParameter</param>
        /// <param name="size">todo: describe size parameter on CreateDbOutParameter</param>
        /// <returns></returns>
        public static IDbDataParameter CreateDbOutParameter(string paramName, int size)
        {
            if (AdoHelper.DbType == DatabaseType.ACCESS)
            {
                paramName = "@" + paramName;
            }

            var param = DbFactory.CreateDbParameter();
            param.Direction = ParameterDirection.Output;
            param.ParameterName = paramName;
            param.Size = size;

            return param;
        }

        /// <summary>
        /// 根據設定檔中所配置的資料庫類型 來創建相應資料庫命令物件
        /// </summary>
        /// <param name="paramName">todo: describe paramName parameter on CreateDbParameter</param>
        /// <param name="value">todo: describe value parameter on CreateDbParameter</param>
        /// <param name="dbType">todo: describe dbType parameter on CreateDbParameter</param>
        /// <param name="direction">todo: describe direction parameter on CreateDbParameter</param>
        /// <returns></returns>
        public static IDbDataParameter CreateDbParameter(string paramName, object value, DbType dbType, ParameterDirection direction)
        {
            if (AdoHelper.DbType == DatabaseType.ACCESS)
            {
                paramName = "@" + paramName;
            }

            var param = DbFactory.CreateDbParameter();
            param.Direction = direction;
            param.DbType = dbType;
            param.ParameterName = paramName;
            param.Value = value;

            return param;
        }

        /// <summary>
        /// 根據設定檔中所配置的資料庫類型 來創建相應資料庫命令物件
        /// </summary>
        /// <param name="size">todo: describe size parameter on CreateDbParameters</param>
        /// <returns></returns>
        public static IDbDataParameter[] CreateDbParameters(int size)
        {
            var i = 0;
            IDbDataParameter[] param = null;
            switch (AdoHelper.DbType)
            {
                case DatabaseType.SQLSERVER:
                    param = new SqlParameter[size];
                    while (i < size) { param[i] = new SqlParameter(); i++; }
                    break;

                case DatabaseType.ORACLE:
                    param = new OracleParameter[size];
                    while (i < size) { param[i] = new OracleParameter(); i++; }
                    break;

                case DatabaseType.MYSQL:
                    param = new MySqlParameter[size];
                    while (i < size) { param[i] = new MySqlParameter(); i++; }
                    break;

                case DatabaseType.ACCESS:
                    param = new OleDbParameter[size];
                    while (i < size) { param[i] = new OleDbParameter(); i++; }
                    break;

                default:
                    throw new Exception("數據庫類型目前不支持！");
            }

            return param;
        }

        /// <summary>
        /// 合併Dictionary
        /// </summary>
        /// <param name="dic1">todo: describe dic1 parameter on ApllyDic</param>
        /// <param name="dic2">todo: describe dic2 parameter on ApllyDic</param>
        /// <returns></returns>
        public static Dictionary<string, object> ApllyDic(Dictionary<string, object> dic1, Dictionary<string, object> dic2)
        {
            var newDic = new Dictionary<string, object>();
            foreach (string key in dic1.Keys)
            {
                if (!newDic.ContainsKey(key))
                {
                    newDic.Add(key, dic1[key]);
                }
            }
            foreach (string key in dic2.Keys)
            {
                if (!newDic.ContainsKey(key))
                {
                    newDic.Add(key, dic2[key]);
                }
            }
            return newDic;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public static IDbTransaction CreateDbTransaction()
        {
            var conn = CreateDbConnection(AdoHelper.ConnectionString);

            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }

            return conn.BeginTransaction();
        }
    }
}