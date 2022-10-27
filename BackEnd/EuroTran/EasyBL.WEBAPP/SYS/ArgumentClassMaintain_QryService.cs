using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.SYS
{
    public class ArgumentClassMaintain_QryService : ServiceBase
    {
        #region 參數類別管理（分頁查詢）

        /// <summary>
        /// 參數類別管理（分頁查詢）
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

                    var sArgumentClassID = _fetchString(i_crm, @"ArgumentClassID");
                    var sArgumentClassName = _fetchString(i_crm, @"ArgumentClassName");
                    var sEffective = _fetchString(i_crm, @"Effective");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    pml.DataList = db.Queryable<OTB_SYS_ArgumentClass>()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.DelStatus != "Y" && x.ArgumentClassID.Contains(sArgumentClassID) && x.ArgumentClassName.Contains(sArgumentClassName) && sEffective.Contains(x.Effective))
                        .Select(x => new View_SYS_ArgumentClass
                        {
                            ArgumentClassID = SqlFunc.GetSelfAndAutoFill(x.ArgumentClassID),
                            OrderCount = SqlFunc.Subqueryable<OTB_SYS_ArgumentClass>().Where(p => p.DelStatus != "Y" && p.OrgID == x.OrgID).Count()
                        })
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount);
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.SYS.ArgumentClassMaintain_QryService", "", "QueryPage（參數類別管理（分頁查詢））", "", "", "");
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

        #endregion 參數類別管理（分頁查詢）

        #region 參數類別管理（更新排序）

        /// <summary>
        /// 參數類別管理（更新排序）
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
                        var sId = _fetchString(i_crm, @"Id");
                        var iOldOrderByValue = _fetchInt(i_crm, @"OldOrderByValue");
                        var iNewOrderByValue = _fetchInt(i_crm, @"NewOrderByValue");

                        var oOrderEntity = db.Queryable<OTB_SYS_ArgumentClass>().Single(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == sId);

                        if (iNewOrderByValue > iOldOrderByValue)
                        {
                            var iRelUp = db.Updateable<OTB_SYS_ArgumentClass>()
                                           .UpdateColumns(x => new OTB_SYS_ArgumentClass { OrderByValue = x.OrderByValue - 1 })
                                           .Where(x => x.OrgID == oOrderEntity.OrgID && x.OrderByValue <= iNewOrderByValue && x.OrderByValue > iOldOrderByValue)
                                           .ExecuteCommand();
                        }
                        else
                        {
                            var iRelDown = db.Updateable<OTB_SYS_ArgumentClass>()
                                             .UpdateColumns(x => new OTB_SYS_ArgumentClass { OrderByValue = x.OrderByValue + 1 })
                                             .Where(x => x.OrgID == oOrderEntity.OrgID && x.OrderByValue >= iNewOrderByValue && x.OrderByValue < iOldOrderByValue)
                                             .ExecuteCommand();
                        }
                        var iRelSelf = db.Updateable(new OTB_SYS_ArgumentClass { OrderByValue = iNewOrderByValue })
                                         .UpdateColumns(x => x.OrderByValue)
                                         .Where(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == sId).ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRelSelf);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentClassMaintain_QryService), @"參數類別管理", @"UpdateOrderByValue（參數類別管理（更新排序））", @"", @"", @"");
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

        #endregion 參數類別管理（更新排序）
    }
}