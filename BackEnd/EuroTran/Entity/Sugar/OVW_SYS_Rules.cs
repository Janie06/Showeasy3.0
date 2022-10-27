using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OVW_SYS_Rules")]
    public partial class OVW_SYS_Rules : ModelContext
    {
        public OVW_SYS_Rules()
        {
        }
        public string MemberName { get; set; }
        public string MemberIDs { get; set; }

        public string RuleID { get; set; }

        public string RuleName { get; set; }

        public string DelStatus { get; set; }

        public string Memo { get; set; }

        public string OrgID { get; set; }

        public string CreateDate { get; set; }

        public string CreateUser { get; set; }

        public string ModifyUser { get; set; }

        public string ModifyDate { get; set; }
    }
}
