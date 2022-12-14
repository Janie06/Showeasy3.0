using System;
using SqlSugar;

namespace EasyBL.WebApi.Models
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("TicketAuth")]
    public partial class TicketAuth
    {
        public TicketAuth()
        {
        }

        [SugarColumn(IsIgnore = true)]
        public int RowIndex { get; set; }

        /// <summary>
        /// Desc: Default: Nullable:False
        /// </summary>
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int NO { get; set; }

        public const string CN_NO = "NO";

        /// <summary>
        /// Desc: Default: Nullable:True
        /// </summary>
        public string OrgID { get; set; }

        public const string CN_ORGID = "OrgID";

        /// <summary>
        /// Desc: Default: Nullable:False
        /// </summary>
        public string UserID { get; set; }

        public const string CN_USERID = "UserID";

        /// <summary>
        /// Desc: Default: Nullable:True
        /// </summary>
        public string UserName { get; set; }

        public const string CN_USERNAME = "UserName";

        /// <summary>
        /// Desc: Default: Nullable:False
        /// </summary>
        public string Token { get; set; }

        public const string CN_TOKEN = "Token";

        /// <summary>
        /// Desc: Default: Nullable:True
        /// </summary>
        public string LoginIp { get; set; }

        public const string CN_LOGINIP = "LoginIp";

        /// <summary>
        /// Desc: Default: Nullable:True
        /// </summary>
        public string IsVerify { get; set; }

        public const string CN_ISVERIFY = "IsVerify";

        /// <summary>
        /// Desc: Default: Nullable:True
        /// </summary>
        public DateTime? LoginTime { get; set; }

        public const string CN_LOGINTIME = "LoginTime";

        /// <summary>
        /// Desc: Default: Nullable:False
        /// </summary>
        public DateTime ExpireTime { get; set; }

        public const string CN_EXPIRETIME = "ExpireTime";

        /// <summary>
        /// Desc: Default: Nullable:True
        /// </summary>
        public DateTime? CreateTime { get; set; }

        public const string CN_CREATETIME = "CreateTime";

        /// <summary>
        /// Desc: Default: Nullable:True
        /// </summary>
        public string OutlookId { get; set; }

        public const string CN_OUTLOOKID = "OutlookId";
    }
}