using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_SYS_Profiles")]
    public partial class OTB_SYS_Profiles : ModelContext
    {
           public OTB_SYS_Profiles(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true,IsIdentity=true)]
           public int SN {get;set;}
           public const string CN_SN = "SN";

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
           public string UserID {get;set;}
           public const string CN_USERID = "UserID";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string ProfileType {get;set;}
           public const string CN_PROFILETYPE = "ProfileType";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string ProfileName {get;set;}
           public const string CN_PROFILENAME = "ProfileName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ProfileSet {get;set;}
           public const string CN_PROFILESET = "ProfileSet";

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
