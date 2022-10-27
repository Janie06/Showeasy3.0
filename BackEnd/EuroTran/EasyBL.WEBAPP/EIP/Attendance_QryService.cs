using EasyBL.WebApi.Message;
using EasyBL;
using Entity;
using Entity.Sugar;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EasyBL.WEBAPP.EIP
{
    public class Attendance_QryService : ServiceBase
    {
        #region 查詢資料

        /// <summary>
        /// 查詢資料
        /// </summary>
        /// <param name="i_crm">參數</param>
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
                    var sUserIDs = _fetchString(i_crm, @"UserIDs");
                    var sDateStart = _fetchString(i_crm, @"DateStart");
                    var sDateEnd = _fetchString(i_crm, @"DateEnd");
                    if (!string.IsNullOrEmpty(sUserIDs))
                    {
                        sUserIDs = @"|" + sUserIDs.Replace(@",", @"|") + @"|";
                    }
                    var rDateStart = new DateTime();
                    var rDateEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sDateStart))
                    {
                        rDateStart = SqlFunc.ToDate(sDateStart);
                    }
                    if (!string.IsNullOrEmpty(sDateEnd))
                    {
                        rDateEnd = SqlFunc.ToDate(sDateEnd);
                    }

                    var saRoles = db.Queryable<OTB_SYS_MembersToRule>()
                        .Where(it => it.OrgID == i_crm.ORIGID && it.MemberID == i_crm.USERID).Select(it => it.RuleID).ToList();
                    var sRoles = JsonToString(saRoles);

                    //var sdb = new SimpleClient<OTB_EIP_Leave>(DBUnit.Instance);
                    pml.DataList = db.Queryable<OTB_EIP_Attendance>().OrderBy(sSortField, sSortOrder).Where(it => it.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sUserIDs), it => sUserIDs.Contains(@"|" + it.UserID + @"|"))
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && string.IsNullOrEmpty(sDateEnd), it => it.CardDate.Date >= rDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && string.IsNullOrEmpty(sDateStart), it => it.CardDate.Date <= rDateEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && !string.IsNullOrEmpty(sDateEnd), it => it.CardDate.Date >= rDateStart.Date && it.CardDate.Date <= rDateEnd.Date)
                        .Where(@" (UserID = @UserID
                              OR CHARINDEX('EipManager', @Roles)> 0
                              OR CHARINDEX('EipView', @Roles)> 0
                              OR CHARINDEX('Admin', @Roles)> 0)", new { UserID = i_crm.USERID, Roles = sRoles }).ToPageList(pml.PageIndex, pml.PageSize, ref iPageCount);
                    pml.Total = iPageCount;

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, pml);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Attendance_QryService), @"考勤查詢", @"QueryPage（查詢資料）", @"", @"", @"");
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

        #endregion 查詢資料

        #region 匯出資料

        /// <summary>
        /// 匯出資料
        /// </summary>
        /// <param name="i_crm">參數</param>
        /// <returns></returns>
        public ResponseMessage GetExcel(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sUserIDs = _fetchString(i_crm, @"UserIDs");
                    var sDateStart = _fetchString(i_crm, @"DateStart");
                    var sDateEnd = _fetchString(i_crm, @"DateEnd");
                    if (!string.IsNullOrEmpty(sUserIDs))
                    {
                        sUserIDs = @"|" + sUserIDs.Replace(@",", @"|") + @"|";
                    }
                    var rDateStart = new DateTime();
                    var rDateEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sDateStart))
                    {
                        rDateStart = SqlFunc.ToDate(sDateStart);
                    }
                    if (!string.IsNullOrEmpty(sDateEnd))
                    {
                        rDateEnd = SqlFunc.ToDate(sDateEnd);
                    }

                    var saRoles = db.Queryable<OTB_SYS_MembersToRule>()
                        .Where(it => it.OrgID == i_crm.ORIGID && it.MemberID == i_crm.USERID).Select(it => it.RuleID).ToList();
                    var sRoles = JsonToString(saRoles);

                    var saAttendance = db.Queryable<OTB_EIP_Attendance>().OrderBy(x => x.CardDate).Where(it => it.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sUserIDs), it => sUserIDs.Contains(@"|" + it.UserID + @"|"))
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && string.IsNullOrEmpty(sDateEnd), it => it.CardDate >= rDateStart)
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && string.IsNullOrEmpty(sDateStart), it => it.CardDate <= rDateEnd)
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && !string.IsNullOrEmpty(sDateEnd), it => it.CardDate >= rDateStart && it.CardDate <= rDateEnd)
                        .Where(@" (UserID = @UserID
                              OR CHARINDEX('EipManager', @Roles)> 0
                              OR CHARINDEX('EipView', @Roles)> 0
                              OR CHARINDEX('Admin', @Roles)> 0)", new { UserID = i_crm.USERID, Roles = sRoles }).ToList();
                    var saExcelData = new List<Dictionary<string, object>>();

                    var mStatus = new Map() { { @"B", @"審核中" }, { @"E", @"待經辦" }, { @"H-O", @"已經辦" } };
                    var saStatus = new string[] { @"B", @"E", @"H-O" };
                    var iIndex = 1;

                    var saLeave = db.Queryable<OTB_EIP_Leave>()
                        .Where(x => x.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sUserIDs), x => sUserIDs.Contains(@"|" + x.AskTheDummy + @"|"))
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && string.IsNullOrEmpty(sDateEnd), x => x.EndDate >= SqlFunc.ToDate(sDateStart))
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && string.IsNullOrEmpty(sDateStart), x => x.StartDate <= rDateEnd)
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && !string.IsNullOrEmpty(sDateEnd), x => x.EndDate >= SqlFunc.ToDate(sDateStart) && x.StartDate <= rDateEnd).ToList();
                    var saBusinessTravel = db.Queryable<OTB_EIP_BusinessTravel>()
                        .Where(x => x.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sUserIDs), x => sUserIDs.Contains(@"|" + x.AskTheDummy + @"|"))
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && string.IsNullOrEmpty(sDateEnd), x => x.EndDate >= SqlFunc.ToDate(sDateStart))
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && string.IsNullOrEmpty(sDateStart), x => x.StartDate <= rDateEnd)
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && !string.IsNullOrEmpty(sDateEnd), x => x.EndDate >= SqlFunc.ToDate(sDateStart) && x.StartDate <= rDateEnd).ToList();
                    var saAttendanceDiff = db.Queryable<OTB_EIP_AttendanceDiff>()
                        .Where(x => x.OrgID == i_crm.ORIGID)
                        .WhereIF(!string.IsNullOrEmpty(sUserIDs), x => sUserIDs.Contains(@"|" + x.AskTheDummy + @"|"))
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && string.IsNullOrEmpty(sDateEnd), x => x.FillBrushDate >= SqlFunc.ToDate(sDateStart))
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && string.IsNullOrEmpty(sDateStart), x => x.FillBrushDate <= rDateEnd)
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && !string.IsNullOrEmpty(sDateEnd), x => x.FillBrushDate >= SqlFunc.ToDate(sDateStart) && x.FillBrushDate <= rDateEnd).ToList();
                    foreach (OTB_EIP_Attendance attendance in saAttendance)
                    {
                        if (string.IsNullOrEmpty(attendance.SignIn))
                        {
                            attendance.SignIn = "------";
                        }
                        if (string.IsNullOrEmpty(attendance.SignOut))
                        {
                            attendance.SignOut = "------";
                        }
                        var dicItem = new Dictionary<string, object>
                        {
                            { @"RowIndex", iIndex },
                            { @"feild1", attendance.OrgID },
                            { @"feild2", attendance.CardUserName },
                            { @"feild3", Convert.ToDateTime(attendance.CardDate).ToString(@"yyyy/MM/dd") },
                            { @"feild4", attendance.TimeA != @"" ? attendance.TimeA + @" / " + attendance.SignIn.Substring(0, 5) : @"-----" },
                            { @"feild5", attendance.TimeP != @"" ? attendance.TimeP + @" / " + attendance.SignOut.Substring(0, 5) : @"-----" },
                            { @"feild6", attendance.Hours },
                            { @"feild7", attendance.Memo }
                        };
                        var oLeave = saLeave.Find(x => x.AskTheDummy == attendance.UserID && x.StartDate.Value.Date <= attendance.CardDate.Date && x.EndDate.Value.Date >= attendance.CardDate.Date && saStatus.Contains(x.Status));
                        dicItem.Add(@"feild8", oLeave == null ? @"" : mStatus[oLeave.Status].ToString());

                        var oBusinessTravel = saBusinessTravel.Find(x => x.AskTheDummy == attendance.UserID && x.StartDate.Value.Date <= attendance.CardDate.Date && x.EndDate.Value.Date >= attendance.CardDate.Date && saStatus.Contains(x.Status));
                        dicItem.Add(@"feild9", oBusinessTravel == null ? @"" : mStatus[oBusinessTravel.Status].ToString());

                        var oAttendanceDiff = saAttendanceDiff.Find(x => x.AskTheDummy == attendance.UserID && x.FillBrushDate.Value.Date == attendance.CardDate.Date && saStatus.Contains(x.Status));
                        dicItem.Add(@"feild10", oAttendanceDiff == null ? @"" : mStatus[oAttendanceDiff.Status].ToString());
                        saExcelData.Add(dicItem);
                        iIndex++;
                    }
                    const string sFileName = @"考勤記錄資料";
                    var dicHeader = new Dictionary<string, object>
                    {
                        { @"RowIndex", @"項次" },
                        { @"feild1", @"組織代號" },
                        { @"feild2", @"姓名" },
                        { @"feild3", @"日期" },
                        { @"feild4", @"上班/刷卡" },
                        { @"feild5", @"下班/刷卡" },
                        { @"feild6", @"時數" },
                        { @"feild7", @"備註" },
                        { @"feild8", @"請假" },
                        { @"feild9", @"出差" },
                        { @"feild10", @"加班" }
                    };
                    var bOk = new ExcelService().CreateExcel(saExcelData, out string sPath, dicHeader, sFileName);
                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, sPath);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Attendance_QryService), @"考勤查詢", @"GetExcel（匯出資料）", @"", @"", @"");
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

        #endregion 匯出資料

        #region 同步打卡資料

        /// <summary>
        /// 同步打卡資料
        /// </summary>
        /// <param name="i_crm">參數</param>
        /// <returns></returns>
        public ResponseMessage TransferEip(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            try
            {
                rm = SugarBase.ExecTran(db =>
                {
                    do
                    {
                        var sDateStart = _fetchString(i_crm, @"DateStart");
                        var sDateEnd = _fetchString(i_crm, @"DateEnd");
                        var bIsReSet = _fetchBool(i_crm, @"IsReSet");
                        var sWorkTimePMKey = Common.ConfigGetValue(@"", @"WorkTimePMKey");//上班開始時間（系統設定key名稱）
                        var sWorkTimeAMKey = Common.ConfigGetValue(@"", @"WorkTimeAMKey");//上班結束時間（系統設定key名稱）
                        var sAttendancePath = Common.ConfigGetValue(@"", @"AttendancePath");//打卡資料存放路徑
                        var sLatestShiftTimeKey = Common.ConfigGetValue("", "LatestShiftTimeKey");//工作時間（系統設定key名稱）
                        var sDelayBufferTimeKey = Common.ConfigGetValue("", "DelayBufferTimeKey");//工作時間遲到緩衝key值

                        var ConfigSettings = new List<string>() { sWorkTimePMKey, sWorkTimeAMKey, sLatestShiftTimeKey, sDelayBufferTimeKey };
                        var saDates = new List<string>();
                        var rDateStart = Convert.ToDateTime(sDateStart);
                        var rDateEnd = Convert.ToDateTime(sDateEnd);
                        var oWorkTimePM = db.Queryable<OTB_SYS_SystemSetting>().First(x => x.OrgID == i_crm.ORIGID && x.Effective == @"Y" && x.SettingItem == sWorkTimePMKey);
                        var oWorkTimeAM = db.Queryable<OTB_SYS_SystemSetting>().First(x => x.OrgID == i_crm.ORIGID && x.Effective == @"Y" && x.SettingItem == sWorkTimeAMKey);
                        var osLatestShiftTime = db.Queryable<OTB_SYS_SystemSetting>().First(x => x.OrgID == i_crm.ORIGID && x.Effective == @"Y" && x.SettingItem == sLatestShiftTimeKey);
                        var DelayBuffer = db.Queryable<OTB_SYS_SystemSetting>().First(x => x.OrgID == i_crm.ORIGID && x.Effective == @"Y" && x.SettingItem == sDelayBufferTimeKey);
                        
                        var AttendanceMethod = new AttendanceMethods();
                        var AttendanceRules = AttendanceMethod.GetAttendanceRule(db, ConfigSettings).Where( ar => ar.OrgID == i_crm.ORIGID).ToList();
                        if (bIsReSet)
                        {
                            var i = db.Deleteable<OTB_EIP_Attendance>()
                                       .Where(x => x.OrgID == i_crm.ORIGID && x.CardDate.Date >= rDateStart.Date && x.CardDate.Date <= rDateEnd.Date).ExecuteCommand();
                        }

                        while (rDateStart <= rDateEnd)
                        {
                            var sCurrentdayStr = rDateStart.ToString(@"yyyyMMdd");
                            var sCurrentPath = sAttendancePath + sCurrentdayStr + @".Txt";

                            if (File.Exists(sCurrentPath))
                            {
                                
                                var AllLine = File.ReadAllLines(sCurrentPath, Encoding.Default);
                                var SelectedData = AllLine.Where(d => d.Contains(i_crm.ORIGID)).ToArray();
                                var saAttendanceInfo = new List<OTB_EIP_Attendance>();
                                var CardDate = rDateStart.AddHours(9);
                                saAttendanceInfo = AttendanceMethod.CalculateAttendance(SelectedData, AttendanceRules, CardDate: CardDate);

                                var saMembers = db.Queryable<OTB_SYS_Members>().Where(it => it.Effective == @"Y" && it.OrgID == i_crm.ORIGID && it.IsAttendance == true).ToList();
                                var saAttendanceAdd = new List<OTB_EIP_Attendance>();
                                var saAttendanceUpd = new List<OTB_EIP_Attendance>();

                                //遍歷所有需要考勤的人，補全打卡信心用於初始化打卡資料
                                foreach (OTB_SYS_Members member in saMembers)
                                {
                                    var bAny = db.Queryable<OTB_EIP_Attendance>().Any(it => it.OrgID == member.OrgID && it.UserID == member.MemberID && SqlFunc.DateIsSame(it.CardDate, rDateStart));
                                    if (!bAny)
                                    {//判斷是否已經有了打卡資料
                                        if (saAttendanceInfo.Any(x => (x.CardId == member.CardId && member.OrgID == x.OrgID)))
                                        {
                                            var oAttendanceInfo = saAttendanceInfo.First(x => x.CardId == member.CardId && member.OrgID == x.OrgID);
                                            if (oAttendanceInfo.StatusA != null && oAttendanceInfo.StatusA == true)
                                            {
                                                oAttendanceInfo.Memo = "遲到";
                                            }
                                            if (oAttendanceInfo.StatusP != null && oAttendanceInfo.StatusP == true)
                                            {
                                                oAttendanceInfo.Memo = oAttendanceInfo.Memo == "" ? "早退" : oAttendanceInfo.Memo + "，早退";
                                            }
                                            if (string.IsNullOrEmpty(oAttendanceInfo.SignOut))
                                            {
                                                oAttendanceInfo.Memo = oAttendanceInfo.Memo == "" ? "(下班)未刷卡" : oAttendanceInfo.Memo + "，(下班)未刷卡";
                                            }
                                            oAttendanceInfo.UserID = member.MemberID;
                                            saAttendanceAdd.Add(oAttendanceInfo);
                                        }
                                        else
                                        {//需要打卡又沒有抓到打卡資料的人默認未打卡
                                            var DefaultCardDate = rDateStart.AddHours(14);
                                            var oAttendanceInfo = new OTB_EIP_Attendance
                                            {
                                                OrgID = member.OrgID,
                                                UserID = member.MemberID,
                                                CardDate = DefaultCardDate,
                                                CardId = member.CardId,
                                                TimeA = @"",
                                                TimeP = @"",
                                                CardUserName = member.MemberName,
                                                Hours = @"0",
                                                SignIn = @"",
                                                SignOut = @"",
                                                StatusP = true,
                                                StatusA = true,
                                                Memo = @"未刷卡",
                                                CreateDate = DateTime.Now
                                            };
                                            saAttendanceAdd.Add(oAttendanceInfo);
                                        }
                                    }
                                    else
                                    {
                                        var oAttendance_Old = db.Queryable<OTB_EIP_Attendance>().First(it => it.OrgID == member.OrgID && it.UserID == member.MemberID && SqlFunc.DateIsSame(it.CardDate, rDateStart));
                                        if (saAttendanceInfo.Any(x => (x.CardId == member.CardId)))
                                        {
                                            var oAttendanceInfo = saAttendanceInfo.First(x => x.CardId == member.CardId);
                                            if (oAttendanceInfo.StatusA != null && oAttendanceInfo.StatusA == true)
                                            {
                                                oAttendanceInfo.Memo = "遲到";
                                            }
                                            if (oAttendanceInfo.StatusP != null && oAttendanceInfo.StatusP == true)
                                            {
                                                oAttendanceInfo.Memo = oAttendanceInfo.Memo == "" ? "早退" : oAttendanceInfo.Memo + "，早退";
                                            }
                                            if (string.IsNullOrEmpty(oAttendanceInfo.SignOut))
                                            {
                                                oAttendanceInfo.Memo = oAttendanceInfo.Memo == "" ? "(下班)未刷卡" : oAttendanceInfo.Memo + "，(下班)未刷卡";
                                            }

                                            oAttendance_Old.TimeA = oAttendanceInfo.TimeA;
                                            oAttendance_Old.TimeP = oAttendanceInfo.TimeP;
                                            oAttendance_Old.Hours = oAttendanceInfo.Hours;
                                            oAttendance_Old.SignIn = oAttendanceInfo.SignIn;
                                            oAttendance_Old.SignOut = oAttendanceInfo.SignOut;
                                            oAttendance_Old.StatusP = oAttendanceInfo.StatusP;
                                            oAttendance_Old.StatusA = oAttendanceInfo.StatusA;
                                            oAttendance_Old.Memo = oAttendanceInfo.Memo;
                                            saAttendanceUpd.Add(oAttendance_Old);
                                        }
                                    }
                                }
                                if (saAttendanceAdd.Count > 0)
                                {
                                    db.Insertable(saAttendanceAdd).ExecuteCommand();

                                    //如果沒有打卡的話就產生未打卡提醒
                                    var saClockTips_Add = new List<OTB_SYS_ClockTips>();
                                    foreach (var attendance in saAttendanceAdd)
                                    {
                                        var sCurYear = attendance.CardDate.Year.ToString();
                                        var oHolidays = db.Queryable<OTB_SYS_Holidays>().Single(x => x.OrgID == attendance.OrgID && x.Year == sCurYear);
                                        if (attendance.SignIn == @"" && oHolidays.Holidays.IndexOf(attendance.CardDate.ToString(@"yyyy-MM-dd")) == -1)
                                        {
                                            if (CheckTips(db, attendance.CardDate, attendance.OrgID, attendance.UserID))
                                            {
                                                var oClockTips = new OTB_SYS_ClockTips
                                                {
                                                    OrgID = attendance.OrgID,
                                                    ParentID = attendance.CardDate.ToString(@"yyyyMMdd"),
                                                    Owner = attendance.UserID,
                                                    Type = nameof(attendance),
                                                    Title = attendance.CardDate.ToString(@"yyyy.MM.dd") + @"日沒有打卡，也沒有對應的行事曆和EIP資訊，請至EIP填寫對應的申請單",
                                                    Content = @"",
                                                    Url = @"",
                                                    CreateUser = @"",
                                                    TipsDate = attendance.CardDate,
                                                    CreateDate = DateTime.Now
                                                };
                                                saClockTips_Add.Add(oClockTips);
                                            }
                                        }
                                    }
                                    if (saClockTips_Add.Count > 0)
                                    {
                                        db.Insertable(saClockTips_Add).ExecuteCommand();
                                    }
                                }
                                if (saAttendanceUpd.Count > 0)
                                {
                                    db.Updateable(saAttendanceUpd).ExecuteCommand();
                                }
                            }
                            rDateStart = rDateStart.AddDays(1);
                        };

                        rm = new SuccessResponseMessage(null, i_crm);
                    } while (false);
                    return rm;
                });
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Attendance_QryService), @"考勤查詢", @"TransferEip（同步打卡資料）", @"", @"", @"");
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

        #endregion 同步打卡資料

        #region 抓去符合的請假單資料

        /// <summary>
        /// 抓去符合的請假單資料（用於匹配考勤記錄）
        /// </summary>
        /// <param name="i_crm">參數</param>
        /// <returns></returns>
        public ResponseMessage GetLeave(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sUserIDs = _fetchString(i_crm, @"UserIDs");
                    var sDateStart = _fetchString(i_crm, @"DateStart");
                    var sDateEnd = _fetchString(i_crm, @"DateEnd");

                    var rDateStart = new DateTime();
                    var rDateEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sDateStart))
                    {
                        rDateStart = SqlFunc.ToDate(sDateStart);
                    }
                    if (!string.IsNullOrEmpty(sDateEnd))
                    {
                        rDateEnd = SqlFunc.ToDate(sDateEnd);
                    }

                    var saStatus = new string[] { @"B", @"E", @"H-O" };
                    var saLeave = db.Queryable<OTB_EIP_Leave>().Where(it => saStatus.Contains(it.Status))
                        .WhereIF(!string.IsNullOrEmpty(sUserIDs), it => sUserIDs.Contains(it.AskTheDummy))
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && string.IsNullOrEmpty(sDateEnd), it => it.EndDate.Value.Date >= rDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && string.IsNullOrEmpty(sDateStart), it => it.StartDate.Value.Date <= rDateEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && !string.IsNullOrEmpty(sDateEnd), it => it.StartDate.Value.Date <= rDateEnd && it.EndDate.Value.Date >= rDateStart.Date).ToList();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saLeave);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Attendance_QryService), @"考勤查詢", @"GetLeave（抓去符合的請假單資料）", @"", @"", @"");
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

        #endregion 抓去符合的請假單資料

        #region 抓去符合的出差資料

        /// <summary>
        /// 抓去符合的出差資料（用於匹配考勤記錄）
        /// </summary>
        /// <param name="i_crm">參數</param>
        /// <returns></returns>
        public ResponseMessage GetBusTrip(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sUserIDs = _fetchString(i_crm, @"UserIDs");
                    var sDateStart = _fetchString(i_crm, @"DateStart");
                    var sDateEnd = _fetchString(i_crm, @"DateEnd");

                    var rDateStart = new DateTime();
                    var rDateEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sDateStart))
                    {
                        rDateStart = SqlFunc.ToDate(sDateStart);
                    }
                    if (!string.IsNullOrEmpty(sDateEnd))
                    {
                        rDateEnd = SqlFunc.ToDate(sDateEnd);
                    }

                    var saStatus = new string[] { @"B", @"E", @"H-O" };
                    var saBusTrip = db.Queryable<OTB_EIP_BusinessTravel>().Where(it => saStatus.Contains(it.Status))
                        .WhereIF(!string.IsNullOrEmpty(sUserIDs), it => sUserIDs.Contains(it.AskTheDummy))
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && string.IsNullOrEmpty(sDateEnd), it => it.EndDate.Value.Date >= rDateStart.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && string.IsNullOrEmpty(sDateStart), it => it.StartDate.Value.Date <= rDateEnd.Date)
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && !string.IsNullOrEmpty(sDateEnd), it => it.StartDate.Value.Date <= rDateEnd.Date && it.EndDate.Value.Date >= rDateStart.Date).ToList();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saBusTrip);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Attendance_QryService), @"考勤查詢", @"GetBusTrip（抓去符合的出差資料）", @"", @"", @"");
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

        #endregion 抓去符合的出差資料

        #region 抓去符合的加班資料

        /// <summary>
        /// 抓去符合的加班資料（用於匹配考勤記錄）
        /// </summary>
        /// <param name="i_crm">參數</param>
        /// <returns></returns>
        public ResponseMessage GetOvertime(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sUserIDs = _fetchString(i_crm, @"UserIDs");
                    var sDateStart = _fetchString(i_crm, @"DateStart");
                    var sDateEnd = _fetchString(i_crm, @"DateEnd");

                    var rDateStart = new DateTime();
                    var rDateEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sDateStart))
                    {
                        rDateStart = SqlFunc.ToDate(sDateStart);
                    }
                    if (!string.IsNullOrEmpty(sDateEnd))
                    {
                        rDateEnd = SqlFunc.ToDate(sDateEnd);
                    }

                    var saStatus = new string[] { @"B", @"E", @"H-O" };
                    var saOverTime = db.Queryable<OTB_EIP_OverTime>().Where(it => saStatus.Contains(it.Status)).ToList();
                    var saOvertimeList = new List<Map>();
                    foreach (OTB_EIP_OverTime overtime in saOverTime)
                    {
                        var jaOverTimes = (JArray)JsonConvert.DeserializeObject(overtime.OverTimes);
                        foreach (JObject jo in jaOverTimes)
                        {
                            var sEmployeeCode = jo[@"EmployeeCode"].ToString();
                            var sStart = jo[@"StartDate"].ToString();
                            var sEnd = jo[@"EndDate"].ToString();
                            var rStart = SqlFunc.ToDate(sStart).Date;
                            var rEnd = SqlFunc.ToDate(sEnd).Date;

                            if ((!string.IsNullOrEmpty(sUserIDs) && sUserIDs.Contains(sEmployeeCode) || string.IsNullOrEmpty(sUserIDs)) &&
                                ((!string.IsNullOrEmpty(sDateStart) && !string.IsNullOrEmpty(sDateEnd) && rStart <= rDateEnd.Date && rEnd >= rDateStart.Date) ||
                                (!string.IsNullOrEmpty(sDateStart) && string.IsNullOrEmpty(sDateEnd) && rEnd >= rDateStart.Date) ||
                                (string.IsNullOrEmpty(sDateStart) && !string.IsNullOrEmpty(sDateEnd) && rStart <= rDateEnd.Date) ||
                                (string.IsNullOrEmpty(sDateStart) && string.IsNullOrEmpty(sDateEnd))))
                            {
                                var map = new Map
                                {
                                    { @"Guid", overtime.Guid },
                                    { @"AskTheDummy", sEmployeeCode },
                                    { @"Status", overtime.Status },
                                    { @"StartDate", sStart },
                                    { @"EndDate", sEnd }
                                };
                                saOvertimeList.Add(map);
                            }
                        }
                    }

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saOvertimeList);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Attendance_QryService), @"考勤查詢", @"GetOvertime（抓去符合的加班資料）", @"", @"", @"");
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

        #endregion 抓去符合的加班資料

        #region 抓去符合的差勤異常資料

        /// <summary>
        /// 抓去符合的差勤異常資料（用於匹配考勤記錄）
        /// </summary>
        /// <param name="i_crm">參數</param>
        /// <returns></returns>
        public ResponseMessage GetAttendanceDiff(RequestMessage i_crm)
        {
            ResponseMessage rm = null;
            string sMsg = null;
            var db = SugarBase.GetIntance();
            try
            {
                do
                {
                    var sUserIDs = _fetchString(i_crm, @"UserIDs");
                    var sDateStart = _fetchString(i_crm, @"DateStart");
                    var sDateEnd = _fetchString(i_crm, @"DateEnd");

                    var rDateStart = new DateTime();
                    var rDateEnd = new DateTime();
                    if (!string.IsNullOrEmpty(sDateStart))
                    {
                        rDateStart = SqlFunc.ToDate(sDateStart);
                    }
                    if (!string.IsNullOrEmpty(sDateEnd))
                    {
                        rDateEnd = SqlFunc.ToDate(sDateEnd).AddDays(1);
                    }

                    var saStatus = new string[] { @"B", @"E", @"H-O" };
                    var saAttendanceDiff = db.Queryable<OTB_EIP_AttendanceDiff>().Where(it => saStatus.Contains(it.Status))
                        .WhereIF(!string.IsNullOrEmpty(sUserIDs), it => sUserIDs.Contains(it.AskTheDummy))
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && string.IsNullOrEmpty(sDateEnd), it => it.FillBrushDate >= rDateStart)
                        .WhereIF(!string.IsNullOrEmpty(sDateEnd) && string.IsNullOrEmpty(sDateStart), it => it.FillBrushDate <= rDateEnd)
                        .WhereIF(!string.IsNullOrEmpty(sDateStart) && !string.IsNullOrEmpty(sDateEnd), it => it.FillBrushDate <= rDateEnd && it.FillBrushDate >= rDateStart).ToList();

                    rm = new SuccessResponseMessage(null, i_crm);
                    rm.DATA.Add(BLWording.REL, saAttendanceDiff);
                } while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
                LogAndSendEmail(sMsg + @"Param：" + JsonToString(i_crm), ex, i_crm.ORIGID, i_crm.USERID, nameof(Attendance_QryService), @"考勤查詢", @"GetAttendanceDiff（抓去符合的差勤異常資料）", @"", @"", @"");
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

        #endregion 抓去符合的差勤異常資料

        /// <summary>
        /// 程序执行时间测试
        /// </summary>
        /// <param name="dateBegin">开始时间</param>
        /// <param name="dateEnd">结束时间</param>
        /// <returns>返回(秒)单位，比如: 0.00239秒</returns>
        public static double ExecDateDiff(DateTime dateBegin, DateTime dateEnd)
        {
            var ts1 = new TimeSpan(dateBegin.Ticks);
            var ts2 = new TimeSpan(dateEnd.Ticks);
            var ts3 = ts1.Subtract(ts2).Duration();
            //你想转的格式
            return ts3.TotalHours;
        }


        /// <summary>
        /// 檢核EIP申請資料與行事曆判斷是否需要填寫
        /// </summary>
        /// <param name="userId">结束时间</param>
        /// <param name="db">todo: describe db parameter on CheckTips</param>
        /// <param name="date">todo: describe date parameter on CheckTips</param>
        /// <param name="origId">todo: describe origId parameter on CheckTips</param>
        /// <returns>返回(秒)单位，比如: 0.00239秒</returns>
        private static bool CheckTips(SqlSugarClient db, DateTime date, string origId, string userId)
        {
            var bToTips = true;
            do
            {
                var iCount_Calendar = db.Queryable<OTB_SYS_Calendar>().Count(x => x.OrgID == origId && x.UserID == userId &&  !x.DelStatus && (x.StartDate.Date == date.Date || x.EndDate.Date == date.Date || (x.StartDate <= date && x.EndDate >= date)));
                if (iCount_Calendar > 0)
                {
                    bToTips = false;
                    break;
                }
                const string sStatus = @"B,E,H-O";
                //差勤異常
                var iCount_AttendanceDiff = db.Queryable<OTB_EIP_AttendanceDiff>().Count(x => x.OrgID == origId && x.AskTheDummy == userId && sStatus.Contains(x.Status) && x.FillBrushDate.Value.Date == date.Date)
;
                if (iCount_AttendanceDiff > 0)
                {
                    bToTips = false;
                    break;
                }
                //請假單
                var iCount_Leave = db.Queryable<OTB_EIP_Leave>().Count(x => x.OrgID == origId && x.AskTheDummy == userId && sStatus.Contains(x.Status) && (x.StartDate.Value.Date == date.Date || x.EndDate.Value.Date == date.Date || (x.StartDate <= date && x.EndDate >= date)));
                if (iCount_Leave > 0)
                {
                    bToTips = false;
                    break;
                }
            } while (false);
            return bToTips;
        }
    }
}