using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_SYS_Calendar")]
    public partial class OTB_SYS_Calendar : ModelContext
    {
           public OTB_SYS_Calendar(){


           }
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
           [SugarColumn(IsPrimaryKey=true,IsIdentity=true)]
           public int NO {get;set;}
           public const string CN_NO = "NO";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string UserID {get;set;}
           public const string CN_USERID = "UserID";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string CalType {get;set;}
           public const string CN_CALTYPE = "CalType";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Title {get;set;}
           public const string CN_TITLE = "Title";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Description {get;set;}
           public const string CN_DESCRIPTION = "Description";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public DateTime StartDate {get;set;}
           public const string CN_STARTDATE = "StartDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public DateTime EndDate {get;set;}
           public const string CN_ENDDATE = "EndDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Importment {get;set;}
           public const string CN_IMPORTMENT = "Importment";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Color {get;set;}
           public const string CN_COLOR = "Color";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public bool AllDay {get;set;}
           public const string CN_ALLDAY = "AllDay";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string OpenMent {get;set;}
           public const string CN_OPENMENT = "OpenMent";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string GroupMembers {get;set;}
           public const string CN_GROUPMEMBERS = "GroupMembers";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Url {get;set;}
           public const string CN_URL = "Url";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public bool? Editable {get;set;}
           public const string CN_EDITABLE = "Editable";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ClassName {get;set;}
           public const string CN_CLASSNAME = "ClassName";

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
           public string OutlookId {get;set;}
           public const string CN_OUTLOOKID = "OutlookId";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string OutlookEventId {get;set;}
           public const string CN_OUTLOOKEVENTID = "OutlookEventId";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string OutlookCalUId {get;set;}
           public const string CN_OUTLOOKCALUID = "OutlookCalUId";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string OutlookRead {get;set;}
           public const string CN_OUTLOOKREAD = "OutlookRead";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string OutlookInfo {get;set;}
           public const string CN_OUTLOOKINFO = "OutlookInfo";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string RelationId {get;set;}
           public const string CN_RELATIONID = "RelationId";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public bool DelStatus { get; set; }
           public const string CN_DELSTATUS = "DelStatus";

    }
}
