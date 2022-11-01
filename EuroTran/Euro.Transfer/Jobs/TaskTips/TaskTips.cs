using Entity.Sugar;
using Euro.Transfer.Base;
using Euro.Transfer.Model;
using SqlSugar;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using EasyBL;
using System.Net.Mail;

namespace Euro.Transfer.Jobs.TaskTips
{
    enum JobType
    {
        TaskTips, RunEIPTips, RunAttendanceLists, RunCreateAttendanceTips, RunClearFiles, CheckAnnualleaveAndMail
    }
    public class Job : ServiceTask
    {

        /// <summary>
        /// 任务开始
        /// </summary>
        protected override void Start()
        {
            try
            {
                //运行中
                this.mIsRunning = true;
                var db = SugarBase.GetIntance();
                var sCurrTime = DateTime.Now.ToString("HH:mm");
                var sTaskTipsTimeID = Common.ConfigGetValue("", "TaskTipsTimeID");
                var sEIPTipsTimeID = Common.ConfigGetValue("", "EIPTipsTimeID");
                var sAttendanceTime = Common.ConfigGetValue("", "ReadAttendanceTime");
                var sCreateAttendanceTips = Common.ConfigGetValue("", "CreateAttendanceTipsTime");
                var sClearFilesTime = Common.ConfigGetValue("", "ClearFilesTime");
                var CheckAnnualleave = Common.ConfigGetValue("", "ReadWenZhongAnnualleave");
                var saSetTask = db.Queryable<OTB_SYS_SystemSetting>().Where(it => it.Effective == "Y" && it.SettingItem == sTaskTipsTimeID).ToList();

                if (saSetTask.Count > 0)
                {
                    foreach (OTB_SYS_SystemSetting set in saSetTask)
                    {
                        if (set.SettingValue.IndexOf(sCurrTime) > -1)
                        {
                            //执行工作项
                            this.RunTaskTips(set.OrgID);
                        }
                    }
                }

                var saSetEIP = db.Queryable<OTB_SYS_SystemSetting>().Where(it => it.Effective == "Y" && it.SettingItem == sEIPTipsTimeID).ToList();
                if (saSetEIP.Count > 0)
                {
                    foreach (OTB_SYS_SystemSetting set in saSetEIP)
                    {
                        if (set.SettingValue.IndexOf(sCurrTime) > -1)
                        {
                            //执行工作项
                            this.RunEIPTips(set.OrgID);
                        }
                    }
                }

                if (sAttendanceTime.IndexOf(sCurrTime) > -1)
                {
                    WriteTaskLog("Start:" + JobType.RunAttendanceLists.ToString(), JobType.RunAttendanceLists);//記錄所有log
                    //讀取卡鐘打卡資料
                    this.RunAttendanceLists();
                    WriteTaskLog("End:" + JobType.RunAttendanceLists.ToString(), JobType.RunAttendanceLists);//記錄所有log
                }

                if (sCurrTime == sCreateAttendanceTips)
                {
                    //產生未打卡定時提醒資料
                    this.RunCreateAttendanceTips();
                }
                if (sCurrTime == sClearFilesTime)
                {

                    //执行清理廢除文件工作项
                    this.RunClearFiles();
                }

                if (sCurrTime == CheckAnnualleave)
                {
                    WriteTaskLog("Start:" + JobType.CheckAnnualleaveAndMail.ToString(), JobType.CheckAnnualleaveAndMail);
                    //檢查特休
                    CheckAnnualleaveAndMail();
                    WriteTaskLog("End:" + JobType.CheckAnnualleaveAndMail.ToString(), JobType.CheckAnnualleaveAndMail);
                }
            }
            catch (Exception error)
            {
                ErrorLog(error);
            }
            finally
            {
                //空闲
                this.mIsRunning = false;
            }
        }

        /// <summary>
        /// 任务停止
        /// </summary>
        protected override void Stop()
        {
            this.mIsRunning = false;
        }

        #region 执行代辦提醒邏輯

        /// <summary>
        /// 执行代辦提醒邏輯
        /// </summary>
        /// <param name="sOrgID">todo: describe sOrgID parameter on RunTaskTips</param>
        protected void RunTaskTips(string sOrgID)
        {
            try
            {
                do
                {
                    var db = SugarBase.GetIntance();
                    var saTasks = db.Queryable<OTB_SYS_Task, OTB_SYS_Members>((t, m) => t.OrgID == m.OrgID && t.CreateUser == m.MemberID)
                        .Select((t, m) => new OTB_SYS_Task
                        {
                            OrgID = t.OrgID,
                            EventID = t.EventID,
                            EventName = t.EventName,
                            Owner = t.Owner,
                            StartDate = t.StartDate,
                            EndDate = t.EndDate,
                            Important = SqlFunc.IIF(t.Important == "M", "普通", "重要"),
                            CreateUser = m.MemberName,
                            TaskDescription = t.TaskDescription,
                            SourceFrom = t.SourceFrom,
                            Params = t.Params
                        })
                            .Where(t => t.OrgID == sOrgID &&
                            ((t.Status == "U" && t.AlertTime == null) || (t.Status != "O" && t.Status != "D" && t.AlertTime >= DateTime.Now))).ToList();

                    if (saTasks.Count > 0)
                    {
                        var listMessages = new List<Message>();
                        foreach (OTB_SYS_Task task in saTasks)
                        {
                            var msg = new Message
                            {
                                Type = MessageType.SendToUser,
                                Memo = "tips",
                                ToOrgId = task.OrgID,
                                ToUserId = task.Owner
                            };
                            if (!listMessages.Any(x => (x.ToOrgId == msg.ToOrgId && x.ToUserId == msg.ToUserId)))
                            {
                                listMessages.Add(msg);
                            }
                        }
                        foreach (Message message in listMessages)
                        {
                            if (hubClient.OnlineUsers.Any(x => (x.OrgId == message.ToOrgId && x.UserId == message.ToUserId)))
                            {
                                var listTaskInfos = new List<TaskInfo>();
                                foreach (OTB_SYS_Task _task in saTasks)
                                {
                                    var sOrgId = _task.OrgID;
                                    var sUserId = _task.Owner;
                                    if (message.ToOrgId == sOrgId && message.ToUserId == sUserId)
                                    {
                                        var task = new TaskInfo
                                        {
                                            OrgID = sOrgId,
                                            Owner = sUserId,
                                            StartDate = _task.StartDate,
                                            EndDate = _task.EndDate == null ? _task.StartDate : _task.EndDate,
                                            EventID = _task.EventID,
                                            EventName = _task.EventName,
                                            Important = _task.Important,
                                            CreateUser = _task.CreateUser,
                                            SourceFrom = _task.SourceFrom,
                                            Params = _task.Params
                                        };
                                        listTaskInfos.Add(task);
                                    }
                                }
                                message.Content = ServiceBase.JsonToString(listTaskInfos);
                                hubClient.msgProxy.Invoke("Send", message);
                            }
                        }
                    }
                } while (false);
            }
            catch (Exception error)
            {
                ErrorLog(error);
            }
        }

        #endregion 执行代辦提醒邏輯

        #region 执行考勤未打卡提醒邏輯

        /// <summary>
        /// 执行考勤未打卡提醒邏輯
        /// </summary>
        /// <param name="sOrgID">todo: describe sOrgID parameter on RunEIPTips</param>
        protected void RunEIPTips(string sOrgID)
        {
            try
            {
                do
                {
                    var db = SugarBase.GetIntance();

                    var rNow = DateTime.Now;
                    var rStart = DateTime.Now;
                    var rEnd = DateTime.Now;
                    if (rNow.Day <= 5)
                    {
                        rStart = rStart.AddMonths(-1);
                        rStart = new DateTime(rStart.Year, rStart.Month, 1);
                    }
                    else
                    {
                        rStart = new DateTime(rNow.Year, rNow.Month, 1);
                    }

                    var saClockTips = db.Queryable<OTB_SYS_ClockTips>().Where(x => x.OrgID == sOrgID && x.TipsDate >= rStart && x.TipsDate <= rEnd).ToList();

                    if (saClockTips.Count > 0)
                    {
                        var listMessages = new List<Message>();
                        foreach (OTB_SYS_ClockTips tips in saClockTips)
                        {
                            var msg = new Message
                            {
                                Type = MessageType.SendToUser,
                                Memo = "attendance",
                                ToOrgId = tips.OrgID,
                                ToUserId = tips.Owner
                            };
                            if (!listMessages.Any(x => (x.ToOrgId == msg.ToOrgId && x.ToUserId == msg.ToUserId)))
                            {
                                listMessages.Add(msg);
                            }
                        }
                        foreach (Message message in listMessages)
                        {
                            if (hubClient.OnlineUsers.Any(x => (x.OrgId == message.ToOrgId && x.UserId == message.ToUserId)))
                            {
                                var listClockTips = new List<ClockTipsInfo>();
                                foreach (OTB_SYS_ClockTips _tips in saClockTips)
                                {
                                    var sOrgId = _tips.OrgID;
                                    var sUserId = _tips.Owner;
                                    if (message.ToOrgId == sOrgId && message.ToUserId == sUserId)
                                    {
                                        var tips = new ClockTipsInfo
                                        {
                                            OrgID = sOrgId,
                                            Owner = sUserId,
                                            NO = _tips.NO,
                                            Type = _tips.Type,
                                            Title = _tips.Title,
                                            Content = _tips.Content,
                                            Url = _tips.Url,
                                            CreateUser = _tips.CreateUser,
                                            CreateDate = _tips.CreateDate,
                                            Parm = ""
                                        };
                                        listClockTips.Add(tips);
                                    }
                                }
                                message.Content = ServiceBase.JsonToString(listClockTips);
                                hubClient.msgProxy.Invoke("Send", message);
                            }
                        }
                    }
                } while (false);
            }
            catch (Exception error)
            {
                ErrorLog(error);
            }
        }

        #endregion 执行考勤未打卡提醒邏輯

        #region 轉入文字檔案

        /// <summary>
        /// 轉入文字檔案
        /// </summary>
        protected void RunAttendanceLists()
        {
            try
            {
                do
                {
                    var db = SugarBase.GetIntance();
                    var ConfigSettings = new List<string>() { Common.ConfigGetValue("", "WorkTimePMKey"), Common.ConfigGetValue("", "WorkTimeAMKey")
                        ,Common.ConfigGetValue("", "LatestShiftTimeKey"), Common.ConfigGetValue("", "DelayBufferTimeKey") };
                    var sAttendanceFilePath = Common.ConfigGetValue("", "AttendancePath");//打卡資料存放路徑
                    var AttendanceMethod = new AttendanceMethods();
                    var AttendanceRules = AttendanceMethod.GetAttendanceRule(db, ConfigSettings);
                    //取得昨天、今日打卡名稱。
                    var LoadingDates = new DateTime[] { DateTime.Now.Date.AddDays(-1), DateTime.Now.Date };
                    var DeleteStartDate = LoadingDates[0].Date;
                    var DeleteEndDate = LoadingDates[1].Date.AddSeconds(24 * 60 * 60 - 1);


                    var i = db.Deleteable<OTB_EIP_Attendance>().Where(x => x.CardDate.Date >= DeleteStartDate && x.CardDate.Date <= DeleteEndDate).ExecuteCommand();
                    WriteTaskLog("已刪除:" + i.ToString() + "筆，從" + DeleteStartDate + "到" + DeleteEndDate, JobType.RunAttendanceLists);
                    var saAttendanceAdd = new List<OTB_EIP_Attendance>();
                    foreach (var date in LoadingDates)
                    {

                        var CurrentDay = date.ToString("yyyyMMdd");
                        var LoadingFilesPath = sAttendanceFilePath + CurrentDay + ".Txt";
                        if (File.Exists(LoadingFilesPath))
                        {
                            var AllLineDatas = File.ReadAllLines(LoadingFilesPath, Encoding.Default).Distinct().ToArray();
                            //固定時間為13點。
                            var CardDate = date.AddHours(13);
                            var saCardAttendanceInfo = AttendanceMethod.CalculateAttendance(AllLineDatas, AttendanceRules, CardDate).OrderBy(c => c.CardId).ToList();
                            var saMembers = db.Queryable<OTB_SYS_Members>().Where(it => it.Effective == "Y" && it.IsAttendance == true).ToList();

                            //遍歷所有需要考勤的人，補全打卡信息用於初始化打卡資料
                            foreach (OTB_SYS_Members member in saMembers)
                            {
                                if (saCardAttendanceInfo.Any(x => (x.CardId == member.CardId && member.OrgID == x.OrgID)))
                                {
                                    var oAttendanceInfo = saCardAttendanceInfo.First(x => x.CardId == member.CardId && member.OrgID == x.OrgID);
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
                                {
                                    //需要打卡又沒有抓到打卡資料的人默認未打卡
                                    var oAttendanceInfo = new OTB_EIP_Attendance
                                    {
                                        OrgID = member.OrgID,
                                        UserID = member.MemberID,
                                        CardDate = CardDate,
                                        CardId = member.CardId,
                                        TimeA = "",
                                        TimeP = "",
                                        CardUserName = member.MemberName,
                                        Hours = "0",
                                        SignIn = "",
                                        SignOut = "",
                                        StatusP = true,
                                        StatusA = true,
                                        Memo = "未刷卡",
                                        CreateDate = DateTime.Now
                                    };
                                    saAttendanceAdd.Add(oAttendanceInfo);
                                }
                            }
                        }
                    }
                    if (saAttendanceAdd.Count > 0)
                    {
                        db.Insertable(saAttendanceAdd).ExecuteCommand();
                        WriteTaskLog("已新增:" + saAttendanceAdd.Count + "筆，從" + LoadingDates[0] + " 到 " + LoadingDates[1], JobType.RunAttendanceLists);
                    }

                } while (false);
            }
            catch (Exception error)
            {
                ErrorLog(error);
            }
        }


        #endregion 轉入文字檔案

        #region 执行清理後台產生的廢除文件

        /// <summary>
        /// 执行清理後台產生的廢除文件
        /// </summary>
        protected void RunClearFiles()
        {
            try
            {
                do
                {
                    var sClearFilesPath = Common.ConfigGetValue("", "ClearFilesPath");
                    var rCurDate = DateTime.Now.AddDays(-7);
                    if (Directory.Exists(sClearFilesPath))
                    {
                        var filePathArr = Directory.GetFiles(sClearFilesPath);
                        var fileCreateDate = new Dictionary<string, DateTime>();
                        foreach (string file in filePathArr)
                        {
                            var fi = new FileInfo(file);
                            fileCreateDate[file] = fi.CreationTime;
                        }
                        fileCreateDate = fileCreateDate.OrderBy(f => f.Value).ToDictionary(f => f.Key, f => f.Value);
                        foreach (KeyValuePair<string, DateTime> item in fileCreateDate)
                        {
                            if (item.Value <= rCurDate && File.Exists(item.Key))
                            {
                                File.Delete(item.Key);
                            }
                        }
                    }
                } while (false);
            }
            catch (Exception error)
            {
                ErrorLog(error);
            }
        }

        #endregion 执行清理後台產生的廢除文件

        #region 產生未打卡定時提醒資料

        /// <summary>
        /// 產生未打卡定時提醒資料
        /// </summary>
        protected void RunCreateAttendanceTips()
        {
            try
            {
                do
                {
                    var db = SugarBase.GetIntance();
                    var sOrigID = Common.ConfigGetValue("", "TransferOrgID");
                    var rNow = DateTime.Now;
                    var rCurDate = rNow.AddDays(-1);
                    var saClockTips_Add = new List<OTB_SYS_ClockTips>();
                    var saAttendance = db.Queryable<OTB_EIP_Attendance>().Where(x => x.OrgID == sOrigID && x.CardDate.Date == rCurDate.Date).ToList();

                    var sCurYear = rCurDate.Year.ToString();
                    var oHolidays = db.Queryable<OTB_SYS_Holidays>().Single(x => x.OrgID == sOrigID && x.Year == sCurYear);

                    foreach (var attendance in saAttendance)
                    {
                        if (attendance.SignIn == "" && oHolidays.Holidays.IndexOf(rCurDate.ToString("yyyy-MM-dd")) == -1)
                        {
                            if (CheckTips(db, rCurDate, attendance.OrgID, attendance.UserID))
                            {
                                var oClockTips = new OTB_SYS_ClockTips
                                {
                                    OrgID = attendance.OrgID,
                                    ParentID = rCurDate.ToString("yyyyMMdd"),
                                    Owner = attendance.UserID,
                                    Type = nameof(attendance),
                                    Title = rCurDate.ToString("yyyy.MM.dd") + "日沒有打卡，也沒有對應的行事曆和EIP資訊，請至EIP填寫對應的申請單",
                                    Content = "",
                                    Url = "",
                                    CreateUser = "",
                                    TipsDate = rCurDate,
                                    CreateDate = rNow
                                };
                                saClockTips_Add.Add(oClockTips);
                            }
                        }
                    }

                    if (saClockTips_Add.Count > 0)
                    {
                        db.Insertable(saClockTips_Add).ExecuteCommand();
                    }
                } while (false);
            }
            catch (Exception error)
            {
                ErrorLog(error);
            }
        }

        #endregion 產生未打卡定時提醒資料

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
                var iCount_Calendar = db.Queryable<OTB_SYS_Calendar>().Count(x => x.OrgID == origId && x.UserID == userId && !x.DelStatus && (x.StartDate.Date == date.Date || x.EndDate.Date == date.Date || (x.StartDate <= date && x.EndDate >= date)));
                if (iCount_Calendar > 0)
                {
                    bToTips = false;
                    break;
                }
                var sStatus = "B,E,H-O";
                //差勤異常
                var iCount_AttendanceDiff = db.Queryable<OTB_EIP_AttendanceDiff>()
                    .Count(x => x.OrgID == origId && x.AskTheDummy == userId && sStatus.Contains(x.Status) && x.FillBrushDate.Value.Date == date.Date);
                if (iCount_AttendanceDiff > 0)
                {
                    bToTips = false;
                    break;
                }
                //請假單
                var iCount_Leave = db.Queryable<OTB_EIP_Leave>()
                    .Count(x => x.OrgID == origId && x.AskTheDummy == userId && sStatus.Contains(x.Status) && (x.StartDate.Value.Date == date.Date || x.EndDate.Value.Date == date.Date || (x.StartDate <= date && x.EndDate >= date)));
                if (iCount_Leave > 0)
                {
                    bToTips = false;
                    break;
                }
            } while (false);
            return bToTips;
        }

        /// <summary>
        /// 程序执行时间测试
        /// </summary>
        /// <param name="dateBegin">开始时间</param>
        /// <param name="dateEnd">结束时间</param>
        /// <returns>返回(秒)单位，比如: 0.00239秒</returns>
        private static double ExecDateDiff(DateTime dateBegin, DateTime dateEnd)
        {
            var ts1 = new TimeSpan(dateBegin.Ticks);
            var ts2 = new TimeSpan(dateEnd.Ticks);
            var ts3 = ts1.Subtract(ts2).Duration();
            //你想转的格式
            return ts3.TotalHours;
        }

        /// <summary>
        /// 檢查是否在時間區間內
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="StartDT"></param>
        /// <param name="EndDT"></param>
        /// <returns></returns>
        public static bool CheckDateRange(DateTime dateTime, DateTime StartDT, DateTime EndDT)
        {
            var DataTicks = dateTime.Ticks;
            var StartTicks = StartDT.Ticks;
            var EndTicks = EndDT.Ticks;
            if (DataTicks >= StartTicks && DataTicks <= EndTicks)
                return true;
            else
                return false;
        }



        

        /// <summary>
        /// 特休提醒
        /// </summary>
        public static void CheckAnnualleaveAndMail()
        {
            var Job = JobType.CheckAnnualleaveAndMail;
            var TitleTemp = "＜系統自動發信＞{UserName}特休假即將失效通知";
            var ContentTemp = "您於{EnableDate}-{ExpirationDate}剩餘的特休假即將在{ExpirationDate}失效，如需遞延最晚請於失效日前1個月向管理部提出書面申請，逾期未申請屆期將直接換算薪資發放。";
            var ReplaceKeys = new { EnableDate = "{EnableDate}", ExpiredDate = "{ExpirationDate}", UserName = "{UserName}" };
            var db = SugarBase.DB;
            try
            {
                do
                {
                    var StartDT = DateTime.Now.Date;
                    var EndDT = StartDT.AddDays(45);

                    var AvailableAnnualleaves = db.Queryable<OTB_EIP_WenZhong>()
                        .Where(it => it.DelFlag == false && it.Notice == false && it.RemainHours > 0 && it.ExpirationDate.HasValue).ToList();
                    var ToBeExpiredAL = AvailableAnnualleaves.Where(c => CheckDateRange(c.ExpirationDate.Value, StartDT, EndDT)).OrderBy(c => c.OrgID).ToList();
                    if (ToBeExpiredAL.Count > 0)
                    {
                        var UpdateWenZhongs = new List<OTB_EIP_WenZhong>();
                        var Users = db.Queryable<OTB_SYS_Members>().Where(it => it.Effective == "Y").ToList();

                        var mailServices = new List<MailService>();
                        var AllOrgIDs = ToBeExpiredAL.Select(c => c.OrgID).ToList().Distinct();
                        foreach (var OrgID in AllOrgIDs)
                        {
                            mailServices.Add(new MailService(OrgID, false));
                        }

                        string LogMsg = @"";
                        foreach (var Al in ToBeExpiredAL)
                        {
                            var FoundUser = Users.FirstOrDefault(c => c.Effective == "Y" && c.MemberID == Al.UserID && c.OrgID == Al.OrgID);
                            if (FoundUser != null)
                            {
                                var OrgID = FoundUser.OrgID;
                                var MailTitle = TitleTemp.Replace(ReplaceKeys.UserName, FoundUser.MemberName);
                                var MailBody = ContentTemp.Replace(ReplaceKeys.EnableDate, Al.EnableDate.Value.ToString("yyyy/MM/dd"))
                                    .Replace(ReplaceKeys.ExpiredDate, Al.ExpirationDate.Value.ToString("yyyy/MM/dd"));

                                using (MailMessage mail = new MailMessage())
                                {
                                    var UseStmpEmailServer = mailServices.Where(c => c.OrgID == FoundUser.OrgID).First();
                                    mail.To.Add(FoundUser.Email);
                                    mail.From = new MailAddress(UseStmpEmailServer.mailSetting["FromEmail"].ToString(), UseStmpEmailServer.mailSetting["FromName"].ToString(), System.Text.Encoding.UTF8);
                                    mail.Subject = MailTitle;
                                    mail.SubjectEncoding = Encoding.UTF8;//郵件標題編碼
                                    mail.Body = MailBody; //郵件內容
                                    mail.BodyEncoding = Encoding.UTF8;//郵件內容編碼 
                                    mail.IsBodyHtml = true;//是否是HTML郵件 

                                    var bSend = UseStmpEmailServer.SendMailNET_NoSelf(mail);
                                    if (bSend)
                                    {
                                        LogMsg += "寄送特休通知給: " + Al.UserName + "，特休期間從" + Al.EnableDate.Value.ToString("yyyy/MM/dd") + " 到 " + Al.ExpirationDate.Value.ToString("yyyy/MM/dd") + Environment.NewLine;
                                        Al.Notice = true;
                                        UpdateWenZhongs.Add(Al);
                                    }
                                    else
                                    {
                                        LogMsg += UseStmpEmailServer.ErrorMessages;
                                    }
                                }
                            }

                        }
                        if (LogMsg.Length > 0)
                        {
                            WriteTaskLog(LogMsg, Job);
                        }
                        if (UpdateWenZhongs.Count > 0)
                        {
                            db.Updateable(UpdateWenZhongs).UpdateColumns(it => new { it.Notice }).ExecuteCommand();
                        }
                    }
                } while (false);
            }
            catch (Exception error)
            {
                ErrorLog(error);
            }
        }

        /// <summary>
        /// 輸出錯誤資訊
        /// </summary>
        /// <param name="error"></param>
        private static void ErrorLog(Exception error)
        {
            ServiceTools.WriteLog("", "Error:" + error.ToString() + "StackTrace:" + Environment.NewLine + error.StackTrace, true);
        }

        private static void WriteTaskLog(string cont, JobType job)
        {
            var FolderName = job.ToString();
            var path = System.Windows.Forms.Application.StartupPath.ToString() + @"\Tasks\" + FolderName + "\\";

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            path += DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            using (StreamWriter sw = new StreamWriter(path, true, Encoding.UTF8))
            {
                sw.WriteLine(DateTime.Now);
                sw.WriteLine(cont);
                sw.WriteLine("");
                sw.Close();
            }
        }
    }
}