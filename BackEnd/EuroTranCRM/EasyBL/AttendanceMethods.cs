using Entity.Sugar;
using SqlSugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyBL
{
    public class AttendanceMethods
    {

        /// <summary>
        /// 彈性工時計算
        /// </summary>
        /// <param name="attendanceRule"></param>
        /// <param name="SignIn"></param>
        /// <returns>依序:工作起、工作迄、是否遲到</returns>
        public Tuple<string, string, bool> GetFlexTime(AttendanceRule attendanceRule, string SignIn)
        {
            var UseStartTime = @""; //實際使用上班時間。
            var UseEndTime = @""; //實際使用上班時間。
            var AnyBuffer = true;
            var WorkDelay = false;
            if (string.IsNullOrWhiteSpace(attendanceRule.WorkStartBuffer))
            {
                AnyBuffer = false;
            }

            
            //檢查有無buffer
            if (AnyBuffer)
            {
                var SignInTime = Convert.ToDateTime(SignIn);
                var StandardWorkStart = Convert.ToDateTime(attendanceRule.WorkStart);
                var StandardWorkEnd = Convert.ToDateTime(attendanceRule.WorkEnd);
                var BufferedWorkStart = Convert.ToDateTime(attendanceRule.WorkStartBuffer + ":59");
                //-1: t1 早於 t2。 0: t1 與 t2 相同。 1: t1 晚於 t2。
                var CheckStandardThreadhold = DateTime.Compare(SignInTime, StandardWorkStart) > 0;
                if (CheckStandardThreadhold)
                {
                    var CheckBufferThreadhold = DateTime.Compare(SignInTime, BufferedWorkStart);
                    switch (CheckBufferThreadhold)
                    {
                        case 1:
                            WorkDelay = true;
                            var TotalBufferHour = ExecDateDiff(StandardWorkStart, BufferedWorkStart);
                            StandardWorkEnd = StandardWorkEnd.AddHours(TotalBufferHour);
                            if (attendanceRule.DelayBuffer > 0)
                            {
                                BufferedWorkStart = BufferedWorkStart.AddMinutes(-1 * attendanceRule.DelayBuffer);
                                StandardWorkEnd = StandardWorkEnd.AddMinutes(-1 * attendanceRule.DelayBuffer);
                            }
                            UseEndTime = StandardWorkEnd.ToString("HH:mm");
                            UseStartTime = BufferedWorkStart.ToString("HH:mm");

                            break;
                        case 0:
                        case -1:
                            var WorkStartBuffer = ExecDateDiff(StandardWorkStart, SignInTime);
                            WorkDelay = false;
                            UseEndTime = StandardWorkEnd.AddHours(WorkStartBuffer).ToString("HH:mm");
                            UseStartTime = StandardWorkStart.AddHours(WorkStartBuffer).ToString("HH:mm");
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    WorkDelay = false;
                    UseEndTime = attendanceRule.WorkEnd;
                    UseStartTime = attendanceRule.WorkStart;
                }
            }
            //無緩衝
            else
            {
                WorkDelay = DateTime.Compare(Convert.ToDateTime(SignIn), Convert.ToDateTime(attendanceRule.WorkStart)) > 0;
                UseStartTime = attendanceRule.WorkStart;
                UseEndTime = attendanceRule.WorkEnd;
                
            }
            return new Tuple<string, string,bool>(UseStartTime, UseEndTime, WorkDelay);
        }

        /// <summary>
        /// 計算工作時間。
        /// 必須SignOut、SignIn有時間，SignOut晚於SignIn、TimeA(工作時間起)和TimeP(工作時間迄)
        /// </summary>
        /// <param name="sSignTime"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public double GetWorkingHour(string sSignTime, OTB_EIP_Attendance info)
        {
            var WorkingHour = 0.0;
            var CheckAvaliableTime = !string.IsNullOrWhiteSpace(info.SignOut) && !string.IsNullOrWhiteSpace(info.SignIn);
            var HasDiffTime = ExecDateDiff(Convert.ToDateTime(info.SignIn), Convert.ToDateTime(info.SignOut)) > 0;
            var HasWorkTime = !string.IsNullOrWhiteSpace(info.TimeA) && !string.IsNullOrWhiteSpace(info.TimeP);
            if (CheckAvaliableTime && HasDiffTime && HasWorkTime)
            {
                var StandardWorkStart = Convert.ToDateTime(info.TimeA);
                var StandardWorkEnd = Convert.ToDateTime(info.TimeP);
                var SignInTime = Convert.ToDateTime(info.SignIn);
                if (SignInTime.Second > 0) //取最小量
                    SignInTime = SignInTime.AddSeconds(-1 * SignInTime.Second);
                var SignOutTime = Convert.ToDateTime(sSignTime);
                if (SignOutTime.Second > 0) //取最大量
                    SignOutTime = SignOutTime.AddSeconds(-1 * SignOutTime.Second);
                var ActualStartTime = new DateTime();
                var ActualEndTime = new DateTime();
                //實際認列開始工作時間。過早一律認列上班時間
                var WorkStartThreadhold = DateTime.Compare(SignInTime, StandardWorkStart);
                //實際認列結束工作時間。
                var WorkEndThreadhold = DateTime.Compare(SignOutTime, StandardWorkEnd);
                switch (WorkStartThreadhold)
                {
                    //刷進時間 晚於 變動上班時間
                    case 1:
                        ActualStartTime = SignInTime;
                        break;
                    //刷進時間 早於(等於) 變動上班時間
                    case 0:
                    case -1:
                        ActualStartTime = StandardWorkStart;
                        break;
                    default:
                        break;
                }
                switch (WorkEndThreadhold)
                {
                    case 1:
                    case 0:
                        ActualEndTime = StandardWorkEnd;
                        break;
                    case -1:
                        ActualEndTime = SignOutTime;
                        break;
                    default:
                        break;
                }
                //實際工作起 早於 實際工作迄(前面條件會變更)
                var TotalWorkTimeThreadhold = DateTime.Compare(ActualStartTime, ActualEndTime);
                switch (TotalWorkTimeThreadhold)
                {
                    case 1:
                    case 0:
                        WorkingHour = 0;
                        break;
                    case -1:
                        WorkingHour = Math.Round(ExecDateDiff(ActualStartTime, ActualEndTime), 2, MidpointRounding.AwayFromZero);
                        break;
                    default:
                        break;
                }

            }
            return WorkingHour;
        }

        /// <summary>
        /// 計算考勤
        /// </summary>
        /// <param name="AllLineDatas">卡中資料</param>
        /// <param name="AttendanceRules">出勤規則</param>
        /// <param name="CardDate">打卡日期</param>
        /// <returns></returns>
        public List<OTB_EIP_Attendance> CalculateAttendance(string[] AllLineDatas, List<AttendanceRule> AttendanceRules, DateTime CardDate)
        {
            var saCardAttendanceInfo = new List<OTB_EIP_Attendance>();

            //讀取打卡信息
            foreach (string str in AllLineDatas)
            {
                //打卡日期,卡號      , 刷卡時間,姓名     ,組織
                //20190701,0410164401,14:03:39,陳OO      ,TE
                var strInfo = str.Split(',');
                var sOrgID = strInfo[4].ToString().Trim();
                if (string.IsNullOrEmpty(sOrgID))
                {
                    continue;
                }
                var sCardDate = strInfo[0].ToString().Trim();
                var sCardId = strInfo[1].ToString().Trim();
                var sSignTime = strInfo[2].ToString().Trim();
                var sCardUserName = strInfo[3].ToString().Trim();
                var sTimeA = @"09:00"; //標準上班時間
                var sTimeP = @"17:30"; //標準下班時間
                var sTimeAE = "";  //允許最彈性下班時間
                var UseStartTime = @""; //實際使用上班時間。
                var UseEndTime = @""; //實際使用上班時間。

                var AnyBuffer = true;
                var ApplyRule = AttendanceRules.Where(ar => ar.OrgID == sOrgID).FirstOrDefault();
                if (ApplyRule != null)
                {
                    sTimeA = ApplyRule.WorkStart;
                    sTimeP = ApplyRule.WorkEnd;
                    sTimeAE = ApplyRule.WorkStartBuffer;
                }

                if (string.IsNullOrWhiteSpace(sTimeAE))
                {
                    AnyBuffer = false;
                    sTimeAE = sTimeP;
                }

                if (saCardAttendanceInfo.Any(x => x.CardId == sCardId))
                {
                    //如果存在則更新打卡信息
                    foreach (OTB_EIP_Attendance info in saCardAttendanceInfo)
                    {
                        if (info.CardId == sCardId)
                        {
                            info.SignOut = sSignTime;
                            var TotalWorkingHours = GetWorkingHour(sSignTime, info);
                            var bStatusP = DateTime.Compare(Convert.ToDateTime(info.TimeP), Convert.ToDateTime(sSignTime)) > 0;
                            info.Hours = TotalWorkingHours.ToString();
                            info.StatusP = bStatusP;
                            break;
                        }
                    }
                }
                //不存在則新增
                else
                {
                    var FlexTime = GetFlexTime(ApplyRule, sSignTime);
                    var oAttendanceInfo = new OTB_EIP_Attendance
                    {
                        OrgID = sOrgID,
                        CardDate = CardDate,
                        CardId = sCardId,
                        TimeA = FlexTime.Item1,
                        TimeP = FlexTime.Item2,
                        CardUserName = sCardUserName,
                        Hours = "0",
                        SignIn = sSignTime,
                        SignOut = ""
                    };

                    oAttendanceInfo.StatusA = FlexTime.Item3;
                    oAttendanceInfo.StatusP = false;
                    oAttendanceInfo.Memo = "";
                    oAttendanceInfo.CreateDate = DateTime.Now;
                    saCardAttendanceInfo.Add(oAttendanceInfo);
                }
            }

            return saCardAttendanceInfo;
        }

        /// <summary>
        /// 取得考勤規則
        /// </summary>
        /// <param name="db"></param>
        /// <param name="ConfigSettings">至少要4個</param>
        /// <returns></returns>
        public List<AttendanceRule> GetAttendanceRule(SqlSugarClient db, List<string> ConfigSettings)
        {
            var AttendanceRule = new List<AttendanceRule>();
            try
            {
                do
                {
                    //WebConfig依序:上班開始時間、上班結束時間、最晚上班時間(工作緩衝時間)、遲到緩衝時間(分)
                    //var ConfigSettings = new List<string>() { Common.ConfigGetValue("", "WorkTimePMKey"), Common.ConfigGetValue("", "WorkTimeAMKey")
                    //    ,Common.ConfigGetValue("", "LatestShiftTimeKey"), Common.ConfigGetValue("", "DelayBufferTimeKey") };
                    var AllOrgIDs = db.Queryable<OTB_SYS_Organization>().Select(it => it.OrgID).ToList().Distinct();

                    if(!ConfigSettings.Any())
                    {
                        ConfigSettings = new List<string> { "WorkTimePM", "WorkTimeAM", "LatestShiftTime", "DelayBufferTime" };
                    }

                    var spWorkTimePMKey = new SugarParameter("@WorkTimePM", ConfigSettings[0]);
                    var spDWorkTimeAMKey = new SugarParameter("@WorkTimeAM", ConfigSettings[1]);
                    var spLatestShiftTimeKey = new SugarParameter("@LatestShiftTime", ConfigSettings[2]);
                    var spDelayBufferKey = new SugarParameter("@DelayBuffer", ConfigSettings[3]);
                    var AllOfSystemSettings = db.Ado.SqlQuery<OTB_SYS_SystemSetting>(@"select *from OTB_SYS_SystemSetting where Effective ='Y' 
                    and SettingItem in (@WorkTimePM, @WorkTimeAM, @LatestShiftTime, @DelayBuffer )", spWorkTimePMKey, spDWorkTimeAMKey, spLatestShiftTimeKey, spDelayBufferKey).ToArray();
                    //
                    foreach (var OrgID in AllOrgIDs)
                    {
                        var attendanceRule = new AttendanceRule() { OrgID = OrgID };
                        foreach (var Key in ConfigSettings)
                        {
                            var MatchedRule = AllOfSystemSettings.Where(s => s.OrgID == OrgID && s.SettingItem == Key).FirstOrDefault();
                            if (MatchedRule != null && !string.IsNullOrWhiteSpace(MatchedRule.SettingValue))
                            {
                                var SplittedTime = MatchedRule.SettingValue.Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries);
                                switch (Key)
                                {
                                    case "WorkTimePM":
                                        attendanceRule.WorkStart = SplittedTime[0].PadLeft(5, '0');//開始時間
                                        attendanceRule.WorkTimePMDeadLine = MatchedRule.SettingValue;
                                        break;
                                    case "WorkTimeAM":
                                        attendanceRule.WorkEnd = SplittedTime[1].PadLeft(5, '0');//結束時間
                                        attendanceRule.WorkTimeAMDeadLine = MatchedRule.SettingValue;
                                        break;
                                    case "LatestShiftTime":
                                        attendanceRule.WorkStartBuffer = SplittedTime[0].PadLeft(5, '0');
                                        break;
                                    case "DelayBufferTime":
                                        var DelayBuffer = 0;
                                        var CheckIntValue = int.TryParse(MatchedRule.SettingValue, out DelayBuffer);
                                        attendanceRule.DelayBuffer = DelayBuffer;
                                        break;
                                    default:
                                        break;
                                }
                            }

                        }
                        AttendanceRule.Add(attendanceRule);
                    }

                } while (false);
            }
            catch (Exception error)
            {
                throw error;
            }
            return AttendanceRule;
        }

        /// <summary>
        /// 程序执行时间测试
        /// </summary>
        /// <param name="dateBegin">开始时间</param>
        /// <param name="dateEnd">结束时间</param>
        /// <returns>返回(秒)单位，比如: 0.00239秒</returns>
        private double ExecDateDiff(DateTime dateBegin, DateTime dateEnd)
        {
            var ts1 = new TimeSpan(dateBegin.Ticks);
            var ts2 = new TimeSpan(dateEnd.Ticks);
            var ts3 = ts1.Subtract(ts2).Duration();
            //你想转的格式
            return ts3.TotalHours;
        }
    }

    
}
