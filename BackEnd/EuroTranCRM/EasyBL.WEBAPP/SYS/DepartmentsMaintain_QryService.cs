using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyBL.WEBAPP.SYS
{
    public class DepartmentsMaintain_QryService : ServiceBase
    {
        #region 部門管理（分頁查詢）

        /// <summary>
        /// 部門管理（分頁查詢）
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

                    var sDepartmentID = _fetchString(i_crm, @"DepartmentID");
                    var sDepartmentName = _fetchString(i_crm, @"DepartmentName");
                    var sEffective = _fetchString(i_crm, @"Effective");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    pml.DataList = db.Queryable<OTB_SYS_Departments>()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.DelStatus != "Y" && x.DepartmentID.Contains(sDepartmentID) && x.DepartmentName.Contains(sDepartmentName) && sEffective.Contains(x.Effective))
                        .Select(x => new View_SYS_Departments
                        {
                            DepartmentID = SqlFunc.GetSelfAndAutoFill(x.DepartmentID),
                            OrderCount = SqlFunc.Subqueryable<OTB_SYS_Departments>().Where(p => p.DelStatus != "Y" && p.OrgID == x.OrgID).Count()
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(DepartmentsMaintain_QryService), "", "QueryPage（部門管理（分頁查詢））", "", "", "");
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

        #endregion 部門管理（分頁查詢）

        #region 部門管理（更新排序）

        /// <summary>
        /// 部門管理（更新排序）
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

                        var oOrderEntity = db.Queryable<OTB_SYS_Departments>().Single(x => x.OrgID == i_crm.ORIGID && x.DepartmentID == sId);

                        if (iNewOrderByValue > iOldOrderByValue)
                        {
                            var iRelUp = db.Updateable<OTB_SYS_Departments>()
                                           .UpdateColumns(x => new OTB_SYS_Departments { OrderByValue = x.OrderByValue - 1 })
                                           .Where(x => x.OrgID == oOrderEntity.OrgID && x.OrderByValue <= iNewOrderByValue && x.OrderByValue > iOldOrderByValue)
                                           .ExecuteCommand();
                        }
                        else
                        {
                            var iRelDown = db.Updateable<OTB_SYS_Departments>()
                                             .UpdateColumns(x => new OTB_SYS_Departments { OrderByValue = x.OrderByValue + 1 })
                                             .Where(x => x.OrgID == oOrderEntity.OrgID && x.OrderByValue >= iNewOrderByValue && x.OrderByValue < iOldOrderByValue)
                                             .ExecuteCommand();
                        }
                        var iRelSelf = db.Updateable(new OTB_SYS_Departments { OrderByValue = iNewOrderByValue })
                                         .UpdateColumns(x => x.OrderByValue)
                                         .Where(x => x.OrgID == i_crm.ORIGID && x.DepartmentID == sId)
                                         .ExecuteCommand();

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRelSelf);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(DepartmentsMaintain_QryService), @"部門管理", @"UpdateOrderByValue（部門管理（更新排序））", @"", @"", @"");
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

        #endregion 部門管理（更新排序）
    }
}