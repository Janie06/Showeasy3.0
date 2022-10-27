using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_OPM_ExhibitionCustomers")]
    public partial class OTB_OPM_ExhibitionCustomers : ModelContext
    {
        public OTB_OPM_ExhibitionCustomers()
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
        public string ExhibitionNO { get; set; }
        public const string CN_EXHIBITIONNO = "ExhibitionNO";

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
        public string SourceType { get; set; }
        public const string CN_SOURCETYPE = "SourceType";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ListSource { get; set; }
        public const string CN_LISTSOURCE = "ListSource";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string BoothNumber { get; set; }
        public const string CN_BOOTHNUMBER = "BoothNumber";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string NumberOfBooths { get; set; }
        public const string CN_NUMBEROFBOOTHS = "NumberOfBooths";

        public string TransportRequire { get; set; }
        public string TransportationMode { get; set; }
        public string ProcessingMode { get; set; }
        public string VolumeForecasting { get; set; }
        public string Potential { get; set; }
        public string CoopTrasportCompany { get; set; }
        public string Memo { get; set; }
    }
}
