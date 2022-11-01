using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.WSM
{
    public class News_QryService : ServiceBase
    {
        #region 最新消息（分頁查詢）

        /// <summary>
        /// 最新消息（分頁查詢）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage QueryPage(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var pml = new PageModel
                    {
                        PageIndex = _fetchInt(i_crm, @"pageIndex"),
                        PageSize = _fetchInt(i_crm, @"pageSize")
                    };
                    var iPageCount = 0;
                    var sSortField = _fetchString(i_crm, @"sortField");
                    var sSortOrder = _fetchString(i_crm, @"sortOrder");

                    var sNews_Type = _fetchString(i_crm, @"News_Type");
                    var sNews_LanguageType = _fetchString(i_crm, @"News_LanguageType");
                    var sNews_Title = _fetchString(i_crm, @"News_Title");
                    var sNews_Show = _fetchString(i_crm, @"News_Show");
                    var sNews_StartDate = _fetchString(i_crm, @"News_StartDate");
                    var sNews_EndDate = _fetchString(i_crm, @"News_EndDate");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    var rNews_StartDate = new DateTime();
                    var rNews_EndDate = new DateTime();
                    if (!string.IsNullOrEmpty(sNews_StartDate))
                    {
                        rNews_StartDate = SqlFunc.ToDate(sNews_StartDate);
                    }
                    if (!string.IsNullOrEmpty(sNews_EndDate))
                    {
                        rNews_EndDate = SqlFunc.ToDate(sNews_EndDate).AddDays(1);
                    }

                    pml.DataList = db.Queryable<OTB_WSM_News, OTB_SYS_Arguments, OTB_SYS_Arguments>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.News_Type == t2.ArgumentID && t2.ArgumentClassID == "News_Class",
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.News_LanguageType == t3.ArgumentID && t3.ArgumentClassID == "LanCountry"
                              }
                        )
                        .Where((t1, t2, t3) => t1.OrgID == i_crm.ORIGID && t1.News_Title.Contains(sNews_Title) && sNews_Show.Contains(t1.News_Show))
                        .WhereIF(!string.IsNullOrEmpty(sNews_Type), (t1, t2, t3) => t1.News_Type == sNews_Type)
                        .WhereIF(!string.IsNullOrEmpty(sNews_LanguageType), (t1, t2, t3) => t1.News_LanguageType == sNews_LanguageType)
                        .WhereIF(!string.IsNullOrEmpty(sNews_StartDate), (t1, t2, t3) => t1.News_EndDete >= rNews_StartDate.Date)
                        .WhereIF(!string.IsNullOrEmpty(sNews_EndDate), (t1, t2, t3) => t1.News_StartDete <= rNews_EndDate.Date)
                        .Select((t1, t2, t3) => new View_WSM_News
                        {
                            SN = SqlFunc.GetSelfAndAutoFill(t1.SN),
                            News_TypeName = t2.ArgumentValue,
                            News_LanguageTypeName = t3.ArgumentValue,
                            OrderCount = SqlFunc.Subqueryable<OTB_WSM_News>().Where(p => p.News_LanguageType == t1.News_LanguageType && p.OrgID == t1.OrgID).Count()
                        })
                        .MergeTable()
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    if (bExcel)
                    {
                    }
                    else
                    {
                        rm.DATA.Add(BLWording.REL, pml);
                    }
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.WSM.News_QryService", "", "QueryPage（最新消息（分頁查詢））", "", "", "");
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

        #endregion 最新消息（分頁查詢）

        #region 最新消息（更新排序）

        /// <summary>
        /// 最新消息（更新排序）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage UpdateOrderByValue(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var iId = _fetchInt(i_crm, @"Id");
                        var iOldOrderByValue = _fetchInt(i_crm, @"OldOrderByValue");
                        var iNewOrderByValue = _fetchInt(i_crm, @"NewOrderByValue");

                        var oOrderEntity = db.Queryable<OTB_WSM_News>().Single(x => x.SN == iId);

                        if (iNewOrderByValue > iOldOrderByValue)
                        {
                            var iRelUp = db.Updateable<OTB_WSM_News>()
                                           .UpdateColumns(x => new OTB_WSM_News { OrderByValue = x.OrderByValue - 1 })
                                           .Where(x => x.OrgID == oOrderEntity.OrgID && x.News_Type == oOrderEntity.News_Type && x.News_LanguageType == oOrderEntity.News_LanguageType && x.OrderByValue <= iNewOrderByValue && x.OrderByValue > iOldOrderByValue)
                                           .ExecuteCommand();
                        }
                        else
                        {
                            var iRelDown = db.Updateable<OTB_WSM_News>()
                                             .UpdateColumns(x => new OTB_WSM_News { OrderByValue = x.OrderByValue + 1 })
                                             .Where(x => x.OrgID == oOrderEntity.OrgID && x.News_Type == oOrderEntity.News_Type && x.News_LanguageType == oOrderEntity.News_LanguageType && x.OrderByValue >= iNewOrderByValue && x.OrderByValue < iOldOrderByValue)
                                             .ExecuteCommand();
                        }
                        var iRelSelf = db.Updateable(new OTB_WSM_News { OrderByValue = iNewOrderByValue })
                                         .UpdateColumns(x => x.OrderByValue)
                                         .Where(x => x.SN == iId).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRelSelf);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(WebSiteSetupService), @"最新消息", @"UpdateOrderByValue（最新消息（更新排序））", @"", @"", @"");
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

        #endregion 最新消息（更新排序）
    }
}