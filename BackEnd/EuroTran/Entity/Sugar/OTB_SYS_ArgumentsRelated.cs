using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_SYS_ArgumentsRelated")]
    public partial class OTB_SYS_ArgumentsRelated : ModelContext
    {
        public OTB_SYS_ArgumentsRelated()
        {
        }

        /// <summary>
        /// Desc: Default: Nullable:False
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string OrgID { get; set; }

        public const string CN_ORGID = "OrgID";

        /// <summary>
        /// Desc: Default: Nullable:False
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string ArgumentClassID { get; set; }

        public const string CN_ARGUMENTCLASSID = "ArgumentClassID";

        /// <summary>
        /// Desc: Default: Nullable:False
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string ArgumentID { get; set; }

        public const string CN_ARGUMENTID = "ArgumentID";

        /// <summary>
        /// Desc: Default: Nullable:True
        /// </summary>
        public string Field1 { get; set; }

        public const string CN_FIELD1 = "Field1";

        /// <summary>
        /// Desc: Default: Nullable:True
        /// </summary>
        public string Field2 { get; set; }

        public const string CN_FIELD2 = "Field2";

        /// <summary>
        /// Desc: Default: Nullable:True
        /// </summary>
        public string Field3 { get; set; }

        public const string CN_FIELD3 = "Field3";

        /// <summary>
        /// Desc: Default: Nullable:True
        /// </summary>
        public string Field4 { get; set; }

        public const string CN_FIELD4 = "Field4";

        /// <summary>
        /// Desc: Default: Nullable:True
        /// </summary>
        public string Field5 { get; set; }

        public const string CN_FIELD5 = "Field5";

        /// <summary>
        /// Desc: Default: Nullable:True
        /// </summary>
        public string Field6 { get; set; }

        public const string CN_FIELD6 = "Field6";

        /// <summary>
        /// Desc: Default: Nullable:True
        /// </summary>
        public string Memo { get; set; }

        public const string CN_MEMO = "Memo";

        /// <summary>
        /// Desc: Default: Nullable:True
        /// </summary>
        public string CreateUser { get; set; }

        public const string CN_CREATEUSER = "CreateUser";

        /// <summary>
        /// Desc: Default: Nullable:True
        /// </summary>
        public DateTime? CreateDate { get; set; }

        public const string CN_CREATEDATE = "CreateDate";

        /// <summary>
        /// Desc: Default: Nullable:True
        /// </summary>
        public string ModifyUser { get; set; }

        public const string CN_MODIFYUSER = "ModifyUser";

        /// <summary>
        /// Desc: Default: Nullable:True
        /// </summary>
        public DateTime? ModifyDate { get; set; }

        public const string CN_MODIFYDATE = "ModifyDate";
    }
}