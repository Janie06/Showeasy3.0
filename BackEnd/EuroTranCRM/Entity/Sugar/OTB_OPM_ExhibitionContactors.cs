using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_OPM_ExhibitionContactors")]
    public partial class OTB_OPM_ExhibitionContactors : ModelContext
    {
        public OTB_OPM_ExhibitionContactors()
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
        public string ContactorId { get; set; }
        public const string CN_CONTACTORID = "ContactorId";

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
        public string IsMain { get; set; }
        public const string CN_ISMAIN = "IsMain";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string IsFormal { get; set; }
        public const string CN_ISFORMAL = "IsFormal";
    }
}
