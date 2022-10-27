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
    public class AttendanceDiff_UpdService : ServiceBase
    {
        #region 差勤異常申請表提交簽核

        /// <summary>
        /// 差勤異常申請表提交簽核
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on AttendanceDiffToAudit</param>
        /// <returns></returns>
        public ResponseMessage AttendanceDiffToAudit(RequestMessage i_crm)
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
                        var sdb = new SimpleClient<OTB_EIP_AttendanceDiff>(db);
                        var oEip = sdb.GetById(sId);

                        if (oEip == null)
                        {
                            sMsg = @"系統找不到對應的差勤異常申請表資料，請核查！";
                            break;
                        }
                        var oAskTheDummy = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == oEip.AskTheDummy);
                        var sTitle = oAskTheDummy.MemberName + @"的差勤異常申請表申請「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }
                        //更新基本資料
                        var oEipUpd = new OTB_EIP_AttendanceDiff
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
                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, signedId, @"AttendanceDiff_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
                                db.Insertable(oTipsAdd).ExecuteCommand();
                                //添加代辦
                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, oEip.Guid, signedId, sTitle, @"AttendanceDiff_View", @"?Action=Upd&Guid=" + oEip.Guid, @"B");
                                db.Insertable(oTaskAdd).ExecuteCommand();
                            }
                        }

                        //檢核未打卡提示中有沒有提醒資料，如果有的話就刪除
                        var saClockTips = db.Queryable<OTB_SYS_ClockTips>()
                            .Where(x => x.OrgID == i_crm.ORIGID && x.Owner == oEip.AskTheDummy && x.TipsDate.Value.Date == oEip.FillBrushDate.Value.Date)
                            .ToList();
                        if (saClockTips.Count > 0)
                        {
                            db.Deleteable(saClockTips).ExecuteCommand();
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(AttendanceDiff_UpdService), @"差勤異常申請表管理", @"AttendanceDiffToAudit（差勤異常申請表提交簽核）", @"", @"", @"");
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

        #endregion 差勤異常申請表提交簽核

        #region 差勤異常申請表簽核|签辦

        /// <summary>
        /// 差勤異常申請表簽核|签辦
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on AttendanceDiffAudit</param>
        /// <returns></returns>
        public ResponseMessage AttendanceDiffAudit(RequestMessage i_crm)
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
                        var sdb = new SimpleClient<OTB_EIP_AttendanceDiff>(db);
                        var oEip = sdb.GetById(sId);

                        if (oEip == null)
                        {
                            sMsg = @"系統找不到對應的差勤異常申請表資料，請核查！";
                            break;
                        }

                        var saNextSignedWays = (JArray)JsonConvert.DeserializeObject(sNextSignedWays);
                        var saNextUsers = (JArray)JsonConvert.DeserializeObject(sNextUsers);
                        var saTipsUsers = (JArray)JsonConvert.DeserializeObject(sTipsUsers);
                        var oUser_Self = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        var oAskTheDummy = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == oEip.AskTheDummy);
                        var sTitle_Self = @"";
                        var sTitle_Handle = oUser_Self.MemberName + @"審批了" + oAskTheDummy.MemberName + @"的差勤異常申請表「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
                        var sTitle_Next = oAskTheDummy.MemberName + @"的差勤異常申請表申請「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
                        var sTitle_Notice = @"";
                        var sStatus = @"B";
                        var TipsType = WebAppGlobalConstWord.CHECK;
                        if (sAction == @"Signed")
                        {
                            sTitle_Self = oUser_Self.MemberName + @"審批了您的差勤異常申請表「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
                            sTitle_Notice = oAskTheDummy.MemberName + @"的差勤異常申請表申請「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber + @"，請點擊查看...";
                            if (sSignedDecision == @"Y")
                            {
                                sTitle_Self += @"，審批結果：同意";
                                if (sHandlePerson != @"")
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
                            sTitle_Self = oUser_Self.MemberName + @"签辦了您的差勤異常申請表「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
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
                        var oEipUpd = new OTB_EIP_AttendanceDiff
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
                                            var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Notice, user, @"AttendanceDiff_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
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
                                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, oEip.Guid, sHandlePerson, sTitle_Handle, @"AttendanceDiff_View", @"?Action=Upd&Guid=" + oEip.Guid, @"E");
                                                listTask.Add(oTaskAdd);
                                                //添加提醒消息
                                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Handle, sHandlePerson, @"AttendanceDiff_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
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
                                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, oEip.Guid, user, sTitle_Next, @"AttendanceDiff_View", @"?Action=Upd&Guid=" + oEip.Guid, @"G");
                                                listTask.Add(oTaskAdd);
                                                //添加提醒消息
                                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Next, user, @"AttendanceDiff_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
                                                listTips.Add(oTipsAdd);
                                                listToTips.Add(user);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //添加提醒消息(給請假的人)
                        var oTips_AskTheDummy = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Self, oEip.AskTheDummy, @"AttendanceDiff_View" + @"|?Action=Upd&Guid=" + oEip.Guid, TipsType);
                        listTips.Add(oTips_AskTheDummy);
                        listToTips.Add(oEip.AskTheDummy);

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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(AttendanceDiff_UpdService), @"差勤異常申請表管理", @"AttendanceDiffAudit（差勤異常申請表簽核|签辦）", @"", @"", @"");
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

        #endregion 差勤異常申請表簽核|签辦
    }
}