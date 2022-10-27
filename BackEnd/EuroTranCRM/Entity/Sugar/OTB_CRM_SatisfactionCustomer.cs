using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    public partial class OTB_CRM_SatisfactionCustomer : ModelContext
    {
        public OTB_CRM_SatisfactionCustomer()
        {


        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public string SN { get; set; }
        public const string CN_SN = "SN";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string CaseSN { get; set; }
        public const string CN_CaseSN = "CaseSN";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string CompareDB { get; set; }
        public const string CN_CompareDB = "CompareDB";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string CustomerID { get; set; }
        public const string CN_CustomerID = "CustomerID";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string CustomerName { get; set; }
        public const string CN_CustomerName = "CustomerName";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FillerName { get; set; }
        public const string CN_FillerName = "FillerName";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Phone { get; set; }
        public const string CN_Phone = "Phone";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Email { get; set; }
        public const string CN_Email = "Email";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Feild01 { get; set; }
        public const string CN_Feild01 = "Feild01";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Feild02 { get; set; }
        public const string CN_Feild02 = "Feild02";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Feild03 { get; set; }
        public const string CN_Feild03 = "Feild03";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Feild04 { get; set; }
        public const string CN_Feild04 = "Feild04";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Feild05 { get; set; }
        public const string CN_Feild05 = "Feild05";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Feild06 { get; set; }
        public const string CN_Feild06 = "Feild06";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Feild07 { get; set; }
        public const string CN_Feild07 = "Feild07";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Feild08 { get; set; }
        public const string CN_Feild08 = "Feild08";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Feild09 { get; set; }
        public const string CN_Feild09 = "Feild09";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Feild10 { get; set; }
        public const string CN_Feild10 = "Feild10";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Feild11 { get; set; }
        public const string CN_Feild11 = "Feild11";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Memo { get; set; }
        public const string CN_Memo = "Memo";

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
    }
}
