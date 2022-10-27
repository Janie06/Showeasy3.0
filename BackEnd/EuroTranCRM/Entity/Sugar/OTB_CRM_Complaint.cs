using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{

    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_CRM_Complaint")]

    public partial class OTB_CRM_Complaint: ModelContext
    {
        public OTB_CRM_Complaint()
        {
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:False
        /// </summary>           
        [SugarColumn(IsPrimaryKey = true)]
        public string Guid { get; set; }
        public const string CN_GUID = "Guid";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string  ComplaintNumber{ get; set; }
        public const string CN_COMPLAINTNUMBER = "ComplaintNumber";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string ComplaintType { get; set; }
        public const string CN_COMPLAINTTYPE = "ComplaintType";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string Important { get; set; }
        public const string CN_IMPORTANT = "Important";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string ComplaintTitle { get; set; }
        public const string CN_COMPLAINTTITLE = "ComplaintTitle";

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
        public string ComplaintSource { get; set; }
        public const string CN_COMPLAINTSOURCE = "ComplaintSource";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string CoopAgent { get; set; }
        public const string CN_COOPAGENT = "CoopAgent";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string GroupUnit { get; set; }
        public const string CN_GROUPUNIT = "GroupUnit";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string Description{ get; set; }
        public const string CN_DESCRIPTION = "Description";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string CustomerId { get; set; }
        public const string CN_CUSTOMERID= "CustomerId";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string Complainant { get; set; }
        public const string CN_COMPLAINANT = "Complainant";

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
        public string JobTitle { get; set; }
        public const string CN_JOBTITLE = "JobTitle";

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
        public string FlowId { get; set; }
        public const string CN_FLOWID = "FlowId";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string CheckOrder { get; set; }
        public const string CN_CHECKORDER = "CheckOrder";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string DataType { get; set; }
        public const string CN_DATATYPE = "DataType";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string IsHandled { get; set; }
        public const string CN_ISHANDLED = "IsHandled";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string Inspectors { get; set; }
        public const string CN_INSPECTORS = "Inspectors";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string Reminders { get; set; }
        public const string CN_REMINDERS = "Reminders";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string CheckFlows { get; set; }
        public const string CN_CHECKFLOWS = "CheckFlows";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string HandleFlows { get; set; }
        public const string CN_HANDLEFLOWS = "HandleFlows";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string VoidReason { get; set; }
        public const string CN_VOIDREASON = "VoidReason";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string Flows_Lock { get; set; }
        public const string CN_FLOWS_LOCK = "Flows_Lock";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string Handle_Lock { get; set; }
        public const string CN_HANDLE_LOCK = "Handle_Lock";

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

       

        public string Handle_DeptID { get; set; }
        public const string CN_HANDLE_DEPTID = "Handle_DeptID";

        public string Handle_Person { get; set; }
        public const string CN_HANDLE_PERSON = "Handle_Person";

        public string RelationId { get; set; }
        public const string CN_RELATIONID = "RelationId";
    }
}
