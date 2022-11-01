using EasyBL.WebApi.Message;
using EasyNet;
using Entity.Sugar;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EasyBL.WEBAPP.EIP
{
    public class InvoiceApplyForPersonal_UpdService : ServiceBase
    {
        #region 請款單（個人）提交簽核

        /// <summary>
        /// 請款單（個人）提交簽核
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on InvoiceApplyForPersonalToAudit</param>
        /// <returns></returns>
        public ResponseMessage InvoiceApplyForPersonalToAudit(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sId = _fetchString(i_crm, EasyNetGlobalConstWord.GUID);
                        var sdb = new SimpleClient<OTB_EIP_InvoiceApplyInfo>(db);
                        var oEip = sdb.GetById(sId);

                        if (oEip == null)
                        {
                            sMsg = @"系統找不到對應的請款單（個人）資料，請核查！";
                            break;
                        }

                        var oApplicant = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == i_crm.ORIGID && it.MemberID == oEip.Applicant);
                        var sTitle = oApplicant.MemberName + @"的請款單（個人）申請「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }
                        //更新基本資料
                        var oEipUpd = new OTB_EIP_InvoiceApplyInfo
                        {
                            Status = @"B",
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oEipUpd)
                            .UpdateColumns(it => new { it.Status, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.Guid == sId).ExecuteCommand();
                        //更新代辦
                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sId);

                        var jaCheckFlows = (JArray)JsonConvert.DeserializeObject(oEip.CheckFlows);
                        var MinOrderToNotice = jaCheckFlows.Min(x => ((JObject)x)["Order"].ToString()).ToString();
                        var saSignedId = jaCheckFlows.Where(x => ((JObject)x)["Order"].ToString() == MinOrderToNotice).Select(x => ((JObject)x)["SignedId"].ToString()).ToList();

                        if (saSignedId.Count > 0)
                        {
                            foreach (string signedId in saSignedId)
                            {
                                //添加提醒消息
                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, signedId, @"InvoiceApplyForPersonal_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
                                db.Insertable(oTipsAdd).ExecuteCommand();
                                //添加代辦
                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, oEip.Guid, signedId, sTitle, @"InvoiceApplyForPersonal_View", @"?Action=Upd&Guid=" + oEip.Guid, @"B");
                                db.Insertable(oTaskAdd).ExecuteCommand();
                            }
                        }
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, saSignedId);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(InvoiceApplyForPersonal_UpdService), @"請款單（個人）", @"InvoiceApplyForPersonalToAudit（請款單（個人）提交簽核）", @"", @"", @"");
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

        #endregion 請款單（個人）提交簽核

        #region 請款單（個人）簽核|签辦

        /// <summary>
        /// 請款單（個人）簽核|签辦
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on InvoiceApplyForPersonalAudit</param>
        /// <returns></returns>
        public ResponseMessage InvoiceApplyForPersonalAudit(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sId = _fetchString(i_crm, @"Guid");
                        var sAction = _fetchString(i_crm, @"Action");
                        var sGoNext = _fetchString(i_crm, @"GoNext");
                        var sHandlePerson = _fetchString(i_crm, @"HandlePerson");
                        var sNextSignedWays = _fetchString(i_crm, @"NextSignedWays");
                        var sNextUsers = _fetchString(i_crm, @"NextUsers");
                        var sTipsUsers = _fetchString(i_crm, @"TipsUsers");
                        var sCheckFlows = _fetchString(i_crm, @"CheckFlows");
                        var sHandleFlows = _fetchString(i_crm, @"HandleFlows");
                        var sSignedDecision = _fetchString(i_crm, @"SignedDecision");
                        var sHandleDecision = _fetchString(i_crm, @"HandleDecision");
                        var sdb = new SimpleClient<OTB_EIP_InvoiceApplyInfo>(db);
                        var oEip = sdb.GetById(sId);
                        var TipsType = WebAppGlobalConstWord.CHECK;
                        if (oEip == null)
                        {
                            sMsg = @"系統找不到對應的請款單（個人）資料，請核查！";
                            break;
                        }

                        var saNextSignedWays = (JArray)JsonConvert.DeserializeObject(sNextSignedWays);
                        var saNextUsers = (JArray)JsonConvert.DeserializeObject(sNextUsers);
                        var saTipsUsers = (JArray)JsonConvert.DeserializeObject(sTipsUsers);

                        if (oEip.Guid == null)
                        {
                            sMsg = @"系統找不到對應的請款單（個人）資料，請核查！";
                            break;
                        }
                        var oUser_Self = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == i_crm.ORIGID && it.MemberID == i_crm.USERID);
                        var oAskTheDummy = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == i_crm.ORIGID && it.MemberID == oEip.Applicant);
                        var sTitle_Self = @"";
                        var sTitle_Handle = oUser_Self.MemberName + @"審批了" + oAskTheDummy.MemberName + @"的請款單（個人）「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
                        var sTitle_Next = oAskTheDummy.MemberName + @"的請款單（個人）申請「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
                        var sTitle_Notice = @"";
                        var sStatus = @"B";

                        if (sAction == @"Signed")
                        {
                            sTitle_Self = oUser_Self.MemberName + @"審批了您的請款單（個人）「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
                            sTitle_Notice = oAskTheDummy.MemberName + @"的請款單（個人）申請「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber + @"，請點擊查看...";
                            if (sSignedDecision == @"Y")
                            {
                                sTitle_Self += @"，審批結果：同意";
                                if (!string.IsNullOrWhiteSpace(sHandlePerson))
                                {
                                    sStatus = @"E";
                                }
                            }
                            else if (sSignedDecision == @"N")
                            {
                                sTitle_Self += @"，審批結果：不同意";
                                sStatus = @"D-O";
                                TipsType = WebAppGlobalConstWord.FAIL;
                            }
                            else if (sSignedDecision == @"O")
                            {
                                sTitle_Self += @"，審批結果：先加簽";
                            }
                        }
                        else
                        {
                            sTitle_Self = oUser_Self.MemberName + @"签辦了您的請款單（個人）申請「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
                            if (sHandleDecision == @"Y")
                            {
                                sTitle_Self += @"簽辦結果：同意";
                                sStatus = @"H-O";
                            }
                            else if (sHandleDecision == @"O")
                            {
                                sTitle_Self += @"簽辦結果：先轉呈其他主管審批";
                            }
                        }
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle_Self = ChineseStringUtility.ToSimplified(sTitle_Self);
                            sTitle_Next = ChineseStringUtility.ToSimplified(sTitle_Next);
                            sTitle_Notice = ChineseStringUtility.ToSimplified(sTitle_Notice);
                        }
                        //更新基本資料bundler
                        var oEipUpd = new OTB_EIP_InvoiceApplyInfo
                        {
                            Status = sStatus,
                            CheckFlows = sCheckFlows,
                            HandleFlows = sHandleFlows,
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oEipUpd)
                            .UpdateColumns(it => new { it.Status, it.CheckFlows, it.HandleFlows, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.Guid == sId).ExecuteCommand();

                        var sOwner = @"";
                        if (sSignedDecision == @"Y" && sGoNext == @"N")
                        {
                            sOwner = i_crm.USERID;
                        }
                        //更新代辦
                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sId, sOwner);

                        var listTips = new List<OTB_SYS_Tips>();
                        var listTask = new List<OTB_SYS_Task>();
                        var listToTips = new List<string>();

                        if (sStatus != @"D-O")
                        {
                            foreach (string flow in saNextSignedWays)
                            {
                                if (sGoNext == @"Y")
                                {
                                    if (flow == @"flow4")//添加通知和提醒給下個流程所有要通知的人
                                    {
                                        foreach (string user in saTipsUsers)
                                        {
                                            //添加提醒消息
                                            var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Notice, user, @"InvoiceApplyForPersonal_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
                                            listTips.Add(oTipsAdd);
                                            listToTips.Add(user);
                                        }
                                    }
                                    else if (flow == @"flow5")//添加通知和提醒給經辦人
                                    {
                                        if (sHandlePerson != @"")
                                        {
                                            if (sHandleDecision != @"N")
                                            {
                                                //添加代辦
                                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, oEip.Guid, sHandlePerson, sTitle_Handle, @"InvoiceApplyForPersonal_View", @"?Action=Upd&Guid=" + oEip.Guid, @"E");
                                                listTask.Add(oTaskAdd);
                                                //添加提醒消息
                                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Handle, sHandlePerson, @"InvoiceApplyForPersonal_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
                                                listTips.Add(oTipsAdd);
                                                listToTips.Add(sHandlePerson);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        foreach (string user in saNextUsers)//添加通知和提醒給下一個審核的人
                                        {
                                            if (sSignedDecision == @"Y" || sSignedDecision == @"O")
                                            {
                                                //添加代辦
                                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, oEip.Guid, user, sTitle_Next, @"InvoiceApplyForPersonal_View", @"?Action=Upd&Guid=" + oEip.Guid, @"G");
                                                listTask.Add(oTaskAdd);
                                                //添加提醒消息
                                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Next, user, @"InvoiceApplyForPersonal_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
                                                listTips.Add(oTipsAdd);
                                                listToTips.Add(user);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //添加提醒消息(給請假的人)
                        var oTips_Applicant = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Self, oEip.Applicant, @"InvoiceApplyForPersonal_View" + @"|?Action=Upd&Guid=" + oEip.Guid, TipsType);
                        listTips.Add(oTips_Applicant);
                        listToTips.Add(oEip.Applicant);

                        if (listTips.Count > 0)
                        {
                            db.Insertable(listTips).ExecuteCommand();
                        }
                        if (listTask.Count > 0)
                        {
                            db.Insertable(listTask).ExecuteCommand();
                        }
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, listToTips);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(InvoiceApplyForPersonal_UpdService), @"請款單（個人）", @"InvoiceApplyForPersonalAudit（請款單（個人）簽核|签辦）", @"", @"", @"");
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

        #endregion 請款單（個人）簽核|签辦

        #region 請款單（個人）簽核前面所有人
        /// <summary>
        /// 請款單（個人）簽核前面所有人
        /// </summary>
        /// <returns></returns>
        public ResponseMessage InvoiceApplyForPersonalAuditAll(RequestMessage i_crm)
        {
            string SettingItemID = "InvoiceApplySuperAuditor";
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sId = _fetchString(i_crm, @"Guid");
                        var sAutoSignedDecision = _fetchString(i_crm, @"AutoSignedDecision");
                        var bAutoSignedDecision = sAutoSignedDecision == "Y" ? true : false;
                        var sAutoSignedOpinion = _fetchString(i_crm, @"AutoSignedOpinion");
                        var sdb = new SimpleClient<OTB_EIP_InvoiceApplyInfo>(db);
                        var oEip = sdb.GetById(sId);
                        var SuperAuditorList = Common.GetSystemSetting(db, i_crm.ORIGID, SettingItemID);
                        var TipsType = WebAppGlobalConstWord.CHECK;
                        if (string.IsNullOrWhiteSpace(SuperAuditorList) || !SuperAuditorList.Contains(i_crm.USERID))
                        {
                            sMsg = @"該使用者無法執行自動簽核，權限不足或者無設置參數。";
                            break;
                        }
                        if (oEip == null || oEip.Guid == null)
                        {
                            sMsg = @"系統找不到對應的請款單（廠商）資料，請核查！";
                            break;
                        }
                        var Auditor = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == i_crm.ORIGID && it.MemberID == i_crm.USERID);
                        var Applicant = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == i_crm.ORIGID && it.MemberID == oEip.Applicant);
                        //Eip、Task、Tip
                        var UpdateInvoiceApplyInfo = new OTB_EIP_InvoiceApplyInfo();
                        var InsertTasks = new List<OTB_SYS_Task>();
                        var InsertTips = new List<OTB_SYS_Tips>();
                        var NoticeUserBySignalR = new List<string>();
                        var NotificationInfo = SuperAuditor.GetNotification(oEip, Auditor, Applicant, bAutoSignedDecision);
                        //，請點擊查看...
                        switch (bAutoSignedDecision)
                        {
                            case true:
                                {
                                    TipsType = WebAppGlobalConstWord.CHECK;
                                    var ToNotifications = new List<string>();
                                    UpdateInvoiceApplyInfo = SuperAuditor.AgreeAll(oEip.CheckFlows, i_crm.USERID, sAutoSignedOpinion, out ToNotifications);
                                    var oTask = SYS.Task_QryService.TaskAdd(i_crm, oEip.Guid, oEip.Handle_Person, NotificationInfo.Item1, @"InvoiceApplyForPersonal_View", @"?Action=Upd&Guid=" + oEip.Guid, @"E");
                                    InsertTasks.Add(oTask);//經辦人員，新增代辦事項
                                    var oTipsToHandlePerson = SYS.Task_QryService.TipsAdd(i_crm, NotificationInfo.Item1, oEip.Handle_Person, @"InvoiceApplyForPersonal_View" + @"|?Action=Upd&Guid=" + oEip.Guid, TipsType);
                                    InsertTips.Add(oTipsToHandlePerson); //經辦人員，新增提醒
                                    NoticeUserBySignalR.Add(oEip.Handle_Person);
                                    foreach (var Name in ToNotifications)
                                    {
                                        var _Tip = SYS.Task_QryService.TipsAdd(i_crm, NotificationInfo.Item1 + @"，請點擊查看...", Name, @"InvoiceApplyForPersonal_View" + @"|?Action=Upd&Guid=" + oEip.Guid, TipsType);
                                        InsertTips.Add(_Tip);//簽核成員，通知
                                        NoticeUserBySignalR.Add(Name);
                                    }
                                    NoticeUserBySignalR.Add(oEip.Applicant);
                                }
                                break;
                            case false:
                                {
                                    TipsType = WebAppGlobalConstWord.FAIL;
                                    UpdateInvoiceApplyInfo = SuperAuditor.Disagree(oEip.CheckFlows, i_crm.USERID, sAutoSignedOpinion);
                                }
                                break;
                            default:
                                break;
                        }

                        var oTipsToApplicant = SYS.Task_QryService.TipsAdd(i_crm, NotificationInfo.Item1 + NotificationInfo.Item2, oEip.Applicant, @"InvoiceApplyForPersonal_View" + @"|?Action=Upd&Guid=" + oEip.Guid, TipsType);
                        InsertTips.Add(oTipsToApplicant);
                        NoticeUserBySignalR.Add(oEip.Applicant);

                        db.Updateable(UpdateInvoiceApplyInfo)
                            .UpdateColumns(it => new { it.CheckFlows, it.ModifyUser, it.ModifyDate, it.Status })
                            .Where(it => it.Guid == sId).ExecuteCommand();
                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sId, "");//清除目前關聯的task

                        if (InsertTips.Count > 0)
                        {
                            db.Insertable(InsertTips).ExecuteCommand();
                        }
                        if (InsertTasks.Count > 0)
                        {
                            db.Insertable(InsertTasks).ExecuteCommand();
                        }

                        NoticeUserBySignalR = NoticeUserBySignalR.Distinct().ToList();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, NoticeUserBySignalR);

                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(InvoiceApplyForCustomer_UpdService), @"請款單（廠商）", @"InvoiceApplyForCustomerAudit（請款單（廠商）簽核|签辦）", @"", @"", @"");
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
        #endregion
    }
}