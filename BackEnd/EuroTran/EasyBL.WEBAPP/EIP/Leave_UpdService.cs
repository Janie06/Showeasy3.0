using EasyBL.WebApi.Message;
using EasyNet;
using Entity.Sugar;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace EasyBL.WEBAPP.EIP
{
    public class Leave_UpdService : ServiceBase
    {

        #region 請假單提交簽核

        /// <summary>
        /// 請假單提交簽核
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on LeaveToAudit</param>
        /// <returns></returns>
        public ResponseMessage LeaveToAudit(RequestMessage i_crm)
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
                        var sLeaveSetGuid = _fetchString(i_crm, @"LeaveSetGuid");
                        var sTotalTime = _fetchString(i_crm, @"TotalTime");
                        var sHolidayCategory = _fetchString(i_crm, @"HolidayCategory");
                        var sLeaveSetInfo = _fetchString(i_crm, @"LeaveSetInfo");
                        var sOrgID = _fetchString(i_crm, @"OrgID");
                        var strCreateUser = _fetchString(i_crm, @"CreateUser");
                        var strStartDate = _fetchString(i_crm, @"StartDate");
                        var strEndDate = _fetchString(i_crm, @"EndDate");
                        var sDaysOfLeaves = _fetchString(i_crm, @"DaysOfLeaves");
                        var sLeaveRequestUsing = _fetchString(i_crm, @"LeaveRequestUsing");
                        var sRoundToInterger = _fetchString(i_crm, @"RoundToInterger");
                        var ListDaysOfLeaves = new List<string>();
                        var UsingDateTimeFormate = "yyyy-MM-dd";
                        var AnnualLeaveMemoInfo = new List<Tuple<string,decimal>>();
                        var NormalLeaveMemoInfo = new List<Tuple<string, decimal>>();
                        var NormalRequestGuid = ",";
                        var LeaveMemo = "";
                        //轉換物件
                        var JaDaysOfLeaves = (JArray)JsonConvert.DeserializeObject(sDaysOfLeaves);
                        foreach (var bill in JaDaysOfLeaves)
                        {
                            var asklof = bill.ToString().Split(new char[] { ':', '|' }, StringSplitOptions.RemoveEmptyEntries);
                            ListDaysOfLeaves.Add(bill.ToString());
                        }

                        var JaLeaveRequestUsings = (JArray)JsonConvert.DeserializeObject(sLeaveRequestUsing);
                        foreach (var lr in JaLeaveRequestUsings)
                        {
                            NormalRequestGuid += "," + lr["Guid"].ToString();
                            decimal.TryParse(lr["UsedHours"].ToString(), out var Result);
                            NormalLeaveMemoInfo.Add(new Tuple<string, decimal>(lr["Guid"].ToString(), Result));
                        }

                        //檢查傳進的ListDaysOfLeaves，一定會有請假資料。
                        if (!ListDaysOfLeaves.Any())
                        {
                            sMsg = "請求錯誤，請重新整理。";
                            break;
                        }

                        //取得請假天數
                        var oMonthLeaveDay = db.Queryable<OTB_EIP_Leave>()
                                           .Where(a => a.AskTheDummy == strCreateUser && a.OrgID == sOrgID && a.StartDate >= DateTime.Parse(strStartDate) && a.EndDate <= DateTime.Parse(strEndDate) && (a.Status != "C-O" && a.Status != "D-O" && a.Status != "X" && a.Status != "A"))
                                           .Count();
                        //判斷請假時間是否已於系統上申請過
                        if (oMonthLeaveDay >= 1) //因會找到自己本身的資料，所以需額外+1
                        {
                            sMsg = "請假人已經在這段時間申請休假，請重新選擇日期";
                            break;
                        }
                        var sdb = new SimpleClient<OTB_EIP_Leave>(db);
                        var oEip = sdb.GetById(sId);

                        if (oEip == null)
                        {
                            sMsg = @"系統找不到對應的請假資料，請核查！";
                            break;
                        }

                        if (sHolidayCategory == @"09")
                        {
                            //取的當年度的特休
                            var ApplyAnnualleaveInfo = GetAnnualRange(db, i_crm.ORIGID, strCreateUser, strStartDate, strEndDate);
                            if (!ApplyAnnualleaveInfo.Item1)
                            {
                                sMsg = ApplyAnnualleaveInfo.Item2;
                                break;
                            }
                            else
                            {
                                decimal? iLeaveHours = Convert.ToDecimal(sTotalTime);
                                var ApplyWZ = ApplyAnnualleaveInfo.Item3;
                                var CheckTheSameYear = ApplyWZ.Count;
                                var UpdateWZData = new List<OTB_EIP_WenZhong>();
                                switch (CheckTheSameYear)
                                {
                                    //同年度
                                    case 1:
                                        {
                                            var UsingWZ = ApplyWZ.First();
                                            //檢查時數是否超過，扣除相對應
                                            if (UsingWZ.RemainHours < iLeaveHours)
                                                sMsg = $"{UsingWZ.EnableDate.Value.ToString(UsingDateTimeFormate) + " ~ " +UsingWZ.ExpirationDate.Value.ToString(UsingDateTimeFormate)}" +
                                                        $"特休剩餘時數不足夠，無法請假。";
                                            else
                                            {
                                                var oWenZhongUpd = new OTB_EIP_WenZhong()
                                                {
                                                    Guid = UsingWZ.Guid,
                                                    DelFlag = false,
                                                    RemainHours = UsingWZ.RemainHours - iLeaveHours,
                                                    UsedHours = UsingWZ.UsedHours + iLeaveHours
                                                };
                                                UpdateWZData.Add(oWenZhongUpd);
                                                AnnualLeaveMemoInfo.Add(new Tuple<string, decimal>(UsingWZ.Guid, iLeaveHours.Value));
                                            }
                                           
                                        }
                                        break;
                                    //跨年度
                                    case 2:
                                        {
                                            //檢查兩個時數是否超過，扣除相對應
                                            var TotalALHours = ApplyWZ.Sum(c => c.RemainHours);
                                            if (TotalALHours < iLeaveHours) 
                                                sMsg = "上年度與今年度特休剩餘時數不足夠，無法請假。";
                                            else
                                            {
                                                var DefaultWorkingHours = 8.0m;
                                                #region 開始請假~上年度結束時間。
                                                var LastYearWZ = ApplyWZ[0];
                                                var StartDate = Convert.ToDateTime(strStartDate).Date;
                                                var WZExpiredDay = LastYearWZ.ExpirationDate.Value.Date;
                                                var LastYearEstimateUse = 0.0m;
                                                var NowYearEstimateUse = 0.0m;
                                                while (StartDate <= WZExpiredDay)
                                                {
                                                    var StartDtDay = StartDate.ToString(UsingDateTimeFormate);
                                                    var FoundDayOfLeave = ListDaysOfLeaves.Where(c => c.Contains(StartDtDay));
                                                    if (FoundDayOfLeave.Any())
                                                    {
                                                        var UsedLeaveInfo = FoundDayOfLeave.First().Split(new char[] { ':', '|' });
                                                        var UsedDay = Convert.ToDecimal(UsedLeaveInfo[1]) * DefaultWorkingHours;
                                                        var UsedHour = Convert.ToDecimal(UsedLeaveInfo[2]);
                                                        LastYearEstimateUse += UsedDay + UsedHour;
                                                    }
                                                    StartDate = StartDate.AddDays(1);
                                                }
                                                if (LastYearWZ.RemainHours < LastYearEstimateUse)
                                                {
                                                    sMsg += $"{LastYearWZ.EnableDate.Value.ToString(UsingDateTimeFormate) + " ~ " + LastYearWZ.ExpirationDate.Value.ToString(UsingDateTimeFormate)}" +
                                                    "特休剩餘時數不足夠，無法請假。";
                                                }
                                                else
                                                {
                                                    var oWenZhongUpd = new OTB_EIP_WenZhong()
                                                    {
                                                        Guid = LastYearWZ.Guid,
                                                        DelFlag = false,
                                                        RemainHours = LastYearWZ.RemainHours - LastYearEstimateUse,
                                                        UsedHours = LastYearWZ.UsedHours + LastYearEstimateUse
                                                    };
                                                    AnnualLeaveMemoInfo.Add(new Tuple<string, decimal>(LastYearWZ.Guid, LastYearEstimateUse));
                                                    UpdateWZData.Add(oWenZhongUpd);
                                                }
                                                #endregion

                                                #region MyRegion
                                                //上年度開始時間~結束請假時間
                                                var NowYearWZ = ApplyWZ[1];
                                                var EndDate = Convert.ToDateTime(strEndDate).Date;
                                                var WZEnabledDay = NowYearWZ.EnableDate.Value.Date;
                                                
                                                while (WZEnabledDay <= EndDate)
                                                {
                                                    var EnabledDay = WZEnabledDay.ToString(UsingDateTimeFormate);
                                                    var FoundDayOfLeave = ListDaysOfLeaves.Where(c => c.Contains(EnabledDay));
                                                    if (FoundDayOfLeave.Any())
                                                    {
                                                        var UsedLeaveInfo = FoundDayOfLeave.First().Split(new char[] { ':', '|' });
                                                        var UsedDay = Convert.ToDecimal(UsedLeaveInfo[1]) * DefaultWorkingHours;
                                                        var UsedHour = Convert.ToDecimal(UsedLeaveInfo[2]);
                                                        NowYearEstimateUse += UsedDay + UsedHour;
                                                    }
                                                    WZEnabledDay = WZEnabledDay.AddDays(1);
                                                }
                                                if (NowYearWZ.RemainHours < NowYearEstimateUse)
                                                {
                                                    sMsg += $"{NowYearWZ.EnableDate.Value.ToString(UsingDateTimeFormate) + " ~ " + NowYearWZ.ExpirationDate.Value.ToString(UsingDateTimeFormate)}" +
                                                    "特休剩餘時數不足夠，無法請假。";
                                                }
                                                else
                                                {
                                                    var oWenZhongUpd = new OTB_EIP_WenZhong()
                                                    {
                                                        Guid = NowYearWZ.Guid,
                                                        DelFlag = false,
                                                        RemainHours = NowYearWZ.RemainHours - NowYearEstimateUse,
                                                        UsedHours = NowYearWZ.UsedHours + NowYearEstimateUse
                                                    };
                                                    AnnualLeaveMemoInfo.Add(new Tuple<string, decimal>(NowYearWZ.Guid, NowYearEstimateUse));
                                                    UpdateWZData.Add(oWenZhongUpd);
                                                }
                                                #endregion
                                                }
                                        }
                                        break;
                                    default:
                                        break;
                                }

                                if (string.IsNullOrWhiteSpace(sMsg) && UpdateWZData.Any()) 
                                {
                                    db.Updateable(UpdateWZData).UpdateColumns(it => new { it.UsedHours, it.RemainHours, it.DelFlag }).ExecuteCommand();
                                    var AnnualLeaveMemoJson = from d in AnnualLeaveMemoInfo
                                                              select new { Guid = d.Item1, Hours = d.Item2 };
                                    LeaveMemo = JsonToString(AnnualLeaveMemoJson);
                                }
                                else
                                {
                                    break;
                                }
                                
                            }
                        }
                        else
                        {
                            
                            var UsingLeaveRequest = db.Queryable<OTB_EIP_LeaveRequest>().Where(t1 => NormalRequestGuid.Contains(t1.guid)).ToList();
                            foreach (var NLMI in NormalLeaveMemoInfo)
                            {
                                var LeaveRequest = UsingLeaveRequest.Where(t1 => t1.guid == NLMI.Item1).Single();
                                LeaveRequest.UsedHours += NLMI.Item2;
                                LeaveRequest.RemainHours = LeaveRequest.PaymentHours - LeaveRequest.UsedHours;
                            }
                            if(UsingLeaveRequest.Any())
                                db.Updateable(UsingLeaveRequest).UpdateColumns(it => new { it.PaymentHours, it.RemainHours, it.UsedHours }).ExecuteCommand();
                            //.Where(it => it.Guid == sLeaveSetGuid).ExecuteCommand();

                            var oLeaveSetUpd = new OTB_EIP_LeaveSet
                            {
                                SetInfo = sLeaveSetInfo
                            };
                            db.Updateable(oLeaveSetUpd).UpdateColumns(it => new { it.SetInfo })
                                .Where(it => it.Guid == sLeaveSetGuid).ExecuteCommand();

                            var NormalLeaveMemoJson = from d in NormalLeaveMemoInfo
                                                      select new { Guid = d.Item1, Hours = d.Item2 };
                            LeaveMemo = JsonToString(NormalLeaveMemoJson);

                        }
                        var oLeaveUpd = new OTB_EIP_Leave
                        {
                            AnnualLeaveMemo = LeaveMemo,
                            Status = @"B",
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oLeaveUpd).UpdateColumns(it => new { it.Status, it.ModifyDate, it.ModifyUser, it.AnnualLeaveMemo })
                            .Where(it => it.Guid == sId).ExecuteCommand();

                        SYS.Task_QryService.TaskStatusUpd(db, i_crm.ORIGID, sId);


                        if (!string.IsNullOrEmpty(oEip.Agent_Person))
                        {
                            var oAskTheDummy = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == i_crm.ORIGID && it.MemberID == oEip.AskTheDummy);

                            var sTitle = oAskTheDummy.MemberName + @"的請假單申請「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
                            if (i_crm.LANG == @"zh")
                            {
                                sTitle = ChineseStringUtility.ToSimplified(sTitle);
                            }
                            //添加提醒消息
                            var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle, oEip.Agent_Person, @"Leave_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
                            db.Insertable(oTipsAdd).ExecuteCommand();
                            //添加代辦
                            var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, oEip.Guid, oEip.Agent_Person, sTitle, @"Leave_View", @"?Action=Upd&Guid=" + oEip.Guid, @"B");
                            db.Insertable(oTaskAdd).ExecuteCommand();
                        }

                        //檢核未打卡提示中有沒有提醒資料，如果有的話就刪除
                        var saClockTips = db.Queryable<OTB_SYS_ClockTips>()
                            .Where(x => x.OrgID == i_crm.ORIGID && x.Owner == oEip.AskTheDummy && x.TipsDate <= oEip.EndDate && x.TipsDate >= oEip.StartDate)
                            .ToList();
                        if (saClockTips.Count > 0)
                        {
                            db.Deleteable(saClockTips).ExecuteCommand();
                        }

                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add(BLWording.REL, oEip.Agent_Person);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Leave_UpdService), @"請假單管理", @"LeaveToAudit（請假單提交簽核）", @"", @"", @"");
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



        #endregion 請假單提交簽核

        /// <summary>
        /// 取得請假當下的區間
        /// </summary>
        /// <param name="db"></param>
        /// <param name="OrgID"></param>
        /// <param name="UserID"></param>
        /// <param name="strStartDate"></param>
        /// <param name="strEndDate"></param>
        /// <returns>依序:是否成功、訊息、資料</returns>
        private Tuple<bool,string,List<OTB_EIP_WenZhong>> GetAnnualRange( SqlSugarClient db,string OrgID, string UserID ,string strStartDate, string strEndDate)
        {
            var AllMatchedWenZhong = db.Queryable<OTB_EIP_WenZhong>()
                            .Where(it => it.OrgID == OrgID && it.UserID == UserID ).OrderBy(it => it.EnableDate).ToList();
            if (AllMatchedWenZhong.Count == 0)
            {
                return new Tuple<bool, string, List<OTB_EIP_WenZhong>>(false, "系統找不到對應的文中特休假設定，請核查！", new List<OTB_EIP_WenZhong>());
            }
            else
            {
                var saWZLastYear = AllMatchedWenZhong.Where(it => it.EnableDate <= DateTime.Parse(strStartDate).Date && it.ExpirationDate >= DateTime.Parse(strStartDate).Date).First();
                var saWZThisYear = AllMatchedWenZhong.Where(it => it.EnableDate <= DateTime.Parse(strEndDate).Date && it.ExpirationDate >= DateTime.Parse(strEndDate).Date).First();
                var CheckEmptyWZ = saWZThisYear == null && saWZLastYear == null;
                if (CheckEmptyWZ)
                {
                    return new Tuple<bool, string, List<OTB_EIP_WenZhong>>(false, "系統找不到對應的文中特休假設定，請核查！", new List<OTB_EIP_WenZhong>());
                }
                //在同個年度內。
                else if(saWZLastYear.Equals(saWZThisYear))
                {
                    return new Tuple<bool, string, List<OTB_EIP_WenZhong>>(true, "請假區間在同文中年度內", new List<OTB_EIP_WenZhong>() { saWZLastYear });
                }
                //在不同年度內
                else
                {
                    return new Tuple<bool, string, List<OTB_EIP_WenZhong>>(true, "請假區間在不同文中年度內。", new List<OTB_EIP_WenZhong>() { saWZLastYear, saWZThisYear });
                }
            }
        }

        /// <summary>
        /// 檢查請假是否為小數
        /// </summary>
        /// <param name="db"></param>
        /// <param name="LeaveType"></param>
        /// <param name="sLeaveType"></param>
        /// <param name="OrgID"></param>
        /// <returns></returns>
        public bool CheckLeaveHourInterger(SqlSugarClient db, string sLeaveType, string OrgID)
        {
            var Default = false;
            try
            {
                do
                {
                    //取得出勤設定資料
                    var oLeaveRuelsSetting = db.Queryable<OTB_SYS_Arguments, OTB_SYS_ArgumentsRelated>
                       ((t1, t2) =>
                       new object[] {
                                JoinType.Left, t1.ArgumentID == t2.ArgumentID
                             }
                       )
                       .Where((t1, t2) => t1.ArgumentClassID == "LeaveType" && t1.ArgumentID == sLeaveType && t1.OrgID == OrgID && t2.OrgID == OrgID)
                       .Select((t1, t2) => new { t1.Correlation, t2.ExFeild4 })
                       .Single();
                    if (oLeaveRuelsSetting.ExFeild4 == "Y")
                        return true;
                    else
                        return Default;
                } while (false);
            }
            catch (Exception ex)
            {
                LogAndSendEmail(@"Param：" , ex, OrgID, "  ", nameof(Leave_UpdService), @"請假單管理", @"CheckInterger（檢查請假是否為小數）", @"", @"", @"");
            }
            return Default;
        }

        #region 請假單簽核|签辦

        /// <summary>
        /// 請假單簽核|签辦
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on LeaveAudit</param>
        /// <returns></returns>
        public ResponseMessage LeaveAudit(RequestMessage i_crm)
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
                        var sdb = new SimpleClient<OTB_EIP_Leave>(db);
                        var oEip = sdb.GetById(sId);
                        var TipsType = WebAppGlobalConstWord.CHECK;
                        if (oEip == null)
                        {
                            sMsg = @"系統找不到對應的請假資料，請核查！";
                            break;
                        }

                        var saNextSignedWays = (JArray)JsonConvert.DeserializeObject(sNextSignedWays);
                        var saNextUsers = (JArray)JsonConvert.DeserializeObject(sNextUsers);
                        var saTipsUsers = (JArray)JsonConvert.DeserializeObject(sTipsUsers);

                        var oUser_Self = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == i_crm.ORIGID && it.MemberID == i_crm.USERID);
                        var oAskTheDummy = db.Queryable<OTB_SYS_Members>().Single(it => it.OrgID == i_crm.ORIGID && it.MemberID == oEip.AskTheDummy);
                        var sTitle_Self = @"";
                        var sTitle_Handle = oUser_Self.MemberName + @"審批了" + oAskTheDummy.MemberName + @"的請假單「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
                        var sTitle_Next = oAskTheDummy.MemberName + @"的請假單申請「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
                        var sTitle_Notice = @"";
                        var sStatus = @"B";

                        if (sAction == @"Signed")
                        {
                            sTitle_Self = oUser_Self.MemberName + @"審批了您的請假單「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
                            sTitle_Notice = oAskTheDummy.MemberName + @"的請假單申請「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber + @"，請點擊查看...";
                            switch (sSignedDecision)
                            {
                                case @"Y":
                                    sTitle_Self += @"，審批結果：同意";
                                    if (sHandlePerson != @"")
                                    {
                                        sStatus = @"E";
                                    }

                                    break;

                                case @"N":
                                    sTitle_Self += @"，審批結果：不同意";
                                    sStatus = @"D-O";
                                    TipsType = WebAppGlobalConstWord.FAIL;
                                    break;

                                case @"O":
                                    sTitle_Self += @"，審批結果：先加簽";
                                    break;

                                default:
                                    break;
                            }
                        }
                        else
                        {
                            sTitle_Self = oUser_Self.MemberName + @"签辦了您的請假單「" + oEip.KeyNote + @"」簽呈編號：" + oEip.SignedNumber;
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

                        var oLeaveUpd = new OTB_EIP_Leave
                        {
                            Status = sStatus,
                            CheckFlows = sCheckFlows,
                            HandleFlows = sHandleFlows,
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oLeaveUpd)
                            .UpdateColumns(it => new { it.Status, it.CheckFlows, it.HandleFlows, it.ModifyDate, it.ModifyUser })
                            .Where(it => it.Guid == sId).ExecuteCommand();

                        var sOwner = @"";
                        if (sSignedDecision == @"Y" && sGoNext == @"N")
                        {
                            sOwner = i_crm.USERID;
                        }

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
                                            var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Notice, user, @"Leave_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
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
                                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, oEip.Guid, sHandlePerson, sTitle_Handle, @"Leave_View", @"?Action=Upd&Guid=" + oEip.Guid, @"E");
                                                listTask.Add(oTaskAdd);
                                                //添加提醒消息
                                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Handle, sHandlePerson, @"Leave_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
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
                                                var oTaskAdd = SYS.Task_QryService.TaskAdd(i_crm, oEip.Guid, user, sTitle_Next, @"Leave_View", @"?Action=Upd&Guid=" + oEip.Guid, @"G");
                                                listTask.Add(oTaskAdd);
                                                //添加提醒消息
                                                var oTipsAdd = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Next, user, @"Leave_View" + @"|?Action=Upd&Guid=" + oEip.Guid, WebAppGlobalConstWord.BELL);
                                                listTips.Add(oTipsAdd);
                                                listToTips.Add(user);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //添加提醒消息(給請假的人)
                        var oTips_AskTheDummy = SYS.Task_QryService.TipsAdd(i_crm, sTitle_Self, oEip.AskTheDummy, @"Leave_View" + @"|?Action=Upd&Guid=" + oEip.Guid, TipsType);
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

                        if (sStatus == @"D-O")
                        {
                            if (oEip.HolidayCategory == @"09")
                            {
                                var iYear = oEip.StartDate.Value.Year;
                                var saWenZhong = db.Queryable<OTB_EIP_WenZhong>()
                                       .Where(it => it.OrgID == i_crm.ORIGID && it.UserID == oEip.AskTheDummy && (it.EnableDate.Value.Year == iYear || it.ExpirationDate.Value.Year == iYear)).ToList();
                                if (saWenZhong.Count > 0)
                                {
                                    foreach (OTB_EIP_WenZhong wz in saWenZhong)
                                    {
                                        if ((DateTime)oEip.StartDate >= (DateTime)wz.EnableDate && (DateTime)oEip.StartDate <= (DateTime)wz.ExpirationDate)
                                        {
                                            var oWenZhongUpd = new OTB_EIP_WenZhong
                                            {
                                                UsedHours = (wz.UsedHours ?? 0) - oEip.TotalTime
                                            };
                                            oWenZhongUpd.RemainHours = (wz.PaymentHours ?? 0) - oWenZhongUpd.UsedHours;
                                            oWenZhongUpd.DelFlag = false;
                                            db.Updateable(oWenZhongUpd).UpdateColumns(it => new { it.UsedHours, it.RemainHours, it.DelFlag })
                                                .Where(it => it.Guid == wz.Guid).ExecuteCommand();
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var sYear = ((DateTime)oEip.StartDate).ToString(@"yyyy");
                                var oLeaveSet = db.Queryable<OTB_EIP_LeaveSet>().Single(it => it.OrgID == i_crm.ORIGID && it.UserID == oEip.AskTheDummy && it.TYear == sYear);
                                if (oLeaveSet != null)
                                {
                                    var jaSetInfo = (JArray)JsonConvert.DeserializeObject(oLeaveSet.SetInfo);
                                    foreach (JObject jo in jaSetInfo)
                                    {
                                        if (jo[@"Id"].ToString() == oEip.HolidayCategory)
                                        {
                                            var sUsedHours = jo[@"UsedHours"] == null ? @"0" : jo[@"UsedHours"].ToString();
                                            var sPaymentHours = jo[@"PaymentHours"] == null ? @"0" : jo[@"PaymentHours"].ToString();
                                            jo[@"UsedHours"] = Convert.ToDecimal(sUsedHours) - oEip.TotalTime;
                                            if (sPaymentHours != @"")
                                            {
                                                jo[@"RemainHours"] = Convert.ToDecimal(sPaymentHours) - Convert.ToDecimal(jo[@"UsedHours"].ToString());
                                            }
                                            break;
                                        }
                                    }
                                    var oLeaveSetUpd = new OTB_EIP_LeaveSet
                                    {
                                        SetInfo = JsonToString(jaSetInfo)
                                    };
                                    db.Updateable(oLeaveSetUpd).UpdateColumns(it => new { it.SetInfo })
                                        .Where(it => it.Guid == oLeaveSet.Guid).ExecuteCommand();
                                }
                            }
                        }
                        db.Ado.CommitTran();
                        rm = new SuccessResponseMessage(null, i_crm);
                        rm.DATA.Add("Status", sStatus);
                        rm.DATA.Add(BLWording.REL, listToTips);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Leave_UpdService), @"請假單管理", @"LeaveAudit（請假單簽核|签辦）", @"", @"", @"");
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

        #endregion 請假單簽核|签辦

        #region 作廢

        /// <summary>
        /// 作廢
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on SetVoid</param>
        /// <returns></returns>
        public ResponseMessage SetVoid(RequestMessage i_crm)
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
                        var sVoidReason = _fetchString(i_crm, @"VoidReason");

                        var sdb = new SimpleClient<OTB_EIP_Leave>(db);
                        var oEip = sdb.GetById(sId);
                        var iYear = oEip.StartDate.Value.Year;

                        if (oEip == null)
                        {
                            sMsg = @"系統找不到對應的請假資料，請核查！";
                            break;
                        }
                        if (oEip.HolidayCategory == @"09" && string.IsNullOrWhiteSpace(oEip.AnnualLeaveMemo))
                        {
                            sMsg = @"請假的特休資訊遺失，請資訊人員核查！";
                            break;

                        }
                        var AnnualLeaveMemos = (JArray)JsonConvert.DeserializeObject(oEip.AnnualLeaveMemo);

                        var oLeaveUpd = new OTB_EIP_Leave
                        {
                            Status = @"X",
                            VoidReason = sVoidReason,
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oLeaveUpd).UpdateColumns(it => new { it.Status, it.VoidReason, it.ModifyDate, it.ModifyUser })
                            .Where(it => it.Guid == sId).ExecuteCommand();

                        db.Deleteable<OTB_SYS_Task>()
                            .Where(it => it.OrgID == i_crm.ORIGID && it.SourceID == sId).ExecuteCommand();
                        new CalendarService().DeleteCalendar(i_crm.ORIGID, "", oEip.Guid);
                        if (oEip.HolidayCategory == @"09")
                        {
                            UpddateWenZhongAnnualLeave(db, i_crm.ORIGID, oEip.AskTheDummy, AnnualLeaveMemos);
                        }
                        else
                        {
                            var TotalRecoveryHours = oEip.TotalTime;
                            var RecoverLeaveRequestResult = UpddateNormalRequestLeave(db, i_crm.ORIGID, oEip.AskTheDummy, AnnualLeaveMemos);
                            TotalRecoveryHours -= RecoverLeaveRequestResult.Item3;
                            if(TotalRecoveryHours >0)
                            {
                                var sYear = iYear.ToString();
                                var oLeaveSet = db.Queryable<OTB_EIP_LeaveSet>().Single(it => it.OrgID == i_crm.ORIGID && it.UserID == oEip.AskTheDummy && it.TYear == sYear);
                                if (oLeaveSet != null)
                                {
                                    var jaSetInfo = (JArray)JsonConvert.DeserializeObject(oLeaveSet.SetInfo);
                                    foreach (JObject jo in jaSetInfo)
                                    {
                                        if (jo[@"Id"].ToString() == oEip.HolidayCategory)
                                        {
                                            jo[@"UsedHours"] = Convert.ToDecimal(jo[@"UsedHours"].ToString()) - oEip.TotalTime;
                                            jo[@"RemainHours"] = Convert.ToDecimal(jo[@"PaymentHours"].ToString()) - Convert.ToDecimal(jo[@"UsedHours"].ToString());
                                            break;
                                        }
                                    }
                                    var oLeaveSetUpd = new OTB_EIP_LeaveSet
                                    {
                                        SetInfo = JsonToString(jaSetInfo)
                                    };
                                    db.Updateable(oLeaveSetUpd).UpdateColumns(it => new { it.SetInfo })
                                        .Where(it => it.Guid == oLeaveSet.Guid).ExecuteCommand();
                                }
                            }

                        }
                        db.Ado.CommitTran();
                        rm = new SuccessResponseMessage(null, i_crm);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Leave_UpdService), @"請假單管理", @"SetVoid（作廢）", @"", @"", @"");
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

        #endregion 作廢

        #region 抽單

        /// <summary>
        /// 抽單動作
        /// </summary>
        /// <param name="i_crm">todo: describe i_crm parameter on SetReEdit</param>
        /// <returns></returns>
        public ResponseMessage SetReEdit(RequestMessage i_crm)
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

                        var sdb = new SimpleClient<OTB_EIP_Leave>(db);
                        var oEip = sdb.GetById(sId);
                        var iYear = oEip.StartDate.Value.Year;
                        if (oEip == null)
                        {
                            sMsg = @"系統找不到對應的請假資料，請核查！";
                            break;
                        }

                        if (oEip.HolidayCategory == @"09" && string.IsNullOrWhiteSpace(oEip.AnnualLeaveMemo) )
                        {
                            sMsg = @"請假的特休資訊遺失，請資訊人員核查！";
                            break;

                        }
                        var AnnualLeaveMemos = (JArray)JsonConvert.DeserializeObject(oEip.AnnualLeaveMemo);

                        var oLeaveUpd = new OTB_EIP_Leave
                        {
                            Status = @"C-O",
                            ModifyUser = i_crm.USERID,
                            ModifyDate = DateTime.Now
                        };
                        db.Updateable(oLeaveUpd).UpdateColumns(it => new { it.Status, it.ModifyDate, it.ModifyUser })
                            .Where(it => it.Guid == sId).ExecuteCommand();

                        db.Deleteable<OTB_SYS_Task>()
                            .Where(it => it.OrgID == i_crm.ORIGID && it.SourceID == sId).ExecuteCommand();
                        new CalendarService().DeleteCalendar(i_crm.ORIGID,"", oEip.Guid);
                        if (oEip.HolidayCategory == @"09")
                        {
                            UpddateWenZhongAnnualLeave(db, i_crm.ORIGID, oEip.AskTheDummy, AnnualLeaveMemos);
                        }
                        else
                        {
                            var TotalRecoveryHours = oEip.TotalTime;
                            var RecoverLeaveRequestResult = UpddateNormalRequestLeave(db, i_crm.ORIGID, oEip.AskTheDummy, AnnualLeaveMemos);
                            TotalRecoveryHours -= RecoverLeaveRequestResult.Item3;
                            if (TotalRecoveryHours > 0) 
                            {
                                var sYear = iYear.ToString();
                                var oLeaveSet = db.Queryable<OTB_EIP_LeaveSet>().Single(it => it.OrgID == i_crm.ORIGID && it.UserID == oEip.AskTheDummy && it.TYear == sYear);
                                if (oLeaveSet != null)
                                {
                                    var jaSetInfo = (JArray)JsonConvert.DeserializeObject(oLeaveSet.SetInfo);
                                    foreach (JObject jo in jaSetInfo)
                                    {
                                        if (jo[@"Id"].ToString() == oEip.HolidayCategory)
                                        {
                                            var sPaymentHours = jo[@"PaymentHours"].ToString();
                                            jo[@"UsedHours"] = Convert.ToDecimal(jo[@"UsedHours"].ToString()) - oEip.TotalTime;
                                            if (sPaymentHours != @"")
                                            {
                                                jo[@"RemainHours"] = Convert.ToDecimal(sPaymentHours) - Convert.ToDecimal(jo[@"UsedHours"].ToString());
                                            }
                                            break;
                                        }
                                    }
                                    var oLeaveSetUpd = new OTB_EIP_LeaveSet
                                    {
                                        SetInfo = JsonToString(jaSetInfo)
                                    };
                                    db.Updateable(oLeaveSetUpd).UpdateColumns(it => new { it.SetInfo })
                                        .Where(it => it.Guid == oLeaveSet.Guid).ExecuteCommand();
                                }
                            }
                        }
                        rm = new SuccessResponseMessage(null, i_crm);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Leave_UpdService), @"請假單管理", @"SetReEdit（抽單）", @"", @"", @"");
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

        #endregion 抽單

        #region 請假單管理（查詢筆數）

        /// <summary>
        /// 請假單管理（查詢筆數）
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
                    var sEndTime = _fetchString(i_crm, @"EndTime");
                    var iHours = _fetchInt(i_crm, @"Hours");

                    var rEndTime = Convert.ToDateTime(sEndTime);
                    var rStartTime = rEndTime.AddHours(-iHours);

                    var iCout = db.Queryable<OTB_EIP_Leave>()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.AskTheDummy == i_crm.USERID && x.EndDate > rStartTime && x.EndDate <= rEndTime)
                        .Count();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iCout);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Leave_UpdService), "", "QueryCout（請假單管理（查詢筆數））", "", "", "");
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

        #endregion 請假單管理（查詢筆數）

        #region 請假單管理（檢核當月最大請教時數）

        /// <summary>
        /// 請假單管理（檢核當月最大請教時數）
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage CheckMaxHours(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sDate = _fetchString(i_crm, @"Date");
                    var iCurHours = decimal.Parse(_fetchString(i_crm, @"CurHours"));
                    var iMaxHours = _fetchInt(i_crm, @"MaxHours");

                    var rDate = Convert.ToDateTime(sDate);
                    var iYear = rDate.Year;
                    var iNonth = rDate.Month;

                    var listData = db.Queryable<OTB_EIP_Leave>()
                        .Where(x => x.OrgID == i_crm.ORIGID && x.AskTheDummy == i_crm.USERID && x.StartDate.HasValue && x.StartDate.Value.Year == iYear)
                        .Select(x => new { x.StartDate, x.EndDate, x.TotalTime }).ToList();

                    var iHours = listData.Where(x => x.StartDate.Value.Month == iNonth || x.EndDate.Value.Month == iNonth).Sum(x => x.TotalTime);

                    //var iHours = db.Queryable<OTB_EIP_Leave>()
                    //    .Where(x => x.OrgID == i_crm.ORIGID && x.AskTheDummy == i_crm.USERID && x.StartDate.Value.Year == iYear && (x.StartDate.Value.Month == iNonth || x.EndDate.Value.Month == iNonth))
                    //    .Sum(x => x.TotalTime);
                    //.ToSql();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, iHours + iCurHours > iMaxHours);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + "Params：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Leave_UpdService), "", "QueryCout（請假單管理（檢核當月最大請教時數））", "", "", "");
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

        #endregion 請假單管理（檢核當月最大請教時數）

        #region 獲取請假規則設定

        /// <summary>
        /// 檢查假別
        /// </summary>
        /// <param name="i_crm"></param>
        /// <returns></returns>
        public ResponseMessage GetLeaveSetting(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var sArgumentID = _fetchString(i_crm, @"ArgumentID");
                    var sOrgID = _fetchString(i_crm, @"OrgID");

                    //取得出勤設定資料
                    var oLeaveRuelsSetting = db.Queryable<OTB_SYS_Arguments, OTB_SYS_ArgumentsRelated>
                       ((t1, t2) =>
                       new object[] {
                                JoinType.Left, t1.ArgumentID == t2.ArgumentID
                             }
                       )
                       .Where((t1, t2) => t1.ArgumentClassID == "LeaveType" && t1.ArgumentID == sArgumentID && t1.OrgID == sOrgID && t2.OrgID == sOrgID)
                       .Select((t1, t2) => new { t1.Correlation, t2.Field1, t2.Field2, t2.Field3 })
                       .Single();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, oLeaveRuelsSetting);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(LeaveSetService), @"出勤設定", @"GetLeaveSettingByType（依據請假類別獲取請假規則設定）", @"", @"", @"");
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

        #endregion 依據請假類別獲取請假規則設定

        #region GetUpdateWenZhong

        /// <summary>
        /// 返回特休假，leave一定要有資料
        /// </summary>
        /// <param name="db"></param>
        /// <param name="leave"></param>
        /// <returns></returns>
        public Tuple<bool, string> UpddateWenZhongAnnualLeave(SqlSugarClient db,string OrgID, string UserID, JArray AnnualLeaveData)
        {
            var Result = new List<OTB_EIP_WenZhong>();
            if (!AnnualLeaveData.Any())
                return new Tuple<bool, string>(false, @"請假的特休資訊遺失，請資訊人員核查！");
            if(string.IsNullOrEmpty(OrgID) || string.IsNullOrEmpty(UserID))
                return new Tuple<bool, string>(false, @"缺少組織或使用者資訊，請重新整理！");

            var RelativeWZGuid = new List<string>();
            var Dic = new Dictionary<string, decimal>();
            foreach (var ALM in AnnualLeaveData)
            {
                var Guid = ALM["Guid"].ToString();
                RelativeWZGuid.Add(ALM["Guid"].ToString());
                decimal.TryParse(ALM["Hours"].ToString(), out var Hours);
                Dic.Add(Guid, Hours);
            }
            var saWenZhong = db.Queryable<OTB_EIP_WenZhong>()
                   .Where(it => it.OrgID == OrgID && it.UserID == UserID && RelativeWZGuid.Contains(it.Guid)).ToList();
            if (saWenZhong.Count > 0)
            {
                var UpdateWZData = new List<OTB_EIP_WenZhong>();
                foreach (OTB_EIP_WenZhong wz in saWenZhong)
                {
                    decimal.TryParse(Dic[wz.Guid].ToString(), out var ReturnHours);
                    var oWenZhong = new OTB_EIP_WenZhong
                    {
                        Guid = wz.Guid,
                        UsedHours = wz.UsedHours - ReturnHours,
                        RemainHours = wz.RemainHours + ReturnHours,
                        DelFlag = false,
                    };
                    UpdateWZData.Add(oWenZhong);
                }
                if (UpdateWZData.Any())
                {
                    db.Updateable(UpdateWZData).UpdateColumns(it => new { it.Guid, it.UsedHours, it.RemainHours, it.DelFlag }).ExecuteCommand();
                }
            }
            return new Tuple<bool, string>(true,"");
        }

        #endregion


        public Tuple<bool, string,decimal> UpddateNormalRequestLeave(SqlSugarClient db, string OrgID, string UserID, JArray NormalLeaveData)
        {
            if (string.IsNullOrEmpty(OrgID) || string.IsNullOrEmpty(UserID))
                return new Tuple<bool, string, decimal>(false, @"缺少組織或使用者資訊，請重新整理！", decimal.Zero);
            if (NormalLeaveData == null  || NormalLeaveData.Count == 0 )
                return new Tuple<bool, string, decimal>(false, @"缺少請假資料，請重新整理！", decimal.Zero);
            var RelativeLeaveRequestGuid = new List<string>();
            var Dic = new Dictionary<string, decimal>();
            var RecoverHours = decimal.Zero;
            foreach (var NLD in NormalLeaveData)
            {
                var Guid = NLD["Guid"].ToString();
                RelativeLeaveRequestGuid.Add(NLD["Guid"].ToString());
                decimal.TryParse(NLD["Hours"].ToString(), out var Hours);
                RecoverHours += Hours;
                Dic.Add(Guid, Hours);
            }
            var saLeaveRequests = db.Queryable<OTB_EIP_LeaveRequest>()
                   .Where(it => it.OrgID == OrgID && it.MemberID == UserID && RelativeLeaveRequestGuid.Contains(it.guid)).ToList();
            if (saLeaveRequests.Count > 0)
            {
                foreach (OTB_EIP_LeaveRequest wz in saLeaveRequests)
                {
                    decimal.TryParse(Dic[wz.guid].ToString(), out var ReturnHours);
                    wz.UsedHours = wz.UsedHours - ReturnHours;
                    wz.RemainHours = wz.RemainHours + ReturnHours;

                }
                db.Updateable(saLeaveRequests).UpdateColumns(it => new { it.UsedHours, it.RemainHours }).ExecuteCommand();
            }
            return new Tuple<bool, string, decimal>(true, "", RecoverHours);
        }

    }
}