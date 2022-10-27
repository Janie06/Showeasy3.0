using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_SYS_Organization")]
    public partial class OTB_SYS_Organization : ModelContext
    {
           public OTB_SYS_Organization(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true)]
           public string OrgID {get;set;}
           public const string CN_ORGID = "OrgID";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string OrgName {get;set;}
           public const string CN_ORGNAME = "OrgName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string OwnerName {get;set;}
           public const string CN_OWNERNAME = "OwnerName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Email {get;set;}
           public const string CN_EMAIL = "Email";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string TEL {get;set;}
           public const string CN_TEL = "TEL";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Ext {get;set;}
           public const string CN_EXT = "Ext";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Fax {get;set;}
           public const string CN_FAX = "Fax";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Address {get;set;}
           public const string CN_ADDRESS = "Address";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string LoGoId {get;set;}
           public const string CN_LOGOID = "LoGoId";

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
           /// Nullable:False
           /// </summary>           
           public string LoginURL {get;set;}
           public const string CN_LOGINURL = "LoginURL";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Introduction {get;set;}
           public const string CN_INTRODUCTION = "Introduction";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Effective {get;set;}
           public const string CN_EFFECTIVE = "Effective";

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
           public string SystemCName {get;set;}
           public const string CN_SYSTEMCNAME = "SystemCName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string SystemEName {get;set;}
           public const string CN_SYSTEMENAME = "SystemEName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string BackgroundImage {get;set;}
           public const string CN_BACKGROUNDIMAGE = "BackgroundImage";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ParentOrgID {get;set;}
           public const string CN_PARENTORGID = "ParentOrgID";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string WebsiteLgoId {get;set;}
           public const string CN_WEBSITELGOID = "WebsiteLgoId";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string PicShowId {get;set;}
           public const string CN_PICSHOWID = "PicShowId";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ServiceTitle {get;set;}
           public const string CN_SERVICETITLE = "ServiceTitle";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string VideoUrl {get;set;}
           public const string CN_VIDEOURL = "VideoUrl";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string VideoDescription {get;set;}
           public const string CN_VIDEODESCRIPTION = "VideoDescription";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string WebsiteLgoId_EN {get;set;}
           public const string CN_WEBSITELGOID_EN = "WebsiteLgoId_EN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string PicShowId_EN {get;set;}
           public const string CN_PICSHOWID_EN = "PicShowId_EN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ServiceTitle_EN {get;set;}
           public const string CN_SERVICETITLE_EN = "ServiceTitle_EN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string VideoUrl_EN {get;set;}
           public const string CN_VIDEOURL_EN = "VideoUrl_EN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string VideoDescription_EN {get;set;}
           public const string CN_VIDEODESCRIPTION_EN = "VideoDescription_EN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Introduction_EN {get;set;}
           public const string CN_INTRODUCTION_EN = "Introduction_EN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string WebsiteLgoId_CN {get;set;}
           public const string CN_WEBSITELGOID_CN = "WebsiteLgoId_CN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string PicShowId_CN {get;set;}
           public const string CN_PICSHOWID_CN = "PicShowId_CN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ServiceTitle_CN {get;set;}
           public const string CN_SERVICETITLE_CN = "ServiceTitle_CN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string VideoUrl_CN {get;set;}
           public const string CN_VIDEOURL_CN = "VideoUrl_CN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string VideoDescription_CN {get;set;}
           public const string CN_VIDEODESCRIPTION_CN = "VideoDescription_CN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Introduction_CN {get;set;}
           public const string CN_INTRODUCTION_CN = "Introduction_CN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string MissionAndVision_TW {get;set;}
           public const string CN_MISSIONANDVISION_TW = "MissionAndVision_TW";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string MissionAndVision_CN {get;set;}
           public const string CN_MISSIONANDVISION_CN = "MissionAndVision_CN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string MissionAndVision_EN {get;set;}
           public const string CN_MISSIONANDVISION_EN = "MissionAndVision_EN";

    }
}
