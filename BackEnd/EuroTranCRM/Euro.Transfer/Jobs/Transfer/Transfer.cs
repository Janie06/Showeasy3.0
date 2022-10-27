using Euro.Transfer.Base;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Euro.Transfer.Jobs.Transfer
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
                var sCurrTime = DateTime.Now.ToString("HH:mm");
                var sCurrWeekDay = DateTime.Now.DayOfWeek.ToString("d");
                var sTransferWeeks = Common.ConfigGetValue("", "TransferWeeks");
                var sTransferTime = Common.ConfigGetValue("", "TransferTime");
                var sIsAuto = Common.ConfigGetValue("", "IsAuto");
                //ServiceTools.WriteLog("", "sCurrTime:" + sCurrTime + "sCurrWeekDay:" + sCurrWeekDay + "sTransferWeeks:" + sTransferWeeks + "sTransferTime:" + sTransferTime, true);
                if (sIsAuto == "true" || (sTransferWeeks.IndexOf(sCurrWeekDay) > -1 && sCurrTime == sTransferTime))
                {
                    //运行中
                    this.mIsRunning = true;
                    //执行工作项
                    this.RunTransfer();
                }
            }
            catch (Exception error)
            {
                //异常日志
                ServiceTools.WriteLog(Errorlog_Path + @"Transfer\" + DateTime.Now.ToString("yyyyMMdd"), error.ToString(), true);
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
        /// 执行代辦提醒邏輯
        /// </summary>
        protected void RunTransfer()
        {
            try
            {
                do
                {
                    var sOrgID = Common.ConfigGetValue("", "TransferOrgID");
                    var sUserID = Common.ConfigGetValue("", "TransferUserID");
                    var user = hubClient.OnlineUsers.FirstOrDefault(u => (u.OrgId == sOrgID && u.UserId == sUserID));
                    if (user != null)
                    {
                        var sIsTest = Common.ConfigGetValue("", "IsTest");//是否為測試
                        if (sIsTest == "true")
                        {
                            ServiceTools.WriteLog(Debuglog_Path, "Euro.Transfer.Jobs.Transfer：" + user.OrgId + "-" + user.UserId + " " + hubClient.connection.State + "   ConnectionId：" + hubClient.connection.ConnectionId, true);
                        }
                        if (hubClient.connection.State == Microsoft.AspNet.SignalR.Client.ConnectionState.Connected)
                        {
                            var sRegisterOrgID = Common.ConfigGetValue("", "RegisterOrgs");
                            var saRegisterOrgs = sRegisterOrgID.Split(',');
                            foreach (string org in saRegisterOrgs)
                            {
                                hubClient.msgProxy.Invoke("pushTransfer", org, user.UserId, "", 1);
                                hubClient.msgProxy.Invoke("pushTransfer", org, user.UserId, "", 2);
                                hubClient.msgProxy.Invoke("pushTransfer", org, user.UserId, "", 3);
                            }
                        }
                        else
                        {
                            hubClient.connection.Start().ContinueWith(t =>
                            {
                                if (!t.IsFaulted)
                                {
                                    //连接成功，调用Register方法
                                    hubClient.msgProxy.Invoke("Register", hubClient.clientOrgId, hubClient.clientId, hubClient.clientName, true);
                                }
                                else
                                {
                                    MessageBox.Show("通訊連接失敗！！ 請檢查Tracking後臺系統是否正常運行");
                                }
                            });
                        }
                    }
                    else
                    {
                        hubClient.connection.Start().ContinueWith(t =>
                        {
                            if (!t.IsFaulted)
                            {
                                //连接成功，调用Register方法
                                hubClient.msgProxy.Invoke("Register", hubClient.clientOrgId, hubClient.clientId, hubClient.clientName, true);
                            }
                            else
                            {
                                MessageBox.Show("通訊連接失敗！！ 請檢查Tracking後臺系統是否正常運行");
                            }
                        });
                    }
                } while (false);
            }
            catch (Exception error)
            {
                //写错误日志
                ServiceTools.WriteLog(Errorlog_Path + @"Transfer\" + DateTime.Now.ToString("yyyyMMdd"), error.ToString(), true);
            }
        }
    }
}