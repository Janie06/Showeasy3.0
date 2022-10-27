using EasyBL.WebApi.Message;
using Entity.Sugar;
using Entity.ViewModels;
using SqlSugar;
using SqlSugar.Base;
using System;

namespace EasyBL.WEBAPP.SYS
{
    public class RoleMaintain_QryService : ServiceBase
    {
        #region 角色管理（分頁資料）

        /// <summary>
        /// 角色管理（分頁資料）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on QueryPage</param>
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
                    var sRuleID = _fetchString(i_crm, @"RuleID");
                    var sRuleName = _fetchString(i_crm, @"RuleName");
                    var sMemberID = _fetchString(i_crm, @"MemberID");
                    var bExcel = _fetchBool(i_crm, @"Excel");

                    pml.DataList = db.Queryable<OTB_SYS_Rules>("x")
                        .Select(x => new OTB_SYS_Rules
                        {
                            OrgID = x.OrgID,
                            RuleID = x.RuleID,
                            RuleName = x.RuleName,
                            DelStatus = x.DelStatus,
                            ExFeild1 = SqlFunc.MappingColumn(x.RuleID, "dbo.[OFN_SYS_GetUserNameByRuleID](x.OrgID,x.RuleID)")
                        })
                        .MergeTable()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.DelStatus == "N" && x.RuleID.Contains(sRuleID) && x.RuleName.Contains(sRuleName) && x.ExFeild1.Contains(sMemberID))
                        .OrderBy(sSortField, sSortOrder)
                        .ToPageList(pml.PageIndex, bExcel ? 100000 : pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, pml);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Task_QryService), @"角色管理", @"GetTasksPage（角色管理（分頁資料））", @"", @"", @"");
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

        #endregion 角色管理（分頁資料）
    }
}