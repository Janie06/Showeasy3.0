using EasyBL.WebApi.Message;
using EasyNet;
using Entity.Sugar;
using Entity.ViewModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyBL.WEBAPP.WSM
{
    public class MembersMaintain_UpdService : ServiceBase
    {
        #region 帳號管理（單筆查詢）

        /// <summary>
        /// 帳號管理（單筆查詢）
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
                    var sMemberID = _fetchString(i_crm, @"MemberID");
                    var oEntity = db.Queryable<OTB_SYS_Members, OTB_SYS_Members, OTB_SYS_Members>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ModifyUser == t3.MemberID
                              }
                        )
                        .Where((t1, t2, t3) => t1.OrgID == i_crm.ORIGID && t1.MemberID == sMemberID)
                        .Select((t1, t2, t3) => new View_SYS_Members
                        {
                            MemberID = SqlFunc.GetSelfAndAutoFill(t1.MemberID),
                            CreateUserName = t2.MemberName,
                            ModifyUserName = t3.MemberName,
                            RuleIDs = SqlFunc.MappingColumn(t1.MemberID, "(select RuleID+',' from OTB_SYS_MembersToRule mr where mr.OrgID=t1.OrgID and mr.MemberID=t1.MemberID for xml path(''))")
                        })
                        .Single();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(MembersMaintain_UpdService), "", "QueryOne（帳號管理（單筆查詢））", "", "", "");
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

        #endregion 帳號管理（單筆查詢）

        #region 帳號管理（新增）

        /// <summary>
        /// 帳號管理（新增）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage Insert(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var oEntity = _fetchEntity<OTB_SYS_Members>(i_crm);
                        _setEntityBase(oEntity, i_crm);
                        //Yang 2019/01/25 判斷Password是否為空值，是直接給預序密碼加密；否取得密碼加密
                        if (!string.IsNullOrEmpty(oEntity.Password))
                        {
                            oEntity.Password = SecurityUtil.Encrypt(oEntity.Password);  //Yang 2019/01/25 取得密碼後加密
                        }
                        else
                        {
                            oEntity.Password = SecurityUtil.Encrypt("123456");  //Yang 2019/01/25 新增帳號預設密碼加密，建議預設值放到web.config
                        }
    
                        oEntity.SysShowMode = "M";
                        var iRel = db.Insertable(oEntity).ExecuteCommand();
                        var saRuleInsert = new List<OTB_SYS_MembersToRule>();
                        //更新角色
                        if (i_crm.DATA["roles"] is JArray saRoles && saRoles.Count > 0)
                        {
                            foreach (var roleid in saRoles)
                            {
                                var oMembersToRule = new OTB_SYS_MembersToRule
                                {
                                    RuleID = roleid.ToString(),
                                    MemberID = oEntity.MemberID
                                };
                                _setEntityBase(oMembersToRule, i_crm);
                                saRuleInsert.Add(oMembersToRule);
                            }
                            iRel += db.Insertable(saRuleInsert).ExecuteCommand();
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(MembersMaintain_UpdService), @"帳號管理", @"Add（帳號管理（新增））", @"", @"", @"");
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

        #endregion 帳號管理（新增）

        #region 帳號管理（修改）

        /// <summary>
        /// 帳號管理（修改）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage Update(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var oNewEntity = _fetchEntity<OTB_SYS_Members>(i_crm);
                        var NewDpt = _fetchString(i_crm, @"DepartmentID");
                        var JobTitle = _fetchString(i_crm, @"JobTitle");
                        var MemberID = _fetchString(i_crm, @"MemberID");
                        _setEntityBase(oNewEntity, i_crm);
                        UpdateCheckFlows(db, MemberID, i_crm.ORIGID, NewDpt, JobTitle, i_crm.USERID);
                        var iRel = db.Deleteable<OTB_SYS_MembersToRule>().Where(x => x.OrgID == i_crm.ORIGID && x.MemberID == oNewEntity.MemberID).ExecuteCommand();
                        var saRuleInsert = new List<OTB_SYS_MembersToRule>();
                        if (i_crm.DATA["roles"] is JArray saRoles && saRoles.Count > 0)
                        {
                            //更新角色
                            foreach (var roleid in saRoles)
                            {
                                var oMembersToRule = new OTB_SYS_MembersToRule
                                {
                                    RuleID = roleid.ToString(),
                                    MemberID = oNewEntity.MemberID
                                };
                                _setEntityBase(oMembersToRule, i_crm);
                                saRuleInsert.Add(oMembersToRule);
                            }
                            iRel += db.Insertable(saRuleInsert).ExecuteCommand();
                        }
                        iRel += db.Updateable(oNewEntity)
                           .IgnoreColumns(x => new
                           {
                               x.Password,
                               x.SysShowMode,
                               x.CreateUser,
                               x.CreateDate
                           }).ExecuteCommand();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(MembersMaintain_UpdService), @"帳號管理", @"Update（帳號管理（修改））", @"", @"", @"");
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

        #endregion 帳號管理（修改）

        #region 帳號管理，更新CheckFlows資料

        public void UpdateCheckFlows(SqlSugarClient db, string MemberID, string OrgID, string NewDepID, string JobTitle, string UserID)
        {
            var User = db.Queryable<OTB_SYS_Members>().Where(m => m.MemberID == MemberID && m.OrgID == OrgID).First();
            var Dept = db.Queryable<OTB_SYS_Departments>().Where(d => d.DepartmentID == NewDepID && d.OrgID == OrgID).First();
            var Job = db.Queryable<OTB_SYS_Jobtitle>().Where(d => d.JobtitleID == JobTitle && d.OrgID == OrgID).First();
            if (User != null)
            {
                var UpdateCheckFlow = new List<OTB_EIP_CheckFlow>();
                var MatchedCheckFlows = db.Queryable<OTB_EIP_CheckFlow>().Where(cf => cf.Flows.Contains(MemberID)).ToList();
                if (MatchedCheckFlows.Any())
                {
                    foreach (var cf in MatchedCheckFlows)
                    {
                        var Flows = JsonConvert.DeserializeObject<List<CheckFlow>>(cf.Flows).OrderBy(c => c.Order).ToList();
                        foreach (var f in Flows)
                        {
                            foreach (var M in f.SignedMember)
                            {
                                if (M.id == MemberID)
                                {
                                    M.deptname = Dept.DepartmentShortName;
                                    M.jobname = Job.JobtitleName;
                                }
                            }

                        }
                        UpdateCheckFlow.Add(new OTB_EIP_CheckFlow() { OrgID = cf.OrgID, Guid = cf.Guid, Flows = JsonConvert.SerializeObject(Flows), ModifyDate = DateTime.Now, ModifyUser = UserID });
                    }
                    if (UpdateCheckFlow.Count > 0)
                    {
                        db.Updateable(UpdateCheckFlow)
                            .UpdateColumns(it => new { it.Flows, it.ModifyDate, it.ModifyUser }).ExecuteCommand();
                    }
                }
            }
            //比對部門。
            //取得所有相關Flows
            //更新裡面資料//JArray
        }

        #endregion

        #region 帳號管理（刪除）

        /// <summary>
        /// 帳號管理（刪除）
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on UpdImportCustomers</param>
        /// <returns></returns>
        public ResponseMessage Delete(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sMemberID = _fetchString(i_crm, @"MemberID");
                        var oMembers = db.Queryable<OTB_SYS_Members>().Single(x => x.MemberID == sMemberID);
                        var iRel = db.Deleteable<OTB_SYS_Members>().Where(x => x.OrgID == i_crm.ORIGID && x.MemberID == sMemberID).ExecuteCommand();
                        iRel += db.Deleteable<OTB_SYS_MembersToRule>().Where(x => x.OrgID == i_crm.ORIGID && x.MemberID == sMemberID).ExecuteCommand();
                        i_crm.DATA.Add("FileID", oMembers.MemberPic);
                        i_crm.DATA.Add("IDType", "parent");
                        new CommonService().DelFile(i_crm);
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(MembersMaintain_UpdService), @"帳號管理", @"Delete（帳號管理（刪除））", @"", @"", @"");
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

        #endregion 帳號管理（刪除）

        #region 帳號管理（職稱下拉查詢）

        /// <summary>
        /// 帳號管理（職稱下拉查詢）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetJobTitleDrop(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sEffective = _fetchString(i_crm, @"Effective");
                    var saJobtitle = db.Queryable<OTB_SYS_Jobtitle>()
                                    .Where(x => x.OrgID == i_crm.ORIGID)
                                    .WhereIF(!string.IsNullOrEmpty(sEffective), x => x.Effective == sEffective)
                                    .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saJobtitle);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(MembersMaintain_UpdService), "", "GetJobTitleDrop（帳號管理（職稱下拉查詢））", "", "", "");
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

        #endregion 帳號管理（職稱下拉查詢）

        #region 帳號管理（角色下拉查詢）

        /// <summary>
        /// 帳號管理（角色下拉查詢）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetRolesDrop(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sEffective = _fetchString(i_crm, @"Effective");
                    var oEntity = db.Queryable<OTB_SYS_Rules>()
                                    .Where(x => x.OrgID == i_crm.ORIGID && x.DelStatus == "N")
                                    .ToList();
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(MembersMaintain_UpdService), "", "GetRolesDrop（帳號管理（角色下拉查詢））", "", "", "");
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

        #endregion 帳號管理（角色下拉查詢）

        #region 帳號管理（查詢筆數）

        /// <summary>
        /// 帳號管理（查詢筆數）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage QueryCout(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sMemberID = _fetchString(i_crm, @"MemberID");
                    var s_MemberID = _fetchString(i_crm, @"_MemberID");
                    var sEmail = _fetchString(i_crm, @"Email");
                    var sOutlookAccount = _fetchString(i_crm, @"OutlookAccount");
                    var iCout = db.Queryable<OTB_SYS_Members>()
                        .Where(x => x.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sMemberID), x => x.MemberID == sMemberID)
                        .WhereIF(!string.IsNullOrEmpty(s_MemberID), x => x.MemberID != s_MemberID)
                        .WhereIF(!string.IsNullOrEmpty(sEmail), x => x.Email == sEmail)
                        .WhereIF(!string.IsNullOrEmpty(sOutlookAccount), x => x.OutlookAccount == sOutlookAccount)
                        .Count();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iCout);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(ArgumentClassMaintain_UpdService), "", "QueryCout（帳號管理（查詢筆數））", "", "", "");
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

        #endregion 帳號管理（查詢筆數）
    }
}