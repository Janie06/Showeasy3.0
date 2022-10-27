using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_CRM_BusinessOpportunity")]
    public partial class OTB_CRM_BusinessOpportunity : ModelContext
    {
        public OTB_CRM_BusinessOpportunity()
        {
        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true)]
        public int SN { get; set; }
        public const string CN_SN = "SN";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>         
        public string ExhibitionNO { get; set; }
        public const string CN_EXHIBITIONNO = "ExhibitionNO";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 
        public string Year { get; set; }
        public const string CN_YEAR = "Year";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 
        public string ExhibitionShotName { get; set; }
        public const string CN_EXHIBITIONSHOTNAME = "ExhibitionShotName";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 
        public string ExhibitionName { get; set; }
        public const string CN_EXHIBITIONNAME = "ExhibitionName";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 
        public string ExhibitionName_EN { get; set; }
        public const string CN_EXHIBITIONNAME_EN = "ExhibitionName_EN";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 
        public string State { get; set; }
        public const string CN_STATE = "State";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 
        public string Industry { get; set; }
        public const string CN_INDUSTRY = "Industry";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 
        public string CustomerName { get; set; }
        public const string CN_CUSTOMERNAME = "CustomerName";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 

        public string Contactor { get; set; }
        public const string CN_CONTACTOR = "Contactor";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 
        public string Department { get; set; }
        public const string CN_DEPARTMENT = "Department";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 
        public string JobTitle { get; set; }
        public const string CN_JOBTITLE = "JobTitle";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 
        public string Email1 { get; set; }
        public const string CN_EMAIL1 = "Email1";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 
        public string Email2 { get; set; }
        public const string CN_EMAIL2 = "Email2";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 
        public string Telephone1 { get; set; }
        public const string CN_TELEPHONE1 = "Telephone1";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 
        public string Telephone2 { get; set; }
        public const string CN_TELEPHONE2 = "Telephone2";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 
        public string Effective { get; set; }
        public const string CN_EFFECTIVE = "Effective";

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
        public DateTime? CreateDate { get; set; }
        public const string CN_CREATEDATE = "CreateDate";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 
        public string ModifyUser { get; set; }
        public const string CN_MODIFYUSER = "ModifyUser";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 
        public DateTime? ModifyDate { get; set; }
        public const string CN_MODIFYDATTE = "ModifyDate";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 
        public DateTime? DateStart { get; set; }
        public const string CN_DATESTART = "DateStart";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary> 
        public DateTime? DateEnd{ get; set; }
        public const string CN_DATEEND = "DateEnd";
    }
}
