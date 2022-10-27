using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.SYS
{
    public class ProgramMaintain_QryService : ServiceBase
    {
        #region 程式管理（分頁查詢）

        /// <summary>
        /// 程式管理（分頁查詢）
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
                    var sProgramCode = _fetchString(i_crm, @"ProgramCode");
                    var sProgramName = _fetchString(i_crm, @"ProgramName");
                    var sEffective = _fetchString(i_crm, @"Effective");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    pml.DataList = db.Queryable<OTB_SYS_ProgramList, OTB_SYS_ModuleForProgram, OTB_SYS_ModuleList>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.ProgramID == t2.ProgramID,
                                JoinType.Left, t2.OrgID == t3.OrgID && t2.ModuleID == t3.ModuleID
                              }
                        )
                        .Where((t1, t2, t3) => t1.OrgID == i_crm.ORIGID && t1.ProgramID.Contains(sProgramCode) && t1.ProgramName.Contains(sProgramName) && sEffective.Contains(t1.Effective))
                        .WhereIF(!string.IsNullOrEmpty(sModuleID), (t1, t2, t3) => t2.ModuleID == sModuleID)
                        .Select((t1, t2, t3) => new View_SYS_ProgramList
                        {
                            OrgID = t1.OrgID,
                            ProgramID = t1.ProgramID,
                            ModuleName = t3.ModuleName,
                            ProgramName = t1.ProgramName,
                            ModuleID = t2.ModuleID,
                            FilePath = t1.FilePath,
                            ImgPath = t1.ImgPath,
                            AllowRight = SqlFunc.Trim(t1.AllowRight),
                            ModOrderBy = t3.OrderByValue,
                            OrderByValue = t2.OrderByValue,
                            ProgramType = t1.ProgramType,
                            BackgroundCSS = t1.BackgroundCSS,
                            GroupTag = t1.GroupTag,
                            Effective = t1.Effective,
                            ShowInList = t1.ShowInList,
                            ShowInHome = t1.ShowInHome,
                            MainTableName = t1.MainTableName,
                            ShowTop = t1.ShowTop,
                            Memo = t1.Memo,
                            CreateUser = t1.CreateUser,
                            CreateDate = t1.CreateDate,
                            ModifyUser = t1.ModifyUser,
                            ModifyDate = t1.ModifyDate,
                            AllModuleID = t1.ModuleID,
                            ProgramTypeName = SqlFunc.IF(t1.ProgramType == "P").Return("程式").ElseIF(t1.ProgramType == "R").Return("報表").End("子程式"),
                            EffectiveName = SqlFunc.IF(t1.Effective == "Y").Return("啟用").ElseIF(t1.Effective == "N").Return("停用").End("維修中"),
                            ShowInListName = SqlFunc.IIF(t1.ShowInList == "Y", "顯示", "不顯示"),
                            OrderCount = SqlFunc.Subqueryable<OTB_SYS_ModuleForProgram>().Where(p => p.ModuleID == t2.ModuleID && p.OrgID == t2.OrgID).Count()
                        })
                        .MergeTable()
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.SYS.ProgramMaintain_QryService", "", "QueryPage（程式管理（分頁查詢））", "", "", "");
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

        #endregion 程式管理（分頁查詢）

        #region 程式管理（更新排序）

        /// <summary>
        /// 程式管理（更新排序）
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
                        var sParentId = _fetchString(i_crm, @"ParentId");
                        var sId = _fetchString(i_crm, @"Id");
                        var iOldOrderByValue = _fetchInt(i_crm, @"OldOrderByValue");
                        var iNewOrderByValue = _fetchInt(i_crm, @"NewOrderByValue");

                        var oOrderEntity = db.Queryable<OTB_SYS_ModuleForProgram>()
                                             .Single(x => x.OrgID == i_crm.ORIGID && x.ModuleID == sParentId && x.ProgramID == sId);

                        if (iNewOrderByValue > iOldOrderByValue)
                        {
                            var iRelUp = db.Updateable<OTB_SYS_ModuleForProgram>()
                                           .UpdateColumns(x => new OTB_SYS_ModuleForProgram { OrderByValue = x.OrderByValue - 1 })
                                           .Where(x => x.OrgID == oOrderEntity.OrgID && x.ModuleID == sParentId && x.OrderByValue <= iNewOrderByValue && x.OrderByValue > iOldOrderByValue)
                                           .ExecuteCommand();
                        }
                        else
                        {
                            var iRelDown = db.Updateable<OTB_SYS_ModuleForProgram>()
                                             .UpdateColumns(x => new OTB_SYS_ModuleForProgram { OrderByValue = x.OrderByValue + 1 })
                                             .Where(x => x.OrgID == oOrderEntity.OrgID && x.ModuleID == sParentId && x.OrderByValue >= iNewOrderByValue && x.OrderByValue < iOldOrderByValue)
                                             .ExecuteCommand();
                        }
                        var iRelSelf = db.Updateable(new OTB_SYS_ModuleForProgram { OrderByValue = iNewOrderByValue })
                                         .UpdateColumns(x => x.OrderByValue)
                                         .Where(x => x.OrgID == i_crm.ORIGID && x.ModuleID == sParentId && x.ProgramID == sId)
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentMaintain_QryService), @"程式管理管理", @"UpdateOrderByValue（程式管理管理（更新排序））", @"", @"", @"");
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

        #endregion 程式管理（更新排序）
    }
}