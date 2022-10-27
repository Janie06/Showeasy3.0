using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_EIP_LeaveRequest")]
    public partial class OTB_EIP_LeaveRequest : ModelContext
    {
        public OTB_EIP_LeaveRequest()
        {

        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true)]
        public string OrgID { get; set; }
        public const string CN_ORGID = "OrgID";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true)]
        public string guid { get; set; }
        public const string CN_GUID = "guid";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string MemberID { get; set; }
        public const string CN_MemberID = "MemberID";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Leave { get; set; }
        public const string CN_LEAVE = "Leave";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? EnableDate { get; set; }
        public const string CN_ENABLEDATE = "EnableDate";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? ExpirationDate { get; set; }
        public const string CN_EXPIRATIONDATE = "ExpirationDate";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? PaymentHours { get; set; }
        public const string CN_PAYMENTHOURS = "PaymentHours";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public decimal? UsedHours { get; set; }
        public const string CN_USEDHOURS = "UsedHours";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        ///// </summary>           
        public decimal? RemainHours { get; set; }
        public const string CN_REMAINHOURS = "RemainHours";


        /// <summary>
        /// Desc:是否有被任何資料使用，例如:使用特休部分，就不能被刪除。
        /// Default:
        /// Nullable:True
        /// </summary>           
        public bool DelFlag { get; set; }
        public const string CN_DELFLAG = "DelFlag";


        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Memo { get; set; }
        public const string CN_MEMO = "Memo";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string CreateUser { get; set; }
        public const string CN_CREATEUSER = "CreateUser";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public DateTime CreateDate { get; set; }
        public const string CN_CREATEDATE = "CreateDate";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ModifyUser { get; set; }
        public const string CN_MODIFYUSER = "ModifyUser";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? ModifyDate { get; set; }
        public const string CN_MODIFYDATE = "ModifyDate";

    }
}
