using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_SYS_ProgramList")]
    public partial class OTB_SYS_ProgramList : ModelContext
    {
           public OTB_SYS_ProgramList(){


           }
           /// <summary>
           /// Desc:建議使用檔名
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true)]
           public string ProgramID {get;set;}
           public const string CN_PROGRAMID = "ProgramID";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string ProgramName {get;set;}
           public const string CN_PROGRAMNAME = "ProgramName";

           /// <summary>
           /// Desc:與SYS_ModuleList進行關聯
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string ModuleID {get;set;}
           public const string CN_MODULEID = "ModuleID";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string FilePath {get;set;}
           public const string CN_FILEPATH = "FilePath";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ImgPath {get;set;}
           public const string CN_IMGPATH = "ImgPath";

           /// <summary>
           /// Desc:例如：Add|ReAdd為新增和存儲後新增
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string AllowRight {get;set;}
           public const string CN_ALLOWRIGHT = "AllowRight";

           /// <summary>
           /// Desc:P：程式	R：報表	S：子程式
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ProgramType {get;set;}
           public const string CN_PROGRAMTYPE = "ProgramType";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string BackgroundCSS {get;set;}
           public const string CN_BACKGROUNDCSS = "BackgroundCSS";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string GroupTag {get;set;}
           public const string CN_GROUPTAG = "GroupTag";

           /// <summary>
           /// Desc:Y：程式有效	N：程式無效
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Effective {get;set;}
           public const string CN_EFFECTIVE = "Effective";

           /// <summary>
           /// Desc:Y：顯示	N：不顯示
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ShowInList {get;set;}
           public const string CN_SHOWINLIST = "ShowInList";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ShowInHome {get;set;}
           public const string CN_SHOWINHOME = "ShowInHome";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public bool? ShowTop {get;set;}
           public const string CN_SHOWTOP = "ShowTop";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string MainTableName {get;set;}
           public const string CN_MAINTABLENAME = "MainTableName";

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
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true)]
           public string OrgID {get;set;}
           public const string CN_ORGID = "OrgID";

    }
}
