using EasyBL;
using Entity.Sugar;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Newtonsoft.Json;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApp.Models;
using WebApp.Models.Hub;

namespace WebApp.Hubs
{
    [HubName("msgHub")]
    public class MsgHub : Hub
    {
        public static Online Euro_Online = new Online(); // 在線用户列表

        public class Online
        {
            public Online()
            {
                Users = new List<UserInfo>(); //在線用户列表
                Rooms = new List<RoomInfo>(); //房間（群組）
            }

            //用戶集合
            public List<UserInfo> Users { get; set; }

            //房間集合
            public List<RoomInfo> Rooms { get; set; }
        }

        /// <summary>
        /// 连线时调用
        /// </summary>
        /// <returns></returns>
        public override Task OnConnected()
        {
            //Console.WriteLine("客戶端連接，連接ID是:{0},當前在綫人數為{1}", Context.ConnectionId, Euro_Online.Users.Count + 1);
            return base.OnConnected();
        }

        /// <summary>
        /// 断线时调用
        /// </summary>
        /// <param name="stopCalled"></param>
        /// <returns></returns>
        public override Task OnDisconnected(bool stopCalled)
        {
            var user = Euro_Online.Users.Find(u => u.ConnectionId == Context.ConnectionId);

            // 判断用户是否存在,存在則刪除
            if (user == null)
            {
                return base.OnDisconnected(stopCalled);
            }
            // 刪除用戶
            Euro_Online.Users.Remove(user);
            var onlineUsers = Euro_Online.Users.Where(x => x.OrgId == user.OrgId).ToList();
            var sIds = HubsTool.GetConnectionIdsByOrgID(user.OrgId);
            Clients.Clients(sIds).onUserDisconnected(user.ConnectionId, onlineUsers, user.OrgId, user.UserId, user.UserName, false);//調用客戶端用戶離線通知
            var sTransferOrgID = Common.ConfigGetValue("", "TransferOrgID");
            var sTransferUerID = Common.ConfigGetValue("", "TransferUerID");
            var transfer = Euro_Online.Users.Find(u => (u.OrgId == sTransferOrgID && u.UserId == sTransferUerID));
            if (transfer != null)
            {
                Clients.Client(transfer.ConnectionId).onUserDisconnected(user.ConnectionId, Euro_Online.Users, user.OrgId, user.UserId, user.UserName, false);
            }
            //Console.WriteLine("客戶端斷線，連接ID是:{0},當前在綫人數為{1}", Context.ConnectionId, OnlineUsers.Count);
            try
            {
                var db = SugarBase.DB;
                var iEf = db.Deleteable<OTB_SYS_OnlineUsers>()
                    .Where(it => it.OrgID == user.OrgId && it.UserID == user.UserId).ExecuteCommand();
            }
            catch (Exception)
            {
                throw;
            }
            return base.OnDisconnected(stopCalled);
        }

        public override Task OnReconnected()
        {
            return base.OnReconnected();
        }

        /// <summary>
        /// 登入連線
        /// </summary>
        /// <param name="orgid">組織Id</param>
        /// <param name="userid"></param>
        /// <param name="username"></param>
        /// <param name="islogin"></param>
        public void Register(string orgid, string userid, string username, bool islogin)
        {
            var connnectId = Context.ConnectionId;
            var db = SugarBase.GetIntance();

            if (Euro_Online.Users.Count(x => x.ConnectionId == connnectId) == 0)
            {
                if (Euro_Online.Users.Any(x => (x.UserId == userid /*&& x.OrgId == orgid*/)))
                {
                    var item = Euro_Online.Users.First(x => (x.UserId == userid /*&& x.OrgId == orgid*/));
                    var oTicket = db.Queryable<OTB_SYS_TicketAuth>().First(it => it.UserID == userid /*&& it.OrgID == orgid*/);
                    if (oTicket != null)
                    {
                        // 20180311 修改帳號登入不同單位自動登出
                        //var users = Euro_Online.Users.Where(x => x.OrgId == orgid).ToList();
                        var users = Euro_Online.Users.ToList();
                        //var _sIds = HubsTool.GetConnectionIdsByOrgID(orgid);
                        var _sIds = Euro_Online.Users.Select(p => p.ConnectionId).ToList();
                        Clients.Clients(_sIds).onUserDisconnected(item.ConnectionId, users, item.OrgId, item.UserId, item.UserName, islogin, "IP：" + oTicket.LoginIp + "<br>DATETIME：" + oTicket.LoginTime);
                    }
                    Euro_Online.Users.RemoveAll(x => (x.UserId == userid && x.OrgId == orgid));
                }
                var Info = new UserInfo
                {
                    ConnectionId = connnectId,
                    OrgId = orgid,
                    UserId = userid,
                    UserName = username,
                    LastLoginTime = DateTime.Now
                };

                //string sWSUrl = EasyBL.Common.GetAppSettings("WebServiceUrl");
                //string[] Params = new string[1];
                //Params[0] = "gstrUserId";
                //string sParams = JsonConvert.SerializeObject(Params, Formatting.Indented); //把list轉成Json字串
                //string sValue = (string)EasyBL.WebServiceUtil.InvokeWebService(sWSUrl, "GetSession", new object[] { Params });
                //Dictionary<string, object> dic_Session = JsonConvert.DeserializeObject<Dictionary<string, object>>(sValue);
                //if (dic_Session["gstrUserId"] != null && dic_Session["gstrUserId"].ToString() != "")
                //{
                //    //添加在線人员
                //    Euro_Online.Users.Add(Info);
                //}

                //添加在線人员
                Euro_Online.Users.Add(Info);
                //CreateRoom(usertype);
                //AddRoom(usertype);
                //Groups.Add(connnectId, usertype);
                try
                {
                    var oNlineUser = new OTB_SYS_OnlineUsers
                    {
                        OrgID = orgid,
                        UserID = userid
                    };
                    var saOnlineUser = db.Queryable<OTB_SYS_OnlineUsers>().Where(it => it.OrgID == orgid && it.UserID == userid).ToList();
                    if (saOnlineUser.Count == 0)
                    {
                        var iEf = db.Insertable(oNlineUser).ExecuteCommand();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }

            // 所有客戶端同步在線用户
            var onlineUsers = Euro_Online.Users.Where(x => x.OrgId == orgid).ToList();
            var sIds = HubsTool.GetConnectionIdsByOrgID(orgid);
            Clients.Clients(sIds).onConnected(connnectId, username, onlineUsers);
            var sTransferOrgID = Common.ConfigGetValue("", "TransferOrgID");
            var sTransferUerID = Common.ConfigGetValue("", "TransferUerID");
            var transfer = Euro_Online.Users.Find(u => (u.OrgId == sTransferOrgID && u.UserId == sTransferUerID));
            if (transfer != null)
            {
                Clients.Client(transfer.ConnectionId).onConnected(connnectId, username, Euro_Online.Users);
            }
        }

        /// <summary>
        /// 登录连线
        /// </summary>
        /// <param name="orgid">組織Id</param>
        /// <param name="userId">用户Id</param>
        /// <param name="userName">用户名</param>
        public void Offline()
        {
            var user = Euro_Online.Users.Find(u => u.ConnectionId == Context.ConnectionId);

            // 判断用户是否存在,存在則刪除
            if (user != null)
            {
                // 删除用户
                var sIds = HubsTool.GetConnectionIdsByOrgID(user.OrgId);
                Euro_Online.Users.Remove(user);
                var onlineUsers = Euro_Online.Users.Where(x => x.OrgId == user.OrgId).ToList();
                Clients.Clients(sIds).onUserDisconnected(user.ConnectionId, onlineUsers, user.OrgId, user.UserId, user.UserName, true);//調用客戶端用戶離線通知
                var sTransferOrgID = Common.ConfigGetValue("", "TransferOrgID");
                var sTransferUerID = Common.ConfigGetValue("", "TransferUerID");
                var transfer = Euro_Online.Users.Find(u => (u.OrgId == sTransferOrgID && u.UserId == sTransferUerID));
                if (transfer != null)
                {
                    Clients.Client(transfer.ConnectionId).onUserDisconnected(user.ConnectionId, Euro_Online.Users, user.OrgId, user.UserId, user.UserName, true);
                }
                {
                    var db = SugarBase.DB;
                    var iEf = db.Deleteable<OTB_SYS_OnlineUsers>()
                        .Where(it => it.OrgID == user.OrgId && it.UserID == user.UserId).ExecuteCommand();
                }
            }
        }

        /// <summary>
        /// 加入群組
        /// </summary>
        /// <param name="groupName">組名稱</param>
        /// <param name="connectionId">當前連線ID</param>
        public void JoinGroup(string groupName, string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                connectionId = Context.ConnectionId;
            }

            Groups.Add(connectionId, groupName);
            Clients.All.hubMessage(connectionId + " joined group " + groupName);
        }

        public void LeaveGroup(string groupName, string connectionId)
        {
            if (string.IsNullOrEmpty(connectionId))
            {
                connectionId = Context.ConnectionId;
            }

            Groups.Remove(connectionId, groupName);
            Clients.All.hubMessage(connectionId + " left group " + groupName);
        }

        //加入聊天室
        public void AddRoom(string roomName)
        {
            //查询聊天室
            var room = Euro_Online.Rooms.Find(a => a.RoomName == roomName);
            //存在則加入
            if (room != null)
            {
                //查找房間中是否存在此用户
                var isUser = room.Users.Find(w => w.ConnectionId == Context.ConnectionId);
                //不存在則加入
                if (isUser == null)
                {
                    var user = Euro_Online.Users.Find(a => a.ConnectionId == Context.ConnectionId);
                    user.Rooms.Add(room);
                    room.Users.Add(user);
                    Groups.Add(Context.ConnectionId, roomName);
                    //註冊加入聊天室的addRoom方法
                    Clients.Client(Context.ConnectionId).addRoom(roomName);
                }
                else
                {
                    Clients.Client(Context.ConnectionId).showMessage("请勿重复加入房间");
                }
            }
        }

        /// <summary>
        /// 更新所有用户的房间列表
        /// </summary>
        private void GetRooms()
        {
            var rooms = JsonConvert.SerializeObject(Euro_Online.Rooms.Select(p => p.RoomName).ToList());
            Clients.All.getRoomList(rooms);
        }

        /// <summary>
        /// 创建聊天室
        /// </summary>
        /// <param name="roomName"></param>
        public void CreateRoom(string roomName)
        {
            var room = Euro_Online.Rooms.Find(a => a.RoomName == roomName);
            if (room == null)
            {
                var r = new RoomInfo() { RoomName = roomName };
                //将房间加入列表
                Euro_Online.Rooms.Add(r);
                AddRoom(roomName);
                Clients.Client(Context.ConnectionId).showMessage("房间创建完成");
                GetRooms();
            }
            else
            {
                Clients.Client(Context.ConnectionId).showMessage("房间名重复");
            }
        }

        /// <summary>
        /// 退出聊天室
        /// </summary>
        /// <param name="roomName"></param>
        public void ExitRoom(string roomName)
        {
            //查找房间是否存在
            var room = Euro_Online.Rooms.Find(a => a.RoomName == roomName);
            //存在则删除
            if (room != null)
            {
                //查找要删除的用户
                var user = room.Users.Find(p => p.ConnectionId == Context.ConnectionId);
                //移除此用户
                room.Users.Remove(user);
                //如果房间人数为0，怎删除房间
                if (room.Users.Count == 0)
                {
                    Euro_Online.Rooms.Remove(room);
                }
                //Groups Remove移除分组方法
                Groups.Remove(Context.ConnectionId, roomName);
                //提示客户端
                Clients.Client(Context.ConnectionId).removeRoom("退出成功");
            }
        }

        /// <summary>
        /// 發送給自己
        /// </summary>
        /// <param name="value"></param>
        public void SendToMe(string value)
        {
            Clients.Caller.hubMessage(value);
        }

        /// <summary>
        /// 檢核當前頁面是否有人在操作
        /// </summary>
        /// <param name="prgid"></param>
        /// <param name="dataid"></param>
        public void CheckEdit(string prgid, string dataid)
        {
            var connnectId = Context.ConnectionId;
            var bEdit = true;
            var sUserName = "";

            foreach (UserInfo user in Euro_Online.Users)
            {
                if (user.ConnectionId != connnectId && user.Prgs.Count(p => p.FnId == prgid && p.DataId == dataid) > 0)
                {
                    sUserName = user.UserName;
                    bEdit = false;
                    break;
                }
            }

            if (bEdit && Euro_Online.Users.Count(x => x.ConnectionId == connnectId) > 0)
            {
                foreach (UserInfo user in Euro_Online.Users)
                {
                    if (user.ConnectionId == connnectId && user.Prgs.Count(p => p.FnId == prgid && p.DataId == dataid) == 0)
                    {
                        var prg = new EditPrgInfo
                        {
                            FnId = prgid,
                            DataId = dataid
                        };
                        user.Prgs.Add(prg);
                        break;
                    }
                }
            }

            Clients.Caller.checkedit(bEdit, prgid, sUserName);
        }

        /// <summary>
        /// 檢核當前頁面是否有人在操作
        /// </summary>
        /// <param name="prgid"></param>
        public void RemoveEditPrg(string prgid)
        {
            var connnectId = Context.ConnectionId;
            foreach (UserInfo user in Euro_Online.Users)
            {
                if (user.ConnectionId == connnectId)
                {
                    var prg = user.Prgs.Find(p => p.FnId == prgid);
                    if (prg != null)
                    {
                        user.Prgs.Remove(prg);
                        break;
                    }
                }
            }
            //Clients.Caller.checkEdit(bEdit);
        }

        /// <summary>
        /// 發送給指定人
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="value"></param>
        public void SendToUser(string userId, string value)
        {
            Clients.User(userId).message(value);
        }

        public void SendToUsers(IList<string> userIds, string value)
        {
            Clients.Users(userIds).message(value);
        }

        public void SendToConnectionId(string connectionId, string value)
        {
            Clients.Client(connectionId).hubMessage(value);
        }

        public void SendToAll(string value)
        {
            Clients.All.hubMessage(value);
        }

        public void SendToGroup(string groupName, string value)
        {
            Clients.Group(groupName).hubMessage(value);
        }

        public void Broadcast(OTB_SYS_Announcement msg)
        {
            //string sConnectionId = Context.ConnectionId;
            Clients.All.broadcast(msg); // 向所有客户端廣播
        }

        public void Send(Message msg)
        {
            switch (msg.Type)
            {
                case MessageType.SendToMe:
                    {
                        Clients.Caller.receive(msg);
                        break;
                    }
                case MessageType.SendToConnectionId:
                    {
                        Clients.Client(msg.ConnectionId).receive(msg); // 特定的客户端，这个方法也就是我们实现端对端聊天的关键
                        break;
                    }
                case MessageType.SendToUser:
                    {
                        var sId = HubsTool.GetConnectionId(msg.ToOrgId, msg.ToUserId);
                        if (sId != "")
                        {
                            Clients.Client(sId).receive(msg); // 特定的客户端，这个方法也就是我们实现端对端聊天的关键
                        }
                        break;
                    }
                case MessageType.SendToUsers:

                    {
                        var sIds = HubsTool.GetConnectionIds(msg.ToUserIds);
                        if (sIds.Count > 0)
                        {
                            Clients.Clients(sIds).receive(msg); // 特定的客户端，这个方法也就是我们实现端对端聊天的关键
                        }
                        break;
                    }
                case MessageType.SendBroadcast:
                    {
                        Clients.All.Broadcast(msg);
                        break;
                    }
                case MessageType.SendToGroup:
                    {
                        Clients.Group(msg.GroupName).receive(msg); // 指定客户端组，这个也是实现群聊的关键所在
                        break;
                    }
                case MessageType.Throw:
                    {
                        throw new InvalidOperationException("Client does not receive this error");
                    }                    //break;

                default:
                    {
                        break;
                    }
            }
        }

        public void PushTip(string orgId, string userId)
        {
            var sId = HubsTool.GetConnectionId(orgId, userId);
            if (sId != "")
            {
                Clients.Client(sId).pushtips();
            }
        }

        public void PushTips(IList<string> userIds)
        {
            var sIds = HubsTool.GetConnectionIds(userIds);
            if (sIds.Count > 0)
            {
                Clients.Clients(sIds).pushtips();
            }
        }

        public void PushMsg(string orgId, string userId)
        {
            var sId = HubsTool.GetConnectionId(orgId, userId);
            if (sId != "")
            {
                Clients.Client(sId).pushmsgs();
            }
        }

        public void PushMsgs(IList<string> userIds)
        {
            var sIds = HubsTool.GetConnectionIds(userIds);
            if (sIds.Count > 0)
            {
                Clients.Clients(sIds).pushmsgs();
            }
        }

        public void PushTransfer(string orgId, string userId, string data, int index)
        {
            var sTransferOrgID = Common.ConfigGetValue("", "TransferOrgID");
            var sTransferUerID = Common.ConfigGetValue("", "TransferUerID");
            var sId = HubsTool.GetConnectionId(sTransferOrgID, sTransferUerID);
            if (sId != "")
            {
                var sTransferData = "";

                switch (index)
                {
                    case 1://抓取要拋轉的賬單資料
                        {
                            sTransferData = MsgHubService.GetBillsString(orgId, data);
                            break;
                        }
                    case 2://抓取要拋轉的客戶資料
                        {
                            sTransferData = MsgHubService.GetCustomersString(orgId, data);
                            break;
                        }
                    case 3://抓取要拋轉的專案資料
                        {
                            sTransferData = MsgHubService.GetExhibitionsString(orgId, data);
                            break;
                        }

                    default:
                        {
                            break;
                        }
                }

                if (sTransferData != "[]")
                {
                    Clients.Client(sId).pushtransfer(orgId, userId, sTransferData, index);
                }
            }
            else
            {
                Clients.Caller.transfertips();
            }
        }

        public void TransferBack(string data, int index)
        {
            switch (index)
            {
                case 1://賬單移除
                    {
                        MsgHubService.RemoveBills(data);
                        break;
                    }
                case 2://客戶移除
                    {
                        MsgHubService.RemoveCustomers(data);
                        break;
                    }
                case 3://專案移除
                    {
                        MsgHubService.RemoveExhibitions(data);
                        break;
                    }

                default:
                    {
                        break;
                    }
            }
        }

        public void ExistTrasfer(string data, string userId)
        {
            var bInstall = true;
            if (Euro_Online.Users.Count(x => x.UserId == userId) == 0)
            {
                bInstall = false;
            }
            Clients.Caller.existtrasfer(data, bInstall);
        }

        public void GetOnlineUsers()
        {
            var user = Euro_Online.Users.Find(u => u.ConnectionId == Context.ConnectionId);
            var sTransferOrgID = Common.ConfigGetValue("", "TransferOrgID");
            var sTransferUerID = Common.ConfigGetValue("", "TransferUerID");
            if (user != null)
            {
                if (user.UserId == sTransferUerID)
                {
                    Clients.Caller.onlineusers(Euro_Online.Users);
                }
                else
                {
                    var onlineUsers = Euro_Online.Users.Where(x => x.OrgId == user.OrgId).ToList();
                    var sIds = HubsTool.GetConnectionIdsByOrgID(user.OrgId);
                    Clients.Clients(sIds).onlineusers(onlineUsers);
                }
            }
        }

        public void Hello()
        {
            Clients.All.hello();
        }
    }
}