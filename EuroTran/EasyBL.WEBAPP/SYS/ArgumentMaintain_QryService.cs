using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;

namespace EasyBL.WEBAPP.SYS
{
    public class ArgumentMaintain_QryService : ServiceBase
    {
        #region 參數值（分頁資料）

        /// <summary>
        /// 參數值（分頁資料）
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
                    var iPageCount = 0;
                    var sSortField = _fetchString(i_crm, @"sortField");
                    var sSortOrder = _fetchString(i_crm, @"sortOrder");
                    var sArgumentClassID = _fetchString(i_crm, @"ArgumentClassID");
                    var sArgumentID = _fetchString(i_crm, @"ArgumentID");
                    var sArgumentValue = _fetchString(i_crm, @"ArgumentValue");
                    var sEffective = _fetchString(i_crm, @"Effective");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    pml.DataList = db.Queryable<OTB_SYS_Arguments, OTB_SYS_ArgumentClass, OTB_SYS_Arguments>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.ArgumentClassID == t2.ArgumentClassID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ArgumentClassID == t3.ArgumentClassID && t1.ParentArgument==t3.ArgumentID
                              }
                        )
                        .Where((t1, t2, t3) => t1.OrgID == i_crm.ORIGID && t1.DelStatus == "N" && t2.DelStatus == "N" && t1.ArgumentID.Contains(sArgumentID) && t1.ArgumentValue.Contains(sArgumentValue) && sEffective.Contains(t1.Effective))
                        .WhereIF(!string.IsNullOrEmpty(sArgumentClassID), (t1, t2, t3) => t1.ArgumentClassID == sArgumentClassID)
                        .Select((t1, t2, t3) => new View_SYS_Arguments
                        {
                            ArgumentID = SqlFunc.GetSelfAndAutoFill(t1.ArgumentID),
                            ArgumentClassName = t2.ArgumentClassName,
                            ParentArgumentName = t2.ArgumentClassName + "-" + t3.ArgumentValue,
                            OrderCount = SqlFunc.Subqueryable<OTB_SYS_Arguments>().Where(p => p.ArgumentClassID == t1.ArgumentClassID && p.OrgID == t1.OrgID && p
                            .DelStatus != "Y").Count()
                        })
                        .MergeTable()
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    if (bExcel)
                    {
                        const string sFileName = "參數值";
                        var oHeader = new Dictionary<string, string>
                        {
                            { "RowIndex", "項次" },
                            { "ArgumentClassName", "參數類別" },
                            { "ParentArgumentName", "父層" },
                            { "ArgumentID", "參數值" },
                            { "ArgumentValue", "參數值說明" },
                            { "Effective", "狀態（Y:有效；N：無效）" }
                        };

                        var dicAlain = ExcelService.GetExportAlain(oHeader, "Effective");
                        var saArguments = pml.DataList as List<View_SYS_Arguments>;
                        var bOk = new ExcelService().CreateExcelByList(saArguments, out string sPath, oHeader, dicAlain, sFileName);
                        rm.DATA.Add(BLWording.REL, sPath);
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentMaintain_QryService), @"參數值", @"QueryPage（參數值（分頁資料））", @"", @"", @"");
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

        #endregion 參數值（分頁資料）

        #region 參數類別（多筆）

        /// <summary>
        /// 參數類別（多筆）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage QueryList(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var saArgumentClass = db.Queryable<OTB_SYS_ArgumentClass>()
                                            .Where(x => x.OrgID == i_crm.ORIGID && x.Effective == "Y" && x.DelStatus == "N")
                                            .OrderBy(x => x.OrderByValue)
                                            .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saArgumentClass);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentMaintain_QryService), "", "QueryList（參數類別（多筆））", "", "", "");
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

        #endregion 參數類別（多筆）

        #region 參數值（更新排序）

        /// <summary>
        /// 參數值（更新排序）
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

                        var oOrderEntity = db.Queryable<OTB_SYS_Arguments>()
                                             .Single(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == sParentId && x.ArgumentID == sId);

                        if (iNewOrderByValue > iOldOrderByValue)
                        {
                            var iRelUp = db.Updateable<OTB_SYS_Arguments>()
                                           .UpdateColumns(x => new OTB_SYS_Arguments { OrderByValue = x.OrderByValue - 1 })
                                           .Where(x => x.OrgID == oOrderEntity.OrgID && x.ArgumentClassID == sParentId && x.DelStatus == "N" && x.OrderByValue <= iNewOrderByValue && x.OrderByValue > iOldOrderByValue)
                                           .ExecuteCommand();
                        }
                        else
                        {
                            var iRelDown = db.Updateable<OTB_SYS_Arguments>()
                                             .UpdateColumns(x => new OTB_SYS_Arguments { OrderByValue = x.OrderByValue + 1 })
                                             .Where(x => x.OrgID == oOrderEntity.OrgID && x.ArgumentClassID == sParentId && x.DelStatus == "N" && x.OrderByValue >= iNewOrderByValue && x.OrderByValue < iOldOrderByValue)
                                             .ExecuteCommand();
                        }
                        var iRelSelf = db.Updateable(new OTB_SYS_Arguments { OrderByValue = iNewOrderByValue })
                                         .UpdateColumns(x => x.OrderByValue)
                                         .Where(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == sParentId && x.ArgumentID == sId)
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentMaintain_QryService), @"參數值管理", @"UpdateOrderByValue（參數值管理（更新排序））", @"", @"", @"");
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

        #endregion 參數值（更新排序）
    }
}