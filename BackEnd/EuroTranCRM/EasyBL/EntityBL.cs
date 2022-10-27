using EasyNet.Common;
using EasyNet.DBUtility;
using Entity;
using SqlSugar.Base;
using System;
using System.Collections.Generic;

namespace EasyBL
{
    public class EntityBL
    {
        public static Object QueryOne(string sType, Object oParam)
        {
            Object rel = null;

            var sSqlCommond = SqlCommand.GetSqlCommand(sType);

            rel = DBHelper.QueryOne(sSqlCommond, oParam);

            return rel;
        }

        public static Object QueryList(string sType, Object oParam)
        {
            Object rel = null;

            var sSqlCommond = SqlCommand.GetSqlCommand(sType);

            rel = DBHelper.QueryList(sSqlCommond, oParam);

            return rel;
        }

        public static PageResult QueryPage(Object oParam)
        {
            var pm = new ParamMap();
            pm.SetPageParamters(oParam);
            return new DBHelper().QueryPage(pm);
        }

        public static PageResult QueryPageByPrc(string sType, Object oParam, bool bCount)
        {
            var sSqlCommond = SqlCommand.GetSqlCommand(sType);
            return DBHelper.QueryPageByPrc(sSqlCommond, oParam, bCount);
        }

        public static Object GetTableByPrc(string sType, Object oParam)
        {
            Object res = null;
            var sSqlCommond = SqlCommand.GetSqlCommand(sType);
            var dic = oParam as Dictionary<string, object>;
            var db = SugarBase.DB;
            res = db.Ado.UseStoredProcedure().GetDataTable(sSqlCommond, dic);
            return res;
        }

        public static int ExecuteSqlTran(Object oParm)
        {
            var Rel = DBHelper.ExecuteSqlTran(oParm);

            return Rel;
        }
    }
}