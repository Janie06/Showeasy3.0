using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_CRM_ImportExhibitionList")]
    public partial class OTB_CRM_ImportExhibitionList : ModelContext
    {
        public OTB_CRM_ImportExhibitionList()
        {


        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string guid { get; set; }
        public const string CN_GUID = "guid";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string OrgID { get; set; }
        public const string CN_ORGID = "OrgID";

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
        public string CustomerCName { get; set; }
        public const string CN_CUSTOMERCNAME = "CustomerCName";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string CustomerEName { get; set; }
        public const string CN_CUSTOMERENAME = "CustomerEName";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string UniCode { get; set; }
        public const string CN_UNICODE = "UniCode";




        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string BoothNumber { get; set; }
        public const string CN_BOOTHNUMBER = "BoothNumber";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string NumberOfBooths { get; set; }
        public const string CN_NUMBEROFBOOTHS = "NumberOfBooths";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Telephone { get; set; }
        public const string CN_TELEPHONE = "Telephone";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string FAX { get; set; }
        public const string CN_FAX = "FAX";




        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Address { get; set; }
        public const string CN_ADDRESS = "Address";


        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string WebsiteAdress { get; set; }
        public const string CN_WEBSITEADRESS = "WebsiteAdress";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Contactor1 { get; set; }
        public const string CN_CONTACTOR1 = "Contactor1";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string JobTitleC1 { get; set; }
        public const string CN_JOBTITLEC1 = "JobTitleC1";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Telephone1C1 { get; set; }
        public const string CN_TELEPHONE1C1 = "Telephone1C1";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Telephone2C1 { get; set; }
        public const string CN_TELEPHONE2C1 = "Telephone2C1";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Email1C1 { get; set; }
        public const string CN_EMAIL1C1 = "Email1C1";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Email2C1 { get; set; }
        public const string CN_EMAIL2C1 = "Email2C1";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Contactor2 { get; set; }
        public const string CN_CONTACTOR2 = "Contactor2";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string JobTitleC2 { get; set; }
        public const string CN_JOBTITLEC2 = "JobTitleC2";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Telephone1C2 { get; set; }
        public const string CN_TELEPHONE1C2 = "Telephone1C2";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Telephone2C2 { get; set; }
        public const string CN_TELEPHONE2C2 = "Telephone2C2";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Email1C2 { get; set; }
        public const string CN_EMAIL1C2 = "Email1C2";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Email2C2 { get; set; }
        public const string CN_EMAIL2C2 = "Email2C2";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Contactor3 { get; set; }
        public const string CN_CONTACTOR3 = "Contactor3";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string JobTitleC3 { get; set; }
        public const string CN_JOBTITLEC3 = "JobTitleC3";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Telephone1C3 { get; set; }
        public const string CN_TELEPHONE1C3 = "Telephone1C3";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Telephone2C3 { get; set; }
        public const string CN_TELEPHONE2C3 = "Telephone2C3";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Email1C3 { get; set; }
        public const string CN_EMAIL1C3 = "Email1C3";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Email2C3 { get; set; }
        public const string CN_EMAIL2C3 = "Email2C3";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Seq { get; set; }
        public const string CN_SEQ = "Seq";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Ext1C1 { get; set; }
        public const string CN_EXT1C1 = "Ext1C1";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Ext2C1 { get; set; }
        public const string CN_EXT2C1 = "Ext2C1";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Ext1C2 { get; set; }
        public const string CN_EXT1C2 = "Ext1C2";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Ext2C2 { get; set; }
        public const string CN_EXT2C2 = "Ext2C2";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Ext1C3 { get; set; }
        public const string CN_EXT1C3 = "Ext1C3";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string Ext2C3 { get; set; }
        public const string CN_EXT2C3 = "Ext2C3";
    }
}
