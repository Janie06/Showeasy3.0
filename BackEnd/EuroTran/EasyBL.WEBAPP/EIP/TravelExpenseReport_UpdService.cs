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
    public class TravelExpenseReport_UpdService : ServiceBase
    {
        public const string PGNAME = "國外差旅費報告書";
        public const string SERVICER = nameof(TravelExpenseReport_UpdService);

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
                        //客戶資料表身
                        var oEntity = _fetchEntity<OTB_EIP_TravelExpense>(i_crm);
                        _setEntityBase(oEntity, i_crm);
                        UpdateTravelFees(oEntity);

                        var saPm = oEntity.SignedNumber.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                        oEntity.SignedNumber = SerialNumber.GetMaxNumberByType(saPm[1].ToString(), saPm[2].ToString(), SerialNumber.GetMaxNumberType(saPm[3].ToString()), i_crm.USERID, int.Parse(saPm[4].ToString()), saPm.Length > 5 ? saPm[5] : "", saPm.Length > 6 ? saPm[6] : "");    //獲取最大編號

                        var iRel = db.Insertable(oEntity).ExecuteReturnEntity();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, iRel);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LeaveRequest_UpdService), @"請假區間編輯", @"Add（請假區間編輯（新增））", @"", @"", @"");
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

        /// <summary>
        /// 更新資料
        /// TODO:前端提交審核(儲存+審核)可以合併
        /// </summary>
        /// <param name="i_crm"></param>
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
                        var oNewEntity = _fetchEntity<OTB_EIP_TravelExpense>(i_crm);
                        _setEntityBase(oNewEntity, i_crm);
                        UpdateTravelFees(oNewEntity);

                        var iRel = db.Updateable(oNewEntity).ExecuteCommand();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oNewEntity);
                    } while (false);

                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LeaveRequest_UpdService), @"請假區間編輯", @"Update（請假區間編輯（修改））", @"", @"", @"");
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

        #region 國外差旅費報告書提交簽核

        /// <summary>
        /// 國外差旅費報告書提交簽核
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on TravelExpenseReportToAudit</param>
        /// <returns></returns>
        public ResponseMessage TravelExpenseReportToAudit(RequestMessage i_crm)
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
                        var sdb = new SimpleClient<OTB_EIP_TravelExpense>(db);
                        var oEip = sdb.GetById(sId);

                        if (oEip == null)
                        {
                            sMsg = @"系統找不到對應的國外差旅費報告書資料，請核查！";
                            break;
                        }

                        var oApplicant = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == oEip.Applicant);

                        var sTitle = oApplicant.MemberName + @"的國外差旅費報告書申請「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
                        if (i_crm.LANG == @"zh")
                        {
                            sTitle = ChineseStringUtility.ToSimplified(sTitle);
                        }
                        //更新基本資料
                        var oEipUpd = new OTB_EIP_TravelExpense
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
                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, signedId, @"TravelExpenseReport_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
                                db.Insertable(oTipsAdd).ExecuteCommand();
                                //添加代辦
                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, oEip.Guid, signedId, sTitle, @"TravelExpenseReport_View", @"?Action=Upd&Guid=" + oEip.Guid, @"B");
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(TravelExpenseReport_UpdService), @"國外差旅費報告書", @"TravelExpenseReportToAudit（國外差旅費報告書提交簽核）", @"", @"", @"");
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

        #endregion 國外差旅費報告書提交簽核

        #region 國外差旅費報告書簽核|签辦

        /// <summary>
        /// 國外差旅費報告書簽核|签辦
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on TravelExpenseReportAudit</param>
        /// <returns></returns>
        public ResponseMessage TravelExpenseReportAudit(RequestMessage i_crm)
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
                        var TipsType = WebAppGlobalConstWord.CHECK;
                        var sdb = new SimpleClient<OTB_EIP_TravelExpense>(db);
                        var oEip = sdb.GetById(sId);

                        if (oEip == null)
                        {
                            sMsg = @"系統找不到對應的國外差旅費報告書資料，請核查！";
                            break;
                        }

                        var saNextSignedWays = (JArray)JsonConvert.DeserializeObject(sNextSignedWays);
                        var saNextUsers = (JArray)JsonConvert.DeserializeObject(sNextUsers);
                        var saTipsUsers = (JArray)JsonConvert.DeserializeObject(sTipsUsers);

                        if (oEip.Guid == null)
                        {
                            sMsg = @"系統找不到對應的國外差旅費報告書資料，請核查！";
                            break;
                        }
                        var oUser_Self = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == i_crm.USERID);
                        var oAskTheDummy = db.Queryable<OTB_SYS_Members>().Single(x => x.OrgID == i_crm.ORIGID && x.MemberID == oEip.Applicant);
                        var sTitle_Self = @"";
                        var sTitle_Handle = oUser_Self.MemberName + @"審批了" + oAskTheDummy.MemberName + @"的國外差旅費報告書「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
                        var sTitle_Next = oAskTheDummy.MemberName + @"的國外差旅費報告書申請「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
                        var sTitle_Notice = @"";
                        var sStatus = @"B";

                        if (sAction == @"Signed")
                        {
                            sTitle_Self = oUser_Self.MemberName + @"審批了您的國外差旅費報告書「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
                            sTitle_Notice = oAskTheDummy.MemberName + @"的國外差旅費報告書申請「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber + @"，請點擊查看...";
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
                            sTitle_Self = oUser_Self.MemberName + @"签辦了您的國外差旅費報告書申請「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
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
                        var oEipUpd = new OTB_EIP_TravelExpense
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
                                            var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Notice, user, @"TravelExpenseReport_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
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
                                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, oEip.Guid, sHandlePerson, sTitle_Handle, @"TravelExpenseReport_View", @"?Action=Upd&Guid=" + oEip.Guid, @"E");
                                                listTask.Add(oTaskAdd);
                                                //添加提醒消息
                                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Handle, sHandlePerson, @"TravelExpenseReport_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
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
                                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, oEip.Guid, user, sTitle_Next, @"TravelExpenseReport_View", @"?Action=Upd&Guid=" + oEip.Guid, @"G");
                                                listTask.Add(oTaskAdd);
                                                //添加提醒消息
                                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Next, user, @"TravelExpenseReport_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
                                                listTips.Add(oTipsAdd);
                                                listToTips.Add(user);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //添加提醒消息(給請假的人)
                        var oTips_AskTheDummy = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Self, oEip.Applicant, @"TravelExpenseReport_View" + @"|?Action=Upd&Guid=" + oEip.Guid, TipsType);
                        listTips.Add(oTips_AskTheDummy);
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
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(TravelExpenseReport_UpdService), @"國外差旅費報告書", @"TravelExpenseReportAudit（國外差旅費報告書簽核|签辦）", @"", @"", @"");
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

        #endregion 國外差旅費報告書簽核|签辦

        /// <summary>
        /// 更新費用項目
        /// 目的解決加總對不上問題
        /// </summary>
        /// <param name="travelExpense"></param>
        public void UpdateTravelFees(OTB_EIP_TravelExpense travelExpense)
        {
            var Round = 0;
            if (travelExpense.OrgID.Equals("SG", StringComparison.OrdinalIgnoreCase))
            {
                Round = 2;
            }
            //先處理TG、TE
            var travelFeeItems = JsonConvert.DeserializeObject<List<TravelFeeItems>>(travelExpense.TravelFeeItems).OrderBy(c => c.Index).ToList();
            var newItemInfo = new TravelFeeInfo();
            var travelFeeInfo = JsonConvert.DeserializeObject<TravelFeeInfo>(travelExpense.TravelFeeInfo);
            travelFeeInfo.Total = 0;
            foreach (var feeItems in travelFeeItems)
            {

                var total = (feeItems.Amount1 + feeItems.Amount2 + feeItems.Amount3) * feeItems.ExchangeRate;
                feeItems.Total = Math.Round(total, Round, MidpointRounding.AwayFromZero);
                newItemInfo.Total += Math.Round(feeItems.Total, Round, MidpointRounding.AwayFromZero);
            }
            newItemInfo.CompanyAdvance = travelFeeInfo.CompanyAdvance;
            travelExpense.TravelFeeInfo = JsonConvert.SerializeObject(newItemInfo);
            travelExpense.TravelFeeItems = JsonConvert.SerializeObject(travelFeeItems);

        }
    }

    /// <summary>
    /// 國外差旅費用項目
    /// </summary>
    public class TravelFeeItems
    {
        public string guid { set; get; }
        public int Index { set; get; }

        /// <summary>
        /// 日期
        /// </summary>
        public string Date { set; get; }

        /// <summary>
        /// PARTICULARS
        /// </summary>
        public string PARTICULARS { set; get; }

        /// <summary>
        /// 幣別
        /// </summary>
        public string Currency { set; get; }

        /// <summary>
        /// 日支額
        /// </summary>
        public decimal Amount1 { set; get; }

        /// <summary>
        /// 住宿費
        /// </summary>
        public decimal Amount2 { set; get; }

        /// <summary>
        /// 其他
        /// </summary>
        public decimal Amount3 { set; get; }

        /// <summary>
        /// 匯率
        /// </summary>
        public decimal ExchangeRate { set; get; }

        /// <summary>
        /// 總計
        /// </summary>
        public decimal Total { set; get; }

    }

    /// <summary>
    /// 國外差旅費用總計
    /// </summary>
    public class TravelFeeInfo
    {
        /// <summary>
        /// TOTAL
        /// </summary>
        public decimal Total { set; get; }
        /// <summary>
        /// 公司預支
        /// </summary>
        public decimal CompanyAdvance { set; get; }
        /// <summary>
        /// 公司應付
        /// </summary>
        public decimal CompanyCope
        {
            get
            {
                return Total - CompanyAdvance;
            }
        }
        /// <summary>
        /// 總計(等同公司應付)
        /// </summary>
        public decimal Sum
        {
            get
            {
                return CompanyCope;
            }
        }
    }
}