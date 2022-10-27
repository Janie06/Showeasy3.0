using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_SYS_Currency")]
    public partial class OTB_SYS_Currency : ModelContext
    {
        public OTB_SYS_Currency()
        {


        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true)]
        public int year { get; set; }
        public const string CN_YEAR = "year";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true)]
        public int month { get; set; }
        public const string CN_MONTH = "month";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public string currency { get; set; }
        public const string CN_CURRENCY = "currency";

        /// <summary>
        /// Desc:
        /// Default:0
        /// Nullable:False
        /// </summary>           
        public decimal exchange_rate { get; set; }
        public const string CN_EXCHANGE_RATE = "exchange_rate";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true)]
        public string OrgID { get; set; }
        public const string CN_ORGID = "OrgID";

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
