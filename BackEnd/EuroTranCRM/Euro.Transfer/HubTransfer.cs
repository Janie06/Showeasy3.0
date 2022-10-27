using Microsoft.AspNet.SignalR.Client;
using Euro.Transfer.Model;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using Euro.Transfer.Base;
using System.Threading;

namespace Euro.Transfer
{
    public class HubTransfer : ServiceBase
    {
        private string transferOrgID = Common.ConfigGetValue("", "TransferOrgID");
        private string transferUserID = Common.ConfigGetValue("", "TransferUserID");

        public delegate void WriteOrLogsHandler(string text, int count);

        public event WriteOrLogsHandler writeOrLogs;

        public List<UserInfo> OnlineUsers = new List<UserInfo>(); // 在线用户列表

        public IHubProxy msgProxy;
        public HubConnection connection;
        public string clientOrgId;
        public string clientId;
        public string clientName;

        public HubTransfer()
        {
            clientOrgId = transferOrgID;
            clientId = transferUserID;
            clientName = "奕達小助手";
        }

        public async Task RunAsync(string url)
        {
            try
            {
                connection = new HubConnection(url);
                //connection.TraceWriter = _traceWriter;

                msgProxy = connection.CreateHubProxy("msgHub");

                msgProxy.On<string, string, List<UserInfo>>("onConnected", (connnectId, username, allUsers) =>
                {
                    OnlineUsers = allUsers;
                });

                msgProxy.On<string, List<UserInfo>, string, string, string, bool>("onUserDisconnected", (connnectId, allUsers, orgid, userid, username, islogin) =>
                 {
                     //var user = OnlineUsers.FirstOrDefault(u => u.ConnectionId == connnectId);
                     //// 判断用户是否存在,存在则删除
                     //if (user != null)
                     //{
                     //    OnlineUsers.Remove(user);
                     //}
                     OnlineUsers = allUsers;

                     if (!islogin && orgid == clientOrgId && userid == clientId)
                     {
                         Thread.Sleep(10000); //延时10秒
                         connection.Start().ContinueWith(t =>
                         {
                             if (!t.IsFaulted)
                             {
                                 //连接成功，调用Register方法
                                 msgProxy.Invoke("Register", clientOrgId, clientId, clientName, true);
                             }
                             else
                             {
                                 //MessageBox.Show("通訊連接失敗！！ 請檢查Tracking後臺系統是否正常運行");
                                 ServiceTools.WriteLog(ServiceBase.Errorlog_Path, "Euro.Transfer.HubTransfer:通訊連接失敗！！ 請檢查Tracking後臺系統是否正常運行", true);
                             }
                         });
                     }
                 });

                msgProxy.On<List<UserInfo>>("onlineusers", (allUsers) =>
                {
                    OnlineUsers = allUsers;
                });

                msgProxy.On("transfertips", () =>
                {
                    msgProxy.Invoke("Register", clientOrgId, clientId, clientName, true);
                    var sIsTest = Common.ConfigGetValue("", "IsTest");//是否為測試
                    if (sIsTest == "true")
                    {
                        ServiceTools.WriteLog(Debuglog_Path, "HubTransfer.transfertips：" + clientOrgId + "-" + clientId + " " + connection.ConnectionId, true);
                    }
                });

                //客户端接收实现，可以用js，也可以用后端接收
                var pushtransfer = msgProxy.On<string, string, string, int>("pushtransfer", (orgid, userid, data, index) =>
                {
                    switch (index)
                    {
                        case 1:
                            TransferService.TransferBill(writeOrLogs, orgid, userid, data);
                            break;

                        case 2:
                            TransferService.TransferCus(writeOrLogs, orgid, userid, data);
                            break;

                        case 3:
                            TransferService.TransferPrj(writeOrLogs, orgid, userid, data);
                            break;

                        default:
                            break;
                    }
                    msgProxy.Invoke("transferBack", data, index);
                });

                await connection.Start().ContinueWith(t =>
                {
                    if (!t.IsFaulted)
                    {
                        //连接成功，调用Register方法
                        //msgProxy.Invoke("Register", clientOrgId, clientId, clientName,true);
                        msgProxy.Invoke("GetOnlineUsers");
                    }
                    else
                    {
                        //MessageBox.Show("通訊連接失敗！！ 請檢查Tracking後臺系統是否正常運行");
                        ServiceTools.WriteLog(ServiceBase.Errorlog_Path, "Euro.Transfer.HubTransfer:通訊連接失敗！！ 請檢查Tracking後臺系統是否正常運行", true);
                    }
                });
                //await msgProxy.Invoke("Hello", "Hello World!");
            }
            catch (Exception ex)
            {
                ServiceTools.WriteLog(ServiceBase.Errorlog_Path, ex.ToString(), true);
            }
        }
    }
}