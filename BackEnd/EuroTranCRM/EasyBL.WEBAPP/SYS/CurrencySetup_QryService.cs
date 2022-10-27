using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;

namespace EasyBL.WEBAPP.SYS
{
    public class CurrencySetup_QryService : ServiceBase
    {
        #region 幣別匯率（分頁資料）

        /// <summary>
        /// 幣別匯率（分頁資料）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on QueryPage</param>
        /// <returns></returns>
        public ResponseMessage QueryPage(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var pml = new PageModel
                    {
                        PageIndex = _fetchInt(i_crm, @"pageIndex"),
                        PageSize = _fetchInt(i_crm, @"pageSize")
                    };
                    int iPageCount = 0;
                    string sSortField = _fetchString(i_crm, @"sortField");
                    string sSortOrder = _fetchString(i_crm, @"sortOrder");
                    string sYear = _fetchString(i_crm, @"year");
                    string sMonth = _fetchString(i_crm, @"month");
                    string sCurrency = _fetchString(i_crm, @"currency");

                    pml.DataList = db.Queryable<OTB_SYS_Currency, OTB_SYS_Arguments>
                        ((t1, t2) =>
                        new object[] {
                                JoinType.Inner, t1.currency == t2.ArgumentID && t2.ArgumentClassID == "Currency" && t2.OrgID == i_crm.ORIGID
                              }
                        )
                        .Where((t1, t2) => t1.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sYear), (t1, t2) => t1.year.ToString() == sYear)
                        .WhereIF(!string.IsNullOrEmpty(sMonth), (t1, t2) => t1.month.ToString() == sMonth)
                        .WhereIF(!string.IsNullOrEmpty(sCurrency), (t1, t2) => t1.currency.ToString() == sCurrency)
                        .OrderBy(sSortField, sSortOrder)
                        .Select((t1, t2) => new OTB_SYS_Currency
                        {
                            year = t1.year,
                            month = t1.month,
                            exchange_rate = t1.exchange_rate,
                            currency = t1.currency
                        })
                        .ToPageList(pml.PageIndex, pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);

                    rm.DATA.Add(BLWording.REL, pml);

                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CurrencySetup_QryService), @"幣別匯率", @"QueryPage（幣別匯率（分頁資料））", @"", @"", @"");
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

        #region 幣別查詢（多筆）

        /// <summary>
        /// 幣別查詢（多筆）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage CurrencyList(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var saArgumentClass = db.Queryable<OTB_SYS_Arguments>()
                                            .Where(x => x.OrgID == i_crm.ORIGID
                                                    && x.ArgumentClassID == "Currency"
                                                    && x.Effective == "Y"
                                                    && x.DelStatus == "N")
                                            .OrderBy(x => x.ArgumentID)
                                            .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saArgumentClass);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CurrencySetup_QryService), "", "QueryList（參數類別（多筆））", "", "", "");
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

        #region GetCurrencyByYear

        /// <summary>
        /// GetCurrencyByYear
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetCurrencyByYear(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    string spYear = _fetchString(i_crm, @"year");
                    DateTime dtParse;

                    if (!DateTime.TryParse(spYear, out dtParse))
                    {
                        sMsg = "DateTime Parse Error";
                        break;
                    }

                    string sYear = dtParse.Year.ToString();
                    string sMonth = dtParse.Month.ToString();

                    var saCurrency = db.Queryable<OTB_SYS_Currency, OTB_SYS_Arguments>
                        ((t1, t2) =>
                        new object[] {
                                JoinType.Inner, t1.currency == t2.ArgumentID && t2.ArgumentClassID == "Currency" && t2.OrgID == i_crm.ORIGID
                              }
                        )
                        .Where((t1, t2) => t1.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sYear), (t1, t2) => t1.year.ToString() == sYear)
                        .WhereIF(!string.IsNullOrEmpty(sMonth), (t1, t2) => t1.month.ToString() == sMonth)
                        .Select((t1, t2) => new OTB_SYS_Arguments
                        {
                            ArgumentID = t1.currency,
                            ArgumentValue = t2.ArgumentValue,
                            Correlation = t1.exchange_rate.ToString()
                        })
                        .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);


                    foreach (OTB_SYS_Arguments item in saCurrency as List<OTB_SYS_Arguments>)
                    {
                        decimal dcExchangeRate;

                        if (decimal.TryParse(item.Correlation, out dcExchangeRate))
                        {
                            item.Correlation = dcExchangeRate.ToString("#0.##");
                        }
                    }

                    rm.DATA.Add(BLWording.REL, saCurrency);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(CurrencySetup_QryService), "", "QueryList（參數類別（多筆））", "", "", "");
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