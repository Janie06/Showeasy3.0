using EasyBL.WebApi.Message;
using Entity.Sugar;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.SYS
{
    public class CurrencySetup_UpdService : ServiceBase
    {
        #region 幣別匯率（單筆查詢）

        /// <summary>
        /// 幣別匯率（單筆查詢）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage QueryOne(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    string sYear = _fetchString(i_crm, @"year");
                    string sMonth = _fetchString(i_crm, @"month");
                    string sCurrency = _fetchString(i_crm, @"currency");

                    var oEntity = db.Queryable<OTB_SYS_Currency>()
                        .Where((x) => x.year.ToString() == sYear
                                && x.month.ToString() == sMonth
                                && x.currency.ToString() == sCurrency
                                && x.OrgID == i_crm.ORIGID)
                        .Single();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CurrencySetup_UpdService), "", "QueryOne（幣別匯率（單筆查詢））", "", "", "");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion

        #region 幣別匯率（新增）

        /// <summary>
        /// 幣別匯率（新增）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage Insert(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var oEntity = _fetchEntity<OTB_SYS_Currency>(i_crm);
                        _setEntityBase(oEntity, i_crm);
                        int iRel = db.Insertable(oEntity).ExecuteCommand();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CurrencySetup_UpdService), @"幣別匯率", @"Add（幣別匯率（新增））", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion

        #region 幣別匯率（修改）

        /// <summary>
        /// 幣別匯率（修改）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage Update(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var oNewEntity = _fetchEntity<OTB_SYS_Currency>(i_crm);
                        _setEntityBase(oNewEntity, i_crm);
                        var iRel = db.Updateable(oNewEntity)
                            .IgnoreColumns(x => new
                            {
                                x.CreateUser,
                                x.CreateDate
                            }).ExecuteCommand();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CurrencySetup_UpdService), @"幣別匯率", @"Update（幣別匯率（修改））", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion

        #region 幣別匯率（刪除）

        /// <summary>
        /// 幣別匯率（刪除）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage Delete(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        string sYear = _fetchString(i_crm, @"year");
                        string sMonth = _fetchString(i_crm, @"month");
                        string sCurrency = _fetchString(i_crm, @"currency");

                        var iRel = db.Deleteable<OTB_SYS_Currency>()
                                        .Where(x => x.year.ToString() == sYear
                                                && x.month.ToString() == sMonth
                                                && x.currency.ToString() == sCurrency
                                                && x.OrgID == i_crm.ORIGID)
                                        .ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CurrencySetup_UpdService), @"幣別匯率", @"Delete（幣別匯率（刪除））", @"", @"", @"");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion

        #region 幣別匯率（查詢筆數）

        /// <summary>
        /// 幣別匯率（查詢筆數）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage QueryCout(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    string sYear = _fetchString(i_crm, @"year");
                    string sMonth = _fetchString(i_crm, @"month");
                    string sCurrency = _fetchString(i_crm, @"currency");

                    var iCout = db.Queryable<OTB_SYS_Currency>()
                        .Where(x => x.year.ToString() == sYear
                                && x.month.ToString() == sMonth
                                && x.currency.ToString() == sCurrency
                                && x.OrgID == i_crm.ORIGID)
                        .Count();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iCout);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CurrencySetup_UpdService), "", "QueryCout（幣別匯率（查詢筆數））", "", "", "");
            }
            finally
            {
                if (null != sMsg)
                {
                    rm = new ErrorResponseMessage(sMsg, i_crm);
                }
            }
            return rm;
        }

        #endregion
    }
}