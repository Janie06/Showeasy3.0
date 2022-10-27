using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euro.Transfer.Model
{
    public class UserInfo
    {
        public UserInfo()
        {
            LastLoginTime = DateTime.Now;
        }
        /// <summary>
        /// 當前連接通訊ID
        /// </summary>
        public string ConnectionId { get; set; }
        /// <summary>
        /// 組織ID
        /// </summary>
        /// 
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
        /// 人員類別（內部，客戶，委外）
        /// </summary>
        public string UserType { get; set; }
        /// <summary>
        /// 最後登陸時間
        /// </summary>
        public DateTime LastLoginTime { get; set; }
    }
}
