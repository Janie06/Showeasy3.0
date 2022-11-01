using EasyNet.DBUtility;
using System;
using System.Data;

namespace EasyNet.Common
{
    public class SQLBuilderHelper
    {
        private static readonly string mssqlPageTemplate = @"select * from (select ROW_NUMBER() OVER(order by {0}) AS RowIndex, {1}) as tmp_tbl where RowIndex BETWEEN @pageStart and @pageEnd ";
        private static readonly string mysqlOrderPageTemplate = @"{0} order by {1} limit ?offset,?limit";
        private static readonly string mysqlPageTemplate = @"{0} limit ?offset,?limit";
        private static readonly string accessPageTemplate = @"select * from (select top @limit * from (select top @offset {0} order by id desc) order by id) order by {1}";

        public static string FetchColumns(string strSQL)
        {
            strSQL = strSQL.Trim();
            var columns = strSQL.Substring(6, strSQL.IndexOf(" from ") - 6);
            return columns;
        }

        public static string FetchPageBody(string strSQL)
        {
            var body = strSQL.Trim().Substring(6);
            return body;
        }

        public static string FetchWhere(string strSQL)
        {
            var index = strSQL.LastIndexOf("where");
            if (index == -1) return "";

            var where = strSQL.Substring(index, strSQL.Length - index);
            return where;
        }

        public static bool IsPage(string strSQL)
        {
            var strSql = strSQL.ToLower();

            if (AdoHelper.DbType == DatabaseType.ACCESS && strSql.IndexOf("top") == -1)
            {
                return false;
            }

            if (AdoHelper.DbType == DatabaseType.SQLSERVER && strSql.IndexOf("row_number()") == -1)
            {
                return false;
            }

            if (AdoHelper.DbType == DatabaseType.MYSQL && strSql.IndexOf("limit") == -1)
            {
                return false;
            }

            if (AdoHelper.DbType == DatabaseType.ORACLE && strSql.IndexOf("rowid") == -1)
            {
                return false;
            }

            return true;
        }

        public static string BuilderPageSQL(string strSql, string orderField, string order)
        {
            var columns = FetchColumns(strSql);
            var orderBy = orderField + " " + order;

            if (AdoHelper.DbType == DatabaseType.SQLSERVER && strSql.IndexOf("row_number()") == -1)
            {
                if (string.IsNullOrEmpty(order))
                {
                    throw new Exception(" SqlException: order field is null, you must support the order field for sqlserver page. ");
                }

                var pageBody = FetchPageBody(strSql);
                strSql = string.Format(mssqlPageTemplate, orderBy, pageBody);
            }

            if (AdoHelper.DbType == DatabaseType.ACCESS && strSql.IndexOf("top") == -1)
            {
                if (string.IsNullOrEmpty(order))
                {
                    throw new Exception(" SqlException: order field is null, you must support the order field for sqlserver page. ");
                }

                //select {0} from (select top @pageSize {1} from (select top @pageSize*@pageIndex {2} from {3} order by {4}) order by id) order by {5}
                var pageBody = FetchPageBody(strSql);
                strSql = string.Format(accessPageTemplate, pageBody, orderBy);
            }

            if (AdoHelper.DbType == DatabaseType.MYSQL)
            {
                if (!string.IsNullOrEmpty(order))
                {
                    strSql = string.Format(mysqlOrderPageTemplate, strSql, orderBy);
                }
                else
                {
                    strSql = string.Format(mysqlPageTemplate, strSql);
                }
            }

            return strSql;
        }

        public static string BuilderCountSQL(string strSQL)
        {
            var index = strSQL.IndexOf(" from ");
            var strFooter = strSQL.Substring(index, strSQL.Length - index);
            var strText = "select count(*) " + strFooter;

            return strText;
        }

        public static string BuilderSQL(object entity, string strSql, IDbDataParameter[] parameters)
        {
            if (AdoHelper.DbType == DatabaseType.ACCESS || AdoHelper.DbType == DatabaseType.SQLSERVER)
            {
                foreach (IDbDataParameter param in parameters)
                {
                    if (param.Value == null) continue;

                    var paramName = param.ParameterName;
                    var paramValue = param.Value.ToString();
                    var type = ReflectionHelper.GetPropertyType(entity, paramName);

                    if (type == "System.String" || type == "System.DateTime")
                    {
                        paramValue = "'" + paramValue + "'";
                    }

                    strSql = strSql.Replace("@" + paramName, paramValue);
                }
            }

            return strSql;
        }

        public static string BuilderSQL(string strSql, IDbDataParameter[] parameters)
        {
            if (AdoHelper.DbType == DatabaseType.ACCESS || AdoHelper.DbType == DatabaseType.SQLSERVER)
            {
                foreach (IDbDataParameter param in parameters)
                {
                    if (param.Value == null) continue;

                    var paramName = param.ParameterName;
                    var paramValue = param.Value.ToString();
                    strSql = strSql.Replace("@" + paramName, paramValue);
                }
            }

            return strSql;
        }
    }
}