using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_WSM_TrackingLog")]
    public partial class OTB_WSM_TrackingLog : ModelContext
    {
           public OTB_WSM_TrackingLog(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true,IsIdentity=true)]
           public int NO {get;set;}
           public const string CN_NO = "NO";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string OrgID {get;set;}
           public const string CN_ORGID = "OrgID";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string QueryNumber {get;set;}
           public const string CN_QUERYNUMBER = "QueryNumber";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string QueryIp {get;set;}
           public const string CN_QUERYIP = "QueryIp";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? QueryTime {get;set;}
           public const string CN_QUERYTIME = "QueryTime";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string IPInfo {get;set;}
           public const string CN_IPINFO = "IPInfo";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CustomerName {get;set;}
           public const string CN_CUSTOMERNAME = "CustomerName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ParentId {get;set;}
           public const string CN_PARENTID = "ParentId";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string AgentName {get;set;}
           public const string CN_AGENTNAME = "AgentName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string DepartmentIDs {get;set;}
           public const string CN_DEPARTMENTIDS = "DepartmentIDs";

    }
}
