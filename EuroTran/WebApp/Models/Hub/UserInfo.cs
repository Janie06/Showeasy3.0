using System;
using System.Collections.Generic;

namespace WebApp.Models.Hub
{
    public class UserInfo
    {
        public UserInfo()
        {
            LastLoginTime = DateTime.Now;

            Rooms = new List<RoomInfo>();

            Prgs = new List<EditPrgInfo>();
        }

        /// <summary>
        /// 當前連接通訊ID
        /// </summary>
        public string ConnectionId { get; set; }

        /// <summary>
        /// 組織ID
        /// </summary>
        public string OrgId { get; set; }

        /// <summary>
        /// 帳號ID
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// 帳號名稱
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 最後登陸時間
        /// </summary>
        public DateTime LastLoginTime { get; set; }

        /// <summary>
        /// 用户房间集合
        /// </summary>
        public virtual List<RoomInfo> Rooms { get; set; }

        //當前編輯程式id集合
        public List<EditPrgInfo> Prgs { get; set; }
    }
}