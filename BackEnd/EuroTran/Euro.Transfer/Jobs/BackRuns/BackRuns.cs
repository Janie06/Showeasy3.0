using Entity.Sugar;
using Euro.Transfer.Base;
using Newtonsoft.Json;
using SqlSugar.Base;
using System;
using System.Collections.Generic;

namespace Euro.Transfer.Jobs.BackRuns
{
    public class Job : ServiceTask
    {
        /// <summary>
        /// 任务开始
        /// </summary>
        protected override void Start()
        {
            try
            {
                var sCurrDate = DateTime.Now.ToString("MM-dd");
                var sCurrTime = DateTime.Now.ToString("HH:mm");
                var sIsBackRuns = Common.ConfigGetValue("", "IsBackRuns");//是否執行

                if ((sCurrDate == "01-01") || sIsBackRuns == "true")
                {
                    //运行中
                    this.mIsRunning = true;
                    //执行工作项
                    this.Run();
                }
            }
            catch (Exception error)
            {
                //异常日志
                ServiceTools.WriteLog(Errorlog_Path + @"BackRuns\" + DateTime.Now.ToString("yyyyMMdd"), error.ToString(), true);
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

        /// <summary>
        /// 执行請假設定默認設定
        /// </summary>
        protected void Run()
        {
            try
            {
                do
                {
                    var db = SugarBase.GetIntance();
                    var sCurrYear = DateTime.Now.ToString("yyyy");
                    var sRegisterOrgID = Common.ConfigGetValue("", "RegisterOrgs");
                    var saRegisterOrgs = sRegisterOrgID.Split(',');
                    var saLeaveSet = new List<OTB_EIP_LeaveSet>();
                    foreach (string org in saRegisterOrgs)
                    {
                        var saMembers = db.Queryable<OTB_SYS_Members>()
                            .Where(t => t.OrgID == org && t.Effective == "Y" && t.ServiceCode != "API").ToList();

                        var saArguments = db.Queryable<OTB_SYS_Arguments>()
                            .Where(t => t.OrgID == org && t.Effective == "Y" && t.ArgumentClassID == "LeaveType").ToList();

                        foreach (OTB_SYS_Members user in saMembers)
                        {
                            var iCount = db.Queryable<OTB_EIP_LeaveSet>().Count(t => t.OrgID == org && t.UserID == user.MemberID && t.TYear == sCurrYear);
                            if (iCount == 0)
                            {
                                var oLeaveSet = new OTB_EIP_LeaveSet
                                {
                                    Guid = Guid.NewGuid().ToString(),
                                    OrgID = org,
                                    UserID = user.MemberID,
                                    TYear = sCurrYear,
                                    CreateUser = nameof(Transfer),
                                    CreateDate = DateTime.Now
                                };
                                var dicSetInfo = new List<Dictionary<string, object>>();
                                foreach (OTB_SYS_Arguments arg in saArguments)
                                {
                                    var dicInfo = new Dictionary<string, object>
                                    {
                                        { "Id", arg.ArgumentID },
                                        { "Name", arg.ArgumentValue },
                                        { "PaymentHours", arg.Correlation ?? "" },
                                        { "UsedHours", "0" },
                                        { "RemainHours", arg.Correlation ?? "0" },
                                        { "Memo", arg.Memo }
                                    };
                                    dicSetInfo.Add(dicInfo);
                                }
                                oLeaveSet.SetInfo = JsonConvert.SerializeObject(dicSetInfo, Formatting.Indented);
                                saLeaveSet.Add(oLeaveSet);
                            }
                        }
                    }
                    if (saLeaveSet.Count > 0)
                    {
                        var iRel = db.Insertable<OTB_EIP_LeaveSet>(saLeaveSet).ExecuteCommand();
                    }
                } while (false);
            }
            catch (Exception error)
            {
                //写错误日志
                ServiceTools.WriteLog(Errorlog_Path + @"BackRuns\" + DateTime.Now.ToString("yyyyMMdd"), error.ToString(), true);
            }
        }
    }
}