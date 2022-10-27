using EasyNet.DBUtility;
using EasyNet.Manager;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace EntityBuilder
{
    public class TableHelper
    {
        /// <summary>
        /// 获取局域网内的所有數據庫服务器名称
        /// </summary>
        /// <returns>服务器名称数组</returns>
        public static List<string> GetSqlServerNames()
        {
            var dataSources = SqlClientFactory.Instance.CreateDataSourceEnumerator().GetDataSources();

            var column = dataSources.Columns[@"InstanceName"];
            var column2 = dataSources.Columns[@"ServerName"];

            var rows = dataSources.Rows;
            var Serverlist = new List<string>();
            var array = string.Empty;
            for (int i = rows.Count - 1; i >= 0; i--)
            {
                var str2 = rows[i][column2] as string;
                if (((!(rows[i][column] is string str)) || (str.Length == 0)) || (@"MSSQLSERVER" == str))
                {
                    array = str2;
                }
                else
                {
                    array = str2 + @"/" + str;
                }

                Serverlist.Add(array);
            }

            Serverlist.Sort();

            return Serverlist;
        }

        /// <summary>
        /// 查詢sql中的非系统库
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static List<string> DatabaseList(string connection)
        {
            var getCataList = new List<string>();
            const string cmdStirng = @"select name from sys.databases where database_id > 4";
            var connect = new SqlConnection(connection);
            using (var cmd = new System.Data.SqlClient.SqlCommand(cmdStirng, connect))
            {
                try
                {
                    if (connect.State == ConnectionState.Closed)
                    {
                        connect.Open();
                        IDataReader dr = cmd.ExecuteReader();
                        getCataList.Clear();
                        while (dr.Read())
                        {
                            getCataList.Add(dr[@"name"].ToString());
                        }
                        dr.Close();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (connect != null && connect.State == ConnectionState.Open)
                    {
                        connect.Dispose();
                    }
                }
                return getCataList;
            }
        }

        public static List<DbTableInfo> GetTables()
        {
            if (AdoHelper.DbType == DatabaseType.SQLSERVER)
            {
                return GetMSSQLTables();
            }
            else if (AdoHelper.DbType == DatabaseType.MYSQL)
            {
                return GetMySQLTables();
            }
            else
            {
                throw new Exception(@"暂时不支持其他數據庫類型");
            }
        }

        /// <summary>
        /// 获取列名
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static List<DbTableInfo> GetMSSQLTables()
        {
            var connection = (SqlConnection)DbFactory.CreateDbConnection(AdoHelper.ConnectionString);
            var tablelist = new List<DbTableInfo>();
            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                    var objTable = connection.GetSchema(@"Tables");
                    foreach (DataRow row in objTable.Rows)
                    {
                        var tb = new DbTableInfo
                        {
                            Name = row[2].ToString(),
                            DbObjectType = row[3].ToString() == @"VIEW" ? DbObjectType.View : DbObjectType.Table
                        };
                        tablelist.Add(tb);
                    }
                }
            }
            catch 
            {
                throw;
            }
            finally
            {
                if (connection != null && connection.State == ConnectionState.Closed)
                {
                    connection.Dispose();
                }
            }

            return tablelist;
        }

        /// <summary>
        /// 获取列名
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static List<DbTableInfo> GetMySQLTables()
        {
            var sql = @"select TABLE_NAME as name from INFORMATION_SCHEMA.`TABLES` WHERE TABLE_SCHEMA = '" + AdoHelper.DbName + @"'";

            var m = ManagerFactory.GetManager();
            var tablelist = DbManager.Query<DbTableInfo>(sql);

            return tablelist;
        }

        public static List<ColumnInfo> GetColumnField(string TableName)
        {
            if (AdoHelper.DbType == DatabaseType.SQLSERVER)
            {
                return GetMSSQLColumnField(TableName);
            }
            else if (AdoHelper.DbType == DatabaseType.MYSQL)
            {
                return GetMySQLColumnField(TableName);
            }
            else
            {
                throw new Exception(@"暂时不支持其他數據庫類型");
            }
        }

        /// <summary>
        /// 獲取字段
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static List<ColumnInfo> GetMSSQLColumnField(string TableName)
        {
            var sb = new StringBuilder();
            if (TableName.IndexOf(@"OVW") > -1)
            {
                sb.Append(@" Select c.name As DbColumnName , t.name As DataType,'' AS IsIdentity,'' AS IsPrimarykey, CASE c.isnullable WHEN 1 THEN '√' ELSE '' END as IsNullable  ");
                sb.Append(@" From SysObjects As o , SysColumns As c , SysTypes As t  ");
                sb.Append(@" Where o.type in ('u','v') And o.id = c.id And c.xtype = t.xtype AND t.name<>'sysname' And o.Name = '").Append(TableName).Append(@"' ");
            }
            else
            {
                sb.Append(@" SELECT a.name as DbColumnName,");
                sb.Append(@" b.name as DataType,");
                sb.Append(@" CASE COLUMNPROPERTY(a.id,a.name,'IsIdentity') WHEN 1 THEN '√' ELSE '' END as IsIdentity, ");
                sb.Append(@" CASE WHEN EXISTS ( SELECT * FROM sysobjects WHERE xtype='PK' AND name IN ( SELECT name FROM sysindexes WHERE id=a.id AND indid IN ( SELECT indid FROM sysindexkeys ");
                sb.Append(@" WHERE id=a.id AND colid IN ( SELECT colid FROM syscolumns WHERE id=a.id AND name=a.name ) ) ) ) THEN '√' ELSE '' END as IsPrimarykey,");
                sb.Append(@" CASE a.isnullable WHEN 1 THEN '√' ELSE '' END as IsNullable ");
                sb.Append(@" FROM syscolumns a ");
                sb.Append(@" LEFT  JOIN systypes      b ON a.xtype=b.xusertype ");
                sb.Append(@" INNER JOIN sysobjects    c ON a.id=c.id AND c.xtype='U' AND c.name<>'dtproperties' ");
                sb.Append(@" LEFT  JOIN syscomments   d ON a.cdefault=d.id ");
                sb.Append(@" WHERE c.name = '").Append(TableName).Append(@"' ");
                sb.Append(@" ORDER BY c.name, a.colorder");
            }

            //使用Mast框架查詢數據
            var m = ManagerFactory.GetManager();
            var list = DbManager.Query<ColumnInfo>(sb.ToString());
            return list;
        }

        /// <summary>
        /// 獲取字段
        /// </summary>
        /// <param name="TableName"></param>
        /// <returns></returns>
        public static List<ColumnInfo> GetMySQLColumnField(string TableName)
        {
            var sb = new StringBuilder();

            sb.Append(@" SELECT COLUMN_NAME as DbColumnName,");
            sb.Append(@" DATA_TYPE as DataType,");
            sb.Append(@" CASE EXTRA WHEN 'auto_increment' THEN '√' ELSE '' END as IsIdentity, ");
            sb.Append(@" CASE COLUMN_KEY WHEN 'PRI' THEN '√' ELSE '' END as IsPrimaryKey, ");
            sb.Append(@" CASE IS_NULLABLE WHEN 'YES' THEN '√' ELSE '' END as IsNullable ");
            sb.Append(@" from INFORMATION_SCHEMA.COLUMNS ");
            sb.Append(@" Where table_name = '").Append(TableName).Append(@"' ");
            sb.Append(@" AND table_schema = '").Append(AdoHelper.DbName).Append(@"' ");

            //使用Mast框架查詢數據
            var m = ManagerFactory.GetManager();
            var list = DbManager.Query<ColumnInfo>(sb.ToString());
            return list;
        }
    }
}