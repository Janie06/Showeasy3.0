using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.SYS
{
    public class MembersMaintain_QryService : ServiceBase
    {
        #region 帳號管理（分頁查詢）

        /// <summary>
        /// 帳號管理（分頁查詢）
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

                    var sMemberID = _fetchString(i_crm, @"MemberID");
                    var sMemberName = _fetchString(i_crm, @"MemberName");
                    var sEffective = _fetchString(i_crm, @"Effective");
                    var sDepartmentID = _fetchString(i_crm, @"DepartmentID");
                    var sJobTitle = _fetchString(i_crm, @"JobTitle");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    pml.DataList = db.Queryable<OTB_SYS_Members, OTB_SYS_Departments, OTB_SYS_Jobtitle, OTB_SYS_Members>
                        ((t1, t2, t3, t4) =>
                        new object[] {
                                JoinType.Inner, t1.OrgID == t2.OrgID && t1.DepartmentID == t2.DepartmentID,
                                JoinType.Inner, t1.OrgID == t3.OrgID && t1.JobTitle == t3.JobtitleID,
                                JoinType.Left, t1.OrgID == t4.OrgID && t1.ImmediateSupervisor == t4.MemberID
                              }
                        )
                        .Where((t1, t2, t3, t4) => t1.OrgID == i_crm.ORIGID && t1.MemberID.Contains(sMemberID) && t1.MemberName.Contains(sMemberName) && sEffective.Contains(t1.Effective))
                        .WhereIF(!string.IsNullOrEmpty(sDepartmentID), (t1, t2, t3, t4) => t1.DepartmentID == sDepartmentID)
                        .WhereIF(!string.IsNullOrEmpty(sJobTitle), (t1, t2, t3, t4) => t1.JobTitle == sJobTitle)
                        .Select((t1, t2, t3, t4) => new View_SYS_Members
                        {
                            MemberID = SqlFunc.GetSelfAndAutoFill(t1.MemberID),
                            DepartmentName = t2.DepartmentName,
                            JobtitleName = t3.JobtitleName,
                            ImmediateSupervisorName = t4.MemberName
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
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(MembersMaintain_QryService), "", "QueryPage（帳號管理（分頁查詢））", "", "", "");
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

        #endregion 帳號管理（分頁查詢）
    }
}