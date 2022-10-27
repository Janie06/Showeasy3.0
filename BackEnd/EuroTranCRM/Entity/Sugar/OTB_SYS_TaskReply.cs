using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_SYS_TaskReply")]
    public partial class OTB_SYS_TaskReply : ModelContext
    {
           public OTB_SYS_TaskReply(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string EventID {get;set;}
           public const string CN_EVENTID = "EventID";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ReplyUser {get;set;}
           public const string CN_REPLYUSER = "ReplyUser";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? ReplyDate {get;set;}
           public const string CN_REPLYDATE = "ReplyDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ReplyStatus {get;set;}
           public const string CN_REPLYSTATUS = "ReplyStatus";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ReplyContent {get;set;}
           public const string CN_REPLYCONTENT = "ReplyContent";

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

    }
}
