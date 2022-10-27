using EasyBL.WebApi.Message;
using Entity.Sugar;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Data;

namespace EasyBL.WEBAPP.SYS
{
    public class AuthantedProgramsService : ServiceBase
    {
        #region 查詢系統所有程式（系統授權）

        /// <summary>
        /// 查詢系統所有程式（系統授權）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetAuthorizeBy_(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var sType = _fetchString(i_crm, @"Type");
                    var sRuleID = _fetchString(i_crm, @"RuleID");
                    var sChildSystem = _fetchString(i_crm, @"ChildSystem");
                    var sModuleID = _fetchString(i_crm, @"ModuleID");
                    var dic_SP = new Dictionary<string, string> {
                        { "Role", "OSP_Common_GetProgramListByRuleId" },
                        { "Dept", "OSP_Common_GetProgramListByDeptId" },
                        { "Member", "OSP_Common_GetProgramListByMemberId" }
                    };
                    var spOrgID = new SugarParameter("@OrgID", i_crm.ORIGID);
                    var spRuleID = new SugarParameter("@RuleID", sRuleID);
                    var spChildSystem = new SugarParameter("@ChildSystem", sChildSystem);
                    var spModuleID = new SugarParameter("@ModuleID", sModuleID);
                    var dt = db.Ado.UseStoredProcedure().GetDataTable(dic_SP[sType], spOrgID, spRuleID, spChildSystem, spModuleID);
                    using (var tb_new = new DataTable())
                    {
                        foreach (DataColumn col in dt.Columns)
                        {
                            tb_new.Columns.Add(col.ToString());
                        }
                        var saAuth = db.Queryable<OTB_SYS_Arguments>()
                                        .Where(x => x.OrgID == i_crm.ORIGID && x.ArgumentClassID == "99999" && x.Effective == "Y")
                                        .ToList();

                        foreach (var _auth in saAuth)
                        {
                            tb_new.Columns.Add(_auth.ArgumentID);
                        }
                        foreach (DataRow row in dt.Rows)
                        {
                            var row_new = tb_new.NewRow();
                            foreach (DataColumn col in dt.Columns)
                            {
                                row_new[col.ToString()] = row[col];
                            }
                            var saCanAllowRight = row["CanAllowRight"].ToString().Trim().Split(new string[] { @"|" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string right in saCanAllowRight)
                            {
                                row_new[right] = right;
                            }
                            tb_new.Rows.Add(row_new);
                        }

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, tb_new);
                    }
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(AuthantedProgramsService), "", @"GetAuthorizeBy_(查詢系統所有程式（系統授權）)", @"", @"", @"");
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

        #endregion 查詢系統所有程式（系統授權）

        #region 修改（系統授權）

        /// <summary>
        /// 修改（系統授權）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage UpdateAuthorize(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sType = _fetchString(i_crm, @"Type");
                        var sRuleID = _fetchString(i_crm, @"RuleID");
                        var sChildSystem = _fetchString(i_crm, @"ChildSystem");
                        var sModuleID = _fetchString(i_crm, @"ModuleID");
                        var dic_SP_Del = new Dictionary<string, string> {
                        { "Role", "[dbo].[OSP_OTB_SYS_Authorize_Delete]" },
                        { "Dept", "OSP_OTB_SYS_AuthorizeForDept_Delete" },
                        { "Member", "OSP_OTB_SYS_AuthorizeForMember_Delete" }
                    };
                        var dic_SP_Add = new Dictionary<string, string> {
                        { "Role", "OSP_OTB_SYS_Authorize_ADD" },
                        { "Dept", "OSP_OTB_SYS_AuthorizeForDept_Add" },
                        { "Member", "OSP_OTB_SYS_AuthorizeForMember_Add" }
                    };
                        var spOrgID = new SugarParameter("@OrgID", i_crm.ORIGID);
                        var spRuleID = new SugarParameter("@RuleID", sRuleID);
                        var spChildSystem = new SugarParameter("@ChildSystem", sChildSystem);
                        var spModuleID = new SugarParameter("@ModuleID", sModuleID);
                        var iRel = db.Ado.UseStoredProcedure().ExecuteCommand(dic_SP_Del[sType], spOrgID, spRuleID, spChildSystem, spModuleID);

                        var saAuth = i_crm.DATA["add"] as JArray;

                        foreach (JObject jo in saAuth)
                        {
                            var saSp = new List<SugarParameter>();
                            foreach (var auth in jo)
                            {
                                saSp.Add(new SugarParameter(auth.Key, auth.Value));
                            }
                            var iRel_Add = db.Ado.UseStoredProcedure().ExecuteCommand(dic_SP_Add[sType], saSp);
                        }

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(AuthantedProgramsService), "", @"UpdateAuthorize(修改（系統授權）)", @"", @"", @"");
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

        #endregion 修改（系統授權）

        #region 獲取系統角色（系統授權）

        /// <summary>
        /// 獲取系統角色（系統授權）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetRules(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var saRules = db.Queryable<OTB_SYS_Rules>().Where(x => x.OrgID == i_crm.ORIGID && x.DelStatus == "N").ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saRules);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(AuthantedProgramsService), "", @"GetRules(獲取系統角色（系統授權）)", @"", @"", @"");
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

        #endregion 獲取系統角色（系統授權）

        #region 獲取系統模組（系統授權）

        /// <summary>
        /// 獲取系統模組（系統授權）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetModulelist(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var bParentID = _fetchBool(i_crm, @"ParentID");
                    var bIncludeParent = _fetchBool(i_crm, @"IncludeParent");
                    var saRules = db.Queryable<OTB_SYS_ModuleList>()
                                    .Where(x => x.OrgID == i_crm.ORIGID)
                                    .WhereIF(!bParentID && !bIncludeParent, x => SqlFunc.HasValue(x.ParentID))
                                    .WhereIF(bParentID && !bIncludeParent, x => !SqlFunc.HasValue(x.ParentID))
                                    .OrderBy(x => x.OrderByValue)
                                    .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saRules);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(AuthantedProgramsService), "", @"GetModulelist(獲取系統模組（系統授權）)", @"", @"", @"");
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

        #endregion 獲取系統模組（系統授權）
    }
}