using EasyNet.Common;
using EasyNet.Manager;
using System;

namespace EasyNet.DBUtility
{
    public class DBHelper
    {
        private DbManager m;

        public DBHelper()
        {
            m = DbManager.PriviteInstance();
        }

        public static DBHelper GetInstance() => new DBHelper();

        /// <summary>
        /// 自定義刪除資料
        /// </summary>
        /// <param name="obj">todo: describe obj parameter on Delete</param>
        /// <typeparam name="param">sql</typeparam>
        /// <returns></returns>
        public int Delete(Object obj) => m.Delete(obj);

        /// <summary>
        /// 自定義修改資料
        /// </summary>
        /// <param name="obj">todo: describe obj parameter on Update</param>
        /// <typeparam name="param">sql</typeparam>
        /// <returns></returns>
        public int Update(Object obj) => m.Update(obj);

        /// <summary>
        /// 自定義修改資料(MasterDteil)
        /// </summary>
        /// <param name="obj">todo: describe obj parameter on UpdateTran</param>
        /// <typeparam name="param">sql</typeparam>
        /// <returns></returns>
        public int UpdateTran(Object obj) => m.UpdateTran(obj);

        /// <summary>
        /// 自定義修改資料
        /// </summary>
        /// <param name="obj">todo: describe obj parameter on Insert</param>
        /// <typeparam name="param">sql</typeparam>
        /// <returns></returns>
        public int Insert(Object obj) => m.Insert(obj);

        /// <summary>
        /// 執行預存函數
        /// </summary>
        /// <param name="param">參數</param>
        /// <returns></returns>
        public static int ExecuteSqlTran(Object param) => DbManager.ExecuteSqlTran(param);

        /// <summary>
        /// 根據SQL查詢資料
        /// </summary>
        /// <param name="sSql">SQL命令</param>
        /// <param name="obj">todo: describe obj parameter on QueryList</param>
        /// <typeparam name="T">對象類型</typeparam>
        /// <returns></returns>
        public static Object QueryList(string sSql, Object obj) => DbManager.QueryList(sSql, obj);

        /// <summary>
        /// 根據SQL查詢資料
        /// </summary>
        /// <param name="obj">todo: describe obj parameter on QueryCount</param>
        /// <typeparam name="T">對象類型</typeparam>
        /// <returns></returns>
        public static int QueryCount(Object obj) => DbManager.QueryCount(obj);

        /// <summary>
        /// 根據SQL查詢單筆資料
        /// </summary>
        /// <param name="sSql">SQL命令</param>
        /// <param name="obj">todo: describe obj parameter on QueryOne</param>
        /// <typeparam name="T">對象類型</typeparam>
        /// <returns></returns>
        public static Object QueryOne(string sSql, Object obj) => DbManager.QueryOne(sSql, obj);

        /// <summary>
        /// 分頁查詢返回分頁對象
        /// </summary>
        /// <typeparam name="T">對象類型</typeparam>
        /// <param name="param">參數</param>
        /// <returns></returns>
        public PageResult QueryPage(ParamMap param) => m.QueryPage(param);

        /// <summary>
        /// 分頁查詢返回分頁對象
        /// </summary>
        /// <param name="sSql">SQL命令</param>
        /// <param name="param">參數</param>
        /// <param name="bCount">todo: describe bCount parameter on QueryPageByPrc</param>
        /// <typeparam name="T">對象類型</typeparam>
        /// <returns></returns>
        public static PageResult QueryPageByPrc(string sSql, Object param, bool bCount) => DbManager.QueryPageByPrc(sSql, param, bCount);

        /// <summary>
        /// 開啟事務
        /// </summary>
        public void BeginTransaction()
        {
            m.BeginTransaction();
        }

        /// <summary>
        /// 提交事務
        /// </summary>
        public void CommitTransaction()
        {
            m.Commit();
        }

        /// <summary>
        /// 回滾事務
        /// </summary>
        public void RollbackTransaction()
        {
            m.Rollback();
        }
    }
}