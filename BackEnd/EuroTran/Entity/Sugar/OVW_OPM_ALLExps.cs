using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OVW_OPM_ALLExps")]
    public partial class OVW_OPM_ALLExps : ModelContext
    {
        public OVW_OPM_ALLExps()
        {


        }
        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string Source { get; set; }
        public const string CN_SOURCE = "Source";

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
        public string Guid { get; set; }
        public const string CN_GUID = "Guid";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string RefNumber { get; set; }
        public const string CN_REFNUMBER = "RefNumber";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string IsVoid { get; set; }
        public const string CN_ISVOID = "IsVoid";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ExhibitionSN { get; set; }
        public const string CN_EXHIBITIONSN = "ExhibitionSN";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string DepartmentID { get; set; }
        public const string CN_DEPARTMENTID = "DepartmentID";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        public string ResponsiblePerson { get; set; }
        public const string CN_RESPONSIBLEPERSON = "ResponsiblePerson";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ActualCost { get; set; }
        public const string CN_ACTUALCOST = "ActualCost";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public string ReturnBills { get; set; }
        public const string CN_RETURNBILLS = "ReturnBills";

    }
}
