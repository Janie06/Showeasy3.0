using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.SYS
{
    public class ModuleMaintain_QryService : ServiceBase
    {
        #region 模組管理（分頁查詢）

        /// <summary>
        /// 模組管理（分頁查詢）
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

                    var sModuleID = _fetchString(i_crm, @"ModuleID");
                    var sModuleName = _fetchString(i_crm, @"ModuleName");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    pml.DataList = db.Queryable<OTB_SYS_ModuleList>()
                        .Select(x => new View_SYS_ModuleList
                        {
                            ModuleID = SqlFunc.GetSelfAndAutoFill(x.ModuleID),
                            ProCount = SqlFunc.ToInt32(SqlFunc.MappingColumn(x.ModuleID, "dbo.[OFN_SYS_GetProgramCountByModuleID](x.OrgID,x.ModuleID)")),
                            OrderCount = SqlFunc.Subqueryable<OTB_SYS_ModuleList>().Where(p => p.OrgID == x.OrgID).Count()
                        })
                        .MergeTable()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.ModuleID.Contains(sModuleID) && x.ModuleName.Contains(sModuleName))
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ModuleMaintain_QryService), "", "QueryPage（模組管理（分頁查詢））", "", "", "");
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

        #endregion 模組管理（分頁查詢）

        #region 模組管理（更新排序）

        /// <summary>
        /// 模組管理（更新排序）
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

                        var oOrderEntity = db.Queryable<OTB_SYS_ModuleList>().Single(x => x.OrgID == i_crm.ORIGID && x.ModuleID == sId);

                        if (iNewOrderByValue > iOldOrderByValue)
                        {
                            var iRelUp = db.Updateable<OTB_SYS_ModuleList>()
                                           .UpdateColumns(x => new OTB_SYS_ModuleList { OrderByValue = x.OrderByValue - 1 })
                                           .Where(x => x.OrgID == oOrderEntity.OrgID && x.OrderByValue <= iNewOrderByValue && x.OrderByValue > iOldOrderByValue)
                                           .ExecuteCommand();
                        }
                        else
                        {
                            var iRelDown = db.Updateable<OTB_SYS_ModuleList>()
                                             .UpdateColumns(x => new OTB_SYS_ModuleList { OrderByValue = x.OrderByValue + 1 })
                                             .Where(x => x.OrgID == oOrderEntity.OrgID && x.OrderByValue >= iNewOrderByValue && x.OrderByValue < iOldOrderByValue)
                                             .ExecuteCommand();
                        }
                        var iRelSelf = db.Updateable(new OTB_SYS_ModuleList { OrderByValue = iNewOrderByValue })
                                         .UpdateColumns(x => x.OrderByValue)
                                         .Where(x => x.OrgID == i_crm.ORIGID && x.ModuleID == sId)
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ModuleMaintain_QryService), @"模組管理", @"UpdateOrderByValue（模組管理（更新排序））", @"", @"", @"");
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

        #endregion 模組管理（更新排序）
    }
}