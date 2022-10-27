using EasyBL.WebApi.Message;
using Entity.Sugar;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;

namespace EasyBL.WEBAPP.WSM
{
    public class RoleMaintain_UpdService : ServiceBase
    {
        #region 角色管理（單筆查詢）

        /// <summary>
        /// 角色管理（單筆查詢）
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
                    var sRuleID = _fetchString(i_crm, @"RuleID");

                    var oEntity = db.Queryable<OTB_SYS_Rules, OTB_SYS_Members, OTB_SYS_Members>
                        ((t1, t2, t3) =>
                        new object[] {
                                JoinType.Left, t1.OrgID == t2.OrgID && t1.CreateUser == t2.MemberID,
                                JoinType.Left, t1.OrgID == t3.OrgID && t1.ModifyUser == t3.MemberID
                              }
                        )
                        .Where((t1, t2, t3) => t1.OrgID == i_crm.ORIGID && t1.RuleID == sRuleID)
                        .Select((t1, t2, t3) => new OTB_SYS_Rules
                        {
                            RuleID = SqlFunc.GetSelfAndAutoFill(t1.RuleID),
                            CreateUserName = t2.MemberName,
                            ModifyUserName = t3.MemberName,
                            ExFeild1 = SqlFunc.MappingColumn(t1.RuleID, "(SELECT MemberID+',' FROM OTB_SYS_MembersToRule b WHERE b.OrgID=t1.OrgID and b.RuleID=t1.RuleID FOR XML PATH(''))")
                        })
                        .Single();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oEntity);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(RoleMaintain_UpdService), "", "QueryOne（角色管理（單筆查詢））", "", "", "");
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

        #endregion 角色管理（單筆查詢）

        #region 角色管理（新增）

        /// <summary>
        /// 角色管理（新增）
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
                        var oEntity = _fetchEntity<OTB_SYS_Rules>(i_crm);
                        _setEntityBase(oEntity, i_crm);
                        oEntity.DelStatus = "N";

                        var oEntity_Del = db.Queryable<OTB_SYS_Rules>()
                                            .Single(x => x.OrgID == i_crm.ORIGID && x.RuleID == oEntity.RuleID && x.DelStatus == "Y");
                        var iRel = oEntity_Del != null ? db.Updateable(oEntity)
                                     .IgnoreColumns(x => new
                                     {
                                         x.CreateUser,
                                         x.CreateDate
                                     }).ExecuteCommand() : db.Insertable(oEntity).ExecuteCommand();

                        var saRuleInsert = new List<OTB_SYS_MembersToRule>();
                        //更新角色
                        if (i_crm.DATA["users"] is JArray saUsers && saUsers.Count > 0)
                        {
                            foreach (var userid in saUsers)
                            {
                                var oMembersToRule = new OTB_SYS_MembersToRule
                                {
                                    RuleID = oEntity.RuleID,
                                    MemberID = userid.ToString()
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(RoleMaintain_UpdService), @"角色管理", @"Add（角色管理（新增））", @"", @"", @"");
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

        #endregion 角色管理（新增）

        #region 角色管理（修改）

        /// <summary>
        /// 角色管理（修改）
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
                        var oNewEntity = _fetchEntity<OTB_SYS_Rules>(i_crm);
                        _setEntityBase(oNewEntity, i_crm);
                        var iRel = db.Updateable(oNewEntity)
                            .IgnoreColumns(x => new
                            {
                                x.DelStatus,
                                x.CreateUser,
                                x.CreateDate
                            }).ExecuteCommand();
                        iRel += db.Deleteable<OTB_SYS_MembersToRule>().Where(x => x.OrgID == i_crm.ORIGID && x.RuleID == oNewEntity.RuleID).ExecuteCommand();

                        var saRuleInsert = new List<OTB_SYS_MembersToRule>();
                        //更新角色
                        if (i_crm.DATA["users"] is JArray saUsers && saUsers.Count > 0)
                        {
                            foreach (var userid in saUsers)
                            {
                                var oMembersToRule = new OTB_SYS_MembersToRule
                                {
                                    RuleID = oNewEntity.RuleID,
                                    MemberID = userid.ToString()
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(RoleMaintain_UpdService), @"角色管理", @"Update（角色管理（修改））", @"", @"", @"");
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

        #endregion 角色管理（修改）

        #region 角色管理（刪除）

        /// <summary>
        /// 角色管理（刪除）
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
                        var sRuleID = _fetchString(i_crm, @"RuleID");
                        var iRel = db.Updateable<OTB_SYS_Rules>()
                                        .UpdateColumns(x => new OTB_SYS_Rules { DelStatus = "Y" })
                                        .Where(x => x.OrgID == i_crm.ORIGID && x.RuleID == sRuleID).ExecuteCommand();
                        iRel += db.Deleteable<OTB_SYS_MembersToRule>().Where(x => x.OrgID == i_crm.ORIGID && x.RuleID == sRuleID).ExecuteCommand();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(RoleMaintain_UpdService), @"角色管理", @"Delete（角色管理（刪除））", @"", @"", @"");
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

        #endregion 角色管理（刪除）
    }
}