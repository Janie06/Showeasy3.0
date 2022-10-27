using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_EIP_AttendanceDiff")]
    public partial class OTB_EIP_AttendanceDiff : ModelContext
    {
           public OTB_EIP_AttendanceDiff(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true)]
           public string Guid {get;set;}
           public const string CN_GUID = "Guid";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string OrgID {get;set;}
           public const string CN_ORGID = "OrgID";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string SignedNumber {get;set;}
           public const string CN_SIGNEDNUMBER = "SignedNumber";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string KeyNote {get;set;}
           public const string CN_KEYNOTE = "KeyNote";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Important {get;set;}
           public const string CN_IMPORTANT = "Important";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string CrosssignTurn {get;set;}
           public const string CN_CROSSSIGNTURN = "CrosssignTurn";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string AskTheDummy {get;set;}
           public const string CN_ASKTHEDUMMY = "AskTheDummy";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Handle_DeptID {get;set;}
           public const string CN_HANDLE_DEPTID = "Handle_DeptID";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Handle_Person {get;set;}
           public const string CN_HANDLE_PERSON = "Handle_Person";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? FillBrushDate {get;set;}
           public const string CN_FILLBRUSHDATE = "FillBrushDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string FillBrushType {get;set;}
           public const string CN_FILLBRUSHTYPE = "FillBrushType";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string FillBrushReason {get;set;}
           public const string CN_FILLBRUSHREASON = "FillBrushReason";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string FlowId {get;set;}
           public const string CN_FLOWID = "FlowId";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string CheckOrder {get;set;}
           public const string CN_CHECKORDER = "CheckOrder";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Status {get;set;}
           public const string CN_STATUS = "Status";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string IsHandled {get;set;}
           public const string CN_ISHANDLED = "IsHandled";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Inspectors {get;set;}
           public const string CN_INSPECTORS = "Inspectors";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Reminders {get;set;}
           public const string CN_REMINDERS = "Reminders";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CheckFlows {get;set;}
           public const string CN_CHECKFLOWS = "CheckFlows";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string HandleFlows {get;set;}
           public const string CN_HANDLEFLOWS = "HandleFlows";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string VoidReason {get;set;}
           public const string CN_VOIDREASON = "VoidReason";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Flows_Lock {get;set;}
           public const string CN_FLOWS_LOCK = "Flows_Lock";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Handle_Lock {get;set;}
           public const string CN_HANDLE_LOCK = "Handle_Lock";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Memo {get;set;}
           public const string CN_MEMO = "Memo";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CreateUser {get;set;}
           public const string CN_CREATEUSER = "CreateUser";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? CreateDate {get;set;}
           public const string CN_CREATEDATE = "CreateDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ModifyUser {get;set;}
           public const string CN_MODIFYUSER = "ModifyUser";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? ModifyDate {get;set;}
           public const string CN_MODIFYDATE = "ModifyDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string RelationId {get;set;}
           public const string CN_RELATIONID = "RelationId";

    }
}
