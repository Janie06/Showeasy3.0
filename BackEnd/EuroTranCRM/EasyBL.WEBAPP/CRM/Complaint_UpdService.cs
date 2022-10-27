using EasyBL.WebApi.Message;
using EasyNet;
using Entity;
using Entity.Sugar;
using Entity.ViewModels;
using JumpKick.HttpLib;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Aspose.Cells;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Data;
using System.IO;
using EasyBL;

namespace EasyBL.WEBAPP.CRM
{
    public class Complaint_UpdService : ServiceBase
    {
        #region 客訴（個人）提交簽核

        /// <summary>
        /// 客訴 提交簽核
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ComplaintForPersonalToAudit</param>
        /// <returns></returns>
        public ResponseMessage ComplaintToAudit(RequestMessage i_crm)
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
                        var sdb = new SimpleClient<OTB_CRM_Complaint>(db);
                        var oCrm = sdb.GetById(sId);

                        if (oCrm == null)
                        {
                            sMsg = @"系統找不到對應的客訴資料，請核查！";
                            break;
                        }

                        var oApplicant = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == i_crm.ORIGID && it.MemberID == oCrm.CreateUser);
                        var sTitle = oApplicant.MemberName + @"的客訴申請「" + oCrm.ComplaintTitle + @"」客訴編號：" + oCrm.ComplaintNumber;
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }
                        //更新基本資料
                        var oCrmUpd = new OTB_CRM_Complaint
                        {
                            DataType = @"B",
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oCrmUpd)
                            .UpdateColumns(it => new { it.DataType, it.ModifyUser, it.ModifyDate })
                            .Where(it => it.Guid == sId).ExecuteCommand();
                        //更新代辦
                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sId);

                        var jaCheckFlows = (JArray)JsonConvert.DeserializeObject(oCrm.CheckFlows);
                        var MinOrderToNotice = jaCheckFlows.Min(x => ((JObject)x)["Order"].ToString()).ToString();
                        var saSignedId = jaCheckFlows.Where(x => ((JObject)x)["Order"].ToString() == MinOrderToNotice).Select(x => ((JObject)x)["SignedId"].ToString()).ToList();

                        if (saSignedId.Count > 0)
                        {
                            foreach (string signedId in saSignedId)
                            {
                                //添加提醒消息
                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, signedId, @"Complaint_View" + @"|?Action=Upd&Guid=" + oCrm.Guid, WebAppGlobalConstWord.BELL);
                                db.Insertable(oTipsAdd).ExecuteCommand();
                                //添加代辦
                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, oCrm.Guid, signedId, sTitle, @"Complaint_View", @"?Action=Upd&Guid=" + oCrm.Guid, @"B");
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Complaint_UpdService), @"客訴管理", @"ComplaintToAudit（客訴 提交簽核）", @"", @"", @"");
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

        #endregion 客訴 提交簽核


        #region 客訴 簽核

        /// <summary>
        /// 客訴 簽核
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on ComplaintAudit</param>
        /// <returns></returns>
        public ResponseMessage ComplaintAudit(RequestMessage i_crm)
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
                        var sdb = new SimpleClient<OTB_CRM_Complaint>(db);
                        var oEip = sdb.GetById(sId);
                        var TipsType = WebAppGlobalConstWord.CHECK;
                        if (oEip == null)
                        {
                            sMsg = @"系統找不到對應的客訴資料，請核查！";
                            break;
                        }
                        //簽核單據傳給下一位簽核者
                        var saNextSignedWays = (JArray)JsonConvert.DeserializeObject(sNextSignedWays);
                        var saNextUsers = (JArray)JsonConvert.DeserializeObject(sNextUsers);
                        var saTipsUsers = (JArray)JsonConvert.DeserializeObject(sTipsUsers);

                        if (oEip.Guid == null)
                        {
                            sMsg = @"系統找不到對應的客訴資料，請核查！";
                            break;
                        }
                        var oUser_Self = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == i_crm.ORIGID && it.MemberID == i_crm.USERID);
                        var oAskTheDummy = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == i_crm.ORIGID && it.MemberID == oEip.CreateUser);
                        var sTitle_Self = @"";
                        var sTitle_Handle = oUser_Self.MemberName + @"審批了" + oAskTheDummy.MemberName + @"的客訴申請「" + oEip.ComplaintTitle + @"」客訴編號：" + oEip.ComplaintNumber;
                        var sTitle_Next = oAskTheDummy.MemberName + @"的客訴申請「" + oEip.ComplaintTitle + @"」簽呈編號：" + oEip.ComplaintNumber;
                        var sTitle_Notice = @"";
                        var sStatus = @"B";

                        if (sAction == @"Signed")
                        {
                            sTitle_Self = oUser_Self.MemberName + @"審批了您的客訴申請「" + oEip.ComplaintTitle + @"」客訴編號：" + oEip.ComplaintNumber;
                            sTitle_Notice = oAskTheDummy.MemberName + @"的客訴申請「" + oEip.ComplaintTitle + @"」客訴編號：" + oEip.ComplaintNumber + @"，請點擊查看...";
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
                            sTitle_Self = oUser_Self.MemberName + @"簽辦了您的客訴申請「" + oEip.ComplaintTitle + @"」客訴編號：" + oEip.ComplaintNumber;
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
                        //更新基本資料
                        var oEipUpd = new OTB_CRM_Complaint
                        {
                            DataType = sStatus,
                            CheckFlows = sCheckFlows,
                            HandleFlows = sHandleFlows,
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oEipUpd)
                            .UpdateColumns(it => new { it.DataType, it.CheckFlows, it.HandleFlows, it.ModifyUser, it.ModifyDate })
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
                                            var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Notice, user, @"Complaint_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
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
                                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, oEip.Guid, sHandlePerson, sTitle_Handle, @"Complaint_View", @"?Action=Upd&Guid=" + oEip.Guid, @"E");
                                                listTask.Add(oTaskAdd);
                                                //添加提醒消息
                                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Handle, sHandlePerson, @"Complaint_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
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
                                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, oEip.Guid, user, sTitle_Next, @"Complaint_View", @"?Action=Upd&Guid=" + oEip.Guid, @"G");
                                                listTask.Add(oTaskAdd);
                                                //添加提醒消息
                                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Next, user, @"Complaint_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
                                                listTips.Add(oTipsAdd);
                                                listToTips.Add(user);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //添加提醒消息(給申請的人)
                        var oTips_Applicant = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Self, oEip.CreateUser, @"Complaint_View" + @"|?Action=Upd&Guid=" + oEip.Guid, TipsType);
                        listTips.Add(oTips_Applicant);
                        listToTips.Add(oEip.CreateUser);

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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Complaint_UpdService), @"客訴管理", @"ComplaintAudit（請款單（廠商）簽核|签辦）", @"", @"", @"");
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



        #endregion 請款單（廠商）簽核|签辦

    }
}
