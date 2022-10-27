using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_CRM_Contactors")]
    public partial class OTB_CRM_Contactors : ModelContext
    {
        public OTB_CRM_Contactors()
        {


        }
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
        /// Nullable:True
        /// </summary>
        public string CustomerId { get; set; }
        public const string CN_CUSTOMERID = "CustomerId";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string ContactorName { get; set; }
        public const string CN_CONTACTORNAME = "ContactorName";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string NickName { get; set; }
        public const string CN_NICKNAME = "NickName";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string Call { get; set; }
        public const string CN_CALL = "Call";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string Birthday { get; set; }
        public const string CN_BIRTHDAY = "Birthday";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string MaritalStatus { get; set; }
        public const string CN_MARITALSTATUS = "MaritalStatus";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string PersonalMobilePhone { get; set; }
        public const string CN_PERSONALMOBILEPHONE = "PersonalMobilePhone";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string PersonalEmail { get; set; }
        public const string CN_PERSONALEMAIL = "PersonalEmail";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string LINE { get; set; }
        public const string CN_LINE = "LINE";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string WECHAT { get; set; }
        public const string CN_WECHAT = "WECHAT";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string Personality { get; set; }
        public const string CN_PERSONALITY = "Personality";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string Preferences { get; set; }
        public const string CN_PREFERENCES = "Preferences";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string PersonalAddress { get; set; }
        public const string CN_PERSONALADDRESS = "PersonalAddress";

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
        /// Nullable:True
        /// </summary>
        public string ImmediateSupervisor { get; set; }
        public const string CN_IMMEDIATESUPERVISOR = "ImmediateSupervisor";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string JobTitle { get; set; }
        public const string CN_JOBTITLE = "JobTitle";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string Department { get; set; }
        public const string CN_DEPARTMENT = "Department";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string Email1 { get; set; }
        public const string CN_EMAIL1 = "Email1";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string Email2 { get; set; }
        public const string CN_EMAIL2 = "Email2";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string Telephone1 { get; set; }
        public const string CN_TELEPHONE1 = "Telephone1";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string Telephone2 { get; set; }
        public const string CN_TELEPHONE2 = "Telephone2";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string ChoseReason { get; set; }
        public const string CN_CHOSEREASON = "ChoseReason";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public int? OrderByValue { get; set; }
        public const string CN_ORDERBYVALUE = "OrderByValue";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CreateUser { get; set; }
        public const string CN_CREATEUSER = "CreateUser";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? CreateDate { get; set; }
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

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Ext1 { get; set; }
        public const string CN_EXT1 = "Ext1";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Ext2 { get; set; }
        public const string CN_EXT2 = "Ext2";
    }
}
