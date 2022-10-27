using System;
using System.Data;

namespace SqlSugar.Base
{
    using log4net;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Sugar ORM父類, 封裝一些基本的操作
    /// </summary>
    public static class SugarBase
    {
        public static readonly ILog mo_Log = LogManager.GetLogger("SQLSugarLog");//記錄異常

        /// <summary>
        /// 資料庫連結字串
        /// </summary>
        public static string DB_ConnectionString = GetConnectionString("ConnectionString");

        /// <summary>
        /// 獲取ORM資料庫連線物件(只操作資料庫一次的使用, 否則會進行多次資料庫連接和關閉) 默認超時時間為30秒 預設為SQL Server資料庫 預設自動關閉資料庫連結,
        /// 多次操作資料庫請勿使用該屬性, 可能會造成性能問題 要自訂請使用GetIntance()方法或者直接使用Exec方法, 傳委託
        /// </summary>
        public static SqlSugarClient DB
        {
            get
            {
                return InitDB(null, 30, DbType.SqlServer, true);
            }
        }

        /// <summary>
        /// 獲得SqlSugarClient(使用該方法, 默認請手動釋放資源, 如using(var db = SugarBase.GetIntance()){你的代碼},
        /// 如果把isAutoCloseConnection參數設置為true, 則無需手動釋放, 會每次操作資料庫釋放一次, 可能會影響性能, 請自行判斷使用)
        /// </summary>
        /// <param name="commandTimeOut">等待超時時間, 默認為30秒 (單位: 秒)</param>
        /// <param name="dbType">資料庫類型, 預設為SQL Server</param>
        /// <param name="isAutoCloseConnection">
        /// 是否自動關閉資料庫連接, 預設不是, 如果設置為true, 則會在每次操作完資料庫後, 即時關閉, 如果一個方法裡面多次操作了資料庫, 建議保持為false, 否則可能會引發性能問題
        /// </param>
        /// <returns></returns>
        public static SqlSugarClient GetIntance(List<SqlFuncExternal> expfn = null, int commandTimeOut = 30, DbType dbType = DbType.SqlServer, bool isAutoCloseConnection = false)
        {
            var db = SugarBase.InitDB(expfn, commandTimeOut, dbType, isAutoCloseConnection);

            db.Aop.OnLogExecuting = (sql, pars) =>
            {

                var temsql = sql;
                
                var dic = pars.ToDictionary(it => it.ParameterName, it => it.Value);
                foreach (var param in dic)
                {
                    if (param.Value != null)
                    {
                        temsql = temsql.Replace(param.Key, "'" + param.Value.ToString() + "'");
                    }
                }

                System.Diagnostics.Debug.WriteLine(temsql + "\r\n");
                //mo_Log.Info(temsql);  //紀錄LOG打開此行
            };

            return db;
        }

        public static SqlSugarClient InstanceByAttribute
        {
            get
            {
                var db = new SqlSugarClient(new ConnectionConfig { InitKeyType = InitKeyType.Attribute, ConnectionString = Config.ConnectionString, DbType = DbType.SqlServer, IsAutoCloseConnection = true });
                return db;
            }
        }

        /// <summary>
        /// 初始化ORM連線物件
        /// </summary>
        /// <param name="commandTimeOut">等待超時時間, 默認為30秒 (單位: 秒)</param>
        /// <param name="dbType">資料庫類型, 預設為SQL Server</param>
        /// <param name="isAutoCloseConnection">
        /// 是否自動關閉資料庫連接, 預設不是, 如果設置為true, 則會在每次操作完資料庫後, 即時關閉, 如果一個方法裡面多次操作了資料庫, 建議保持為false, 否則可能會引發性能問題
        /// </param>
        private static SqlSugarClient InitDB(List<SqlFuncExternal> expfn, int commandTimeOut = 30, DbType dbType = DbType.SqlServer, bool isAutoCloseConnection = false)
        {
            var expMethods = GetExpMethods(expfn);
            var db = new SqlSugarClient(new ConnectionConfig
            {
                ConnectionString = SugarBase.DB_ConnectionString,
                DbType = dbType,
                InitKeyType = InitKeyType.Attribute,
                IsAutoCloseConnection = isAutoCloseConnection,
                ConfigureExternalServices = new ConfigureExternalServices
                {
                    SqlFuncServices = expMethods//set ext method
                }
            });
            db.Ado.CommandTimeOut = commandTimeOut;
            return db;
        }

        /// <summary>
        /// 執行資料庫操作
        /// </summary>
        /// <typeparam name="Result">返回數值型別 泛型</typeparam>
        /// <param name="func">方法委託</param>
        /// <param name="commandTimeOut">超時時間, 單位為秒, 預設為30秒</param>
        /// <param name="dbType">資料庫類型, 預設為SQL Server</param>
        /// <returns>泛型返回值</returns>
        public static Result Exec<Result>(Func<SqlSugarClient, Result> func, int commandTimeOut = 30, DbType dbType = DbType.SqlServer)
        {
            if (func == null) throw new Exception("委託為null, 交易處理無意義");
            using (var db = InitDB(null, commandTimeOut, dbType))
            {
                try
                {
                    return func(db);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    db.Dispose();
                }
            }
        }

        /// <summary>
        /// 帶交易處理的執行資料庫操作
        /// </summary>
        /// <typeparam name="Result">返回數值型別 泛型</typeparam>
        /// <param name="func">方法委託</param>
        /// <param name="commandTimeOut">超時時間, 單位為秒, 預設為30秒</param>
        /// <param name="dbType">資料庫類型, 預設為SQL Server</param>
        /// <returns>泛型返回值</returns>
        public static Result ExecTran<Result>(Func<SqlSugarClient, Result> func, int commandTimeOut = 30, DbType dbType = DbType.SqlServer)
        {
            if (func == null) throw new Exception("委託為null, 交易處理無意義");
            using (var db = InitDB(null, commandTimeOut, dbType))
            {
                try
                {
                    db.Ado.BeginTran(IsolationLevel.Unspecified);
                    var result = func(db);
                    db.Ado.CommitTran();
                    return result;
                }
                catch (Exception ex)
                {
                    db.Ado.RollbackTran();
                    throw ex;
                }
                finally
                {
                    db.Dispose();
                }
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
                var connectionString = DBUnit.GetAppSettings(Key);

                if (!String.IsNullOrEmpty(connectionString)) return connectionString;

                var sDbHost = DBUnit.GetAppSettings("DbHost");
                var sDbPort = DBUnit.GetAppSettings("DbPort");
                var sDbName = DBUnit.GetAppSettings("DbName");
                var sDbUser = DBUnit.GetAppSettings("DbUser");
                var sDbPassword = DBUnit.GetAppSettings("DbPassword");
                var sDbMinPoolSize = DBUnit.GetAppSettings("DbMinPoolSize");
                var sDbMaxPoolSize = DBUnit.GetAppSettings("DbMaxPoolSize");
                var sDbCharset = DBUnit.GetAppSettings("DbCharset");

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

        public static List<SqlFuncExternal> GetExpMethods(List<SqlFuncExternal> expfn)
        {
            var saMethods = new List<SqlFuncExternal>();
            var method = new SqlFuncExternal
            {
                UniqueMethodName = nameof(ISNULL),
                MethodValue = (expInfo, dbType, expContext) =>
                 {
                     return $"ISNULL({expInfo.Args[0].MemberName},'')";
                 }
            };
            if (expfn != null && expfn.Count > 0)
            {
                foreach (var _method in expfn)
                {
                    saMethods.Add(_method);
                }
            }

            return saMethods;
        }

        public static string ISNULL<T>(T str)
        {
            throw new NotSupportedException("Can only be used in expressions");
        }
    }
}