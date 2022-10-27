using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.EIP
{
    public class TravelExpenseReport_QryService : ServiceBase
    {
        #region 國外差旅費報告書分頁查詢

        /// <summary>
        /// 國外差旅費報告書分頁查詢
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

                    pml.DataList = db.Queryable<OTB_EIP_TravelExpense, OTB_SYS_Members, OTB_SYS_Departments, OTB_SYS_Members>
                        ((t1, t2, t3, t4) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.Applicant == t2.MemberID,
                                JoinType.Inner, t2.OrgID == t3.OrgID && t2.DepartmentID == t3.DepartmentID,
                                JoinType.Inner, t1.OrgID == t4.OrgID && t1.Handle_Person == t4.MemberID
                              }
                        )
                        .Where((t1, t2, t3, t4) => t1.OrgID == i_crm.ORIGID && t1.KeyNote.Contains(sKeyNote) && sImportant.Contains(t1.Important))
                        //.Where((t1, t2, t3, t4) => t1.OrgID == i_crm.ORIGID )
                        .WhereIF(!string.IsNullOrEmpty(sApplicant), (t1, t2, t3, t4) => t1.Applicant == sApplicant)
                        .WhereIF(!string.IsNullOrEmpty(sStatus), (t1, t2, t3, t4) => sStatus.Contains(t1.Status))
                        .WhereIF(!(sRoles.Contains("Admin") || sRoles.Contains("EipManager") || sRoles.Contains("EipView")), (t1, t2, t3, t4) => (t1.Applicant == i_crm.USERID || t1.Handle_Person == i_crm.USERID || t1.CheckFlows.Contains(i_crm.USERID)))
                        .Select((t1, t2, t3, t4) => new View_EIP_TravelExpense
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.EIP.TravelExpenseReport_QryService", "", "QueryPage（國外差旅費報告書分頁查詢）", "", "", "");
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

        #endregion 國外差旅費報告書分頁查詢

        #region 國外差旅費報告書單筆查詢

        /// <summary>
        /// 國外差旅費報告書單筆查詢
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

                    var oEntity = db.Queryable<OTB_EIP_TravelExpense, OTB_SYS_Members, OTB_SYS_Departments, OTB_SYS_Members>
                        ((t1, t2, t3, t4) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.Applicant == t2.MemberID,
                                JoinType.Inner, t2.OrgID == t3.OrgID && t2.DepartmentID == t3.DepartmentID,
                                JoinType.Inner, t1.OrgID == t4.OrgID && t1.Handle_Person == t4.MemberID
                              }
                        )
                        .Where((t1, t2, t3, t4) => t1.OrgID == i_crm.ORIGID && t1.Guid == sId)
                        .Select((t1, t2, t3, t4) => new View_EIP_TravelExpense
                        {
                            Guid = SqlFunc.GetSelfAndAutoFill(t1.Guid),
                            ApplicantName = t2.MemberName,
                            DeptName = t3.DepartmentName,
                            Handle_PersonName = t4.MemberName
                        }).Single();
                    if (!string.IsNullOrEmpty(oEntity.RelationId))
                    {
                        var oRelation = db.Queryable<OTB_EIP_TravelExpense>().Single(x => x.OrgID == i_crm.ORIGID && x.Guid == oEntity.RelationId);
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, "EasyBL.WEBAPP.EIP.TravelExpenseReport_QryService", "", "QueryOne（國外差旅費報告書單筆查詢）", "", "", "");
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

        #endregion 國外差旅費報告書單筆查詢
    }
}