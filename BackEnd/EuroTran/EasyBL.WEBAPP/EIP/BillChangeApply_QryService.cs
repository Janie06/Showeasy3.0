using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;

namespace EasyBL.WEBAPP.EIP
{
    public class BillChangeApply_QryService : ServiceBase
    {
        #region 帳單更改申請單分頁查詢

        /// <summary>
        /// 帳單更改申請單分頁查詢
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

                    var sKeyNote = _fetchString(i_crm, @"KeyNote");
                    var sApplicant = _fetchString(i_crm, @"Applicant");
                    var sImportant = _fetchString(i_crm, @"Important");
                    var sStatus = _fetchString(i_crm, @"Status");
                    var sRoles = _fetchString(i_crm, @"Roles");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    pml.DataList = db.Queryable<OTB_EIP_BillChangeApply, OTB_SYS_Members, OTB_SYS_Departments, OTB_SYS_Members>
                        ((t1, t2, t3, t4) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.Applicant == t2.MemberID,
                                JoinType.Inner, t2.OrgID == t3.OrgID && t2.DepartmentID == t3.DepartmentID,
                                JoinType.Inner, t1.OrgID == t4.OrgID && t1.Handle_Person == t4.MemberID
                              }
                        )
                        .Where((t1, t2, t3, t4) => t1.OrgID == i_crm.ORIGID && t1.KeyNote.Contains(sKeyNote) && sImportant.Contains(t1.Important))
                        .WhereIF(!string.IsNullOrEmpty(sApplicant), (t1, t2, t3, t4) => t1.Applicant == sApplicant)
                        .WhereIF(!string.IsNullOrEmpty(sStatus), (t1, t2, t3, t4) => sStatus.Contains(t1.Status))
                        .WhereIF(!(sRoles.Contains("Admin") || sRoles.Contains("EipManager") || sRoles.Contains("EipView")), (t1, t2, t3, t4) => (t1.Applicant == i_crm.USERID || t1.Handle_Person == i_crm.USERID || t1.CheckFlows.Contains(i_crm.USERID)))
                        .Select((t1, t2, t3, t4) => new View_EIP_BillChangeApply
                        {
                            Guid = SqlFunc.GetSelfAndAutoFill(t1.Guid),
                            ApplicantName = t2.MemberName,
                            DeptName = t3.DepartmentName,
                            Handle_PersonName = t4.MemberName
                        })
                        .MergeTable()
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    if (bExcel)
                    {
                        const string sFileName = "請款單（廠商）費用明細";
                        var oHeader = new Dictionary<string, string>
                                        {
                                            { "RowIndex", "項次" },
                                            { "WenZhongAcount", "員工代號" },
                                            { "AskTheDummyName", "員工姓名" },
                                            { "FillBrushDate", "補刷卡日期" },
                                            { "FillBrushType", "補刷卡時間" },
                                            { "FillBrushReason", "補刷卡說明 " },
                                            { "Memo", "備註" }
                                        };

                        var saAttendanceDiff = pml.DataList as List<View_EIP_AttendanceDiff>;
                        foreach (var item in saAttendanceDiff)
                        {
                            item.FillBrushDate = Convert.ToDateTime(Convert.ToDateTime(item.FillBrushDate).ToString("yyyy/MM/dd"));
                            item.FillBrushType = item.FillBrushType == "A" ? "上午" : item.FillBrushType == "P" ? "下午" : "全天";
                        }
                        var dicAlain = ExcelService.GetExportAlain(oHeader, new string[] { "Currency", "PayeeCode", "BillNO" }, "Amount");

                        var bOk = new ExcelService().CreateExcelByList(saAttendanceDiff, out string sPath, oHeader, dicAlain, sFileName);
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.EIP.BillChangeApply_QryService", "", "QueryPage（帳單更改申請單分頁查詢）", "", "", "");
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

        #endregion 帳單更改申請單分頁查詢

        #region 帳單更改申請單筆查詢

        /// <summary>
        /// 帳單更改申請單筆查詢
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
                    var sId = _fetchString(i_crm, @"Guid");

                    var oEntity = db.Queryable<OTB_EIP_BillChangeApply, OTB_SYS_Members, OTB_SYS_Departments, OTB_SYS_Members>
                        ((t1, t2, t3, t4) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.Applicant == t2.MemberID,
                                JoinType.Inner, t2.OrgID == t3.OrgID && t2.DepartmentID == t3.DepartmentID,
                                JoinType.Inner, t1.OrgID == t4.OrgID && t1.Handle_Person == t4.MemberID
                              }
                        )
                        .Where((t1, t2, t3, t4) => t1.OrgID == i_crm.ORIGID && t1.Guid == sId)
                        .Select((t1, t2, t3, t4) => new View_EIP_BillChangeApply
                        {
                            Guid = SqlFunc.GetSelfAndAutoFill(t1.Guid),
                            ApplicantName = t2.MemberName,
                            DeptName = t3.DepartmentName,
                            Handle_PersonName = t4.MemberName
                        }).Single();
                    if (!string.IsNullOrEmpty(oEntity.RelationId))
                    {
                        var oRelation = db.Queryable<OTB_EIP_BillChangeApply>().Single(x => x.OrgID == i_crm.ORIGID && x.Guid == oEntity.RelationId);
                        if (oRelation != null)
                        {
                            oEntity.ExFeild1 = oRelation.KeyNote;
                        }
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.EIP.BillChangeApply_QryService", "", "QueryOne（帳單更改申請單筆查詢）", "", "", "");
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

        #endregion 帳單更改申請單筆查詢
    }
}