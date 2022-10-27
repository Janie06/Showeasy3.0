using System;

namespace Entity
{
    public class Token
    {
        /// <summary>
        /// 組織ID
        /// </summary>
        public int OrgId { get; set; }

        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public int UserName { get; set; }

        /// <summary>
        /// 用户名对应签名Token
        /// </summary>
        public string SignToken { get; set; }

        /// <summary>
        /// Token过期时间
        /// </summary>
        public DateTime ExpireTime { get; set; }
    }
}