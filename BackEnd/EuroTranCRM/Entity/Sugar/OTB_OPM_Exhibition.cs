using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_OPM_Exhibition")]
    public partial class OTB_OPM_Exhibition : ModelContext
    {
           public OTB_OPM_Exhibition(){


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
           public string Exhibitioname_TW {get;set;}
           public const string CN_EXHIBITIONAME_TW = "Exhibitioname_TW";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Exhibitioname_EN {get;set;}
           public const string CN_EXHIBITIONAME_EN = "Exhibitioname_EN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? ExhibitionDateStart {get;set;}
           public const string CN_EXHIBITIONDATESTART = "ExhibitionDateStart";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? ExhibitionDateEnd {get;set;}
           public const string CN_EXHIBITIONDATEEND = "ExhibitionDateEnd";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Area {get;set;}
           public const string CN_AREA = "Area";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string State {get;set;}
           public const string CN_STATE = "State";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Industry {get;set;}
           public const string CN_INDUSTRY = "Industry";

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
           /// Default:N
           /// Nullable:True
           /// </summary>           
           public string IsShowWebSite {get;set;}
           public const string CN_ISSHOWWEBSITE = "IsShowWebSite";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ExhibitionCode {get;set;}
           public const string CN_EXHIBITIONCODE = "ExhibitionCode";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Exhibitioname_CN {get;set;}
           public const string CN_EXHIBITIONAME_CN = "Exhibitioname_CN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ExhibitionAddress {get;set;}
           public const string CN_EXHIBITIONADDRESS = "ExhibitionAddress";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string LogoFileId {get;set;}
           public const string CN_LOGOFILEID = "LogoFileId";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string IsShowWebSim {get;set;}
           public const string CN_ISSHOWWEBSIM = "IsShowWebSim";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? ShelfTime_Home {get;set;}
           public const string CN_SHELFTIME_HOME = "ShelfTime_Home";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? ShelfTime_Abroad {get;set;}
           public const string CN_SHELFTIME_ABROAD = "ShelfTime_Abroad";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ExhibitioShotName_TW {get;set;}
           public const string CN_EXHIBITIOSHOTNAME_TW = "ExhibitioShotName_TW";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ExhibitioShotName_CN {get;set;}
           public const string CN_EXHIBITIOSHOTNAME_CN = "ExhibitioShotName_CN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ExhibitioShotName_EN {get;set;}
           public const string CN_EXHIBITIOSHOTNAME_EN = "ExhibitioShotName_EN";

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
           /// Nullable:True
           /// </summary>           
           public string Effective {get;set;}
           public const string CN_EFFECTIVE = "Effective";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string IsTransfer {get;set;}
           public const string CN_ISTRANSFER = "IsTransfer";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? LastTransfer_Time {get;set;}
           public const string CN_LASTTRANSFER_TIME = "LastTransfer_Time";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CostRulesId {get;set;}
           public const string CN_COSTRULESID = "CostRulesId";

            /// <summary>
            /// Desc:
            /// Default:
            /// Nullable:True
            /// </summary>   
            public string ResponsiblePerson { get; set; }
            public const string CN_RESPONSIBLEPERSON = "ResponsiblePerson";

            public DateTime? SeaReceiveingDate { get; set; }
            public DateTime? SeaClosingDate { get; set; }
            public DateTime? AirReceiveingDate { get; set; }
            public DateTime? AirClosingDate { get; set; }
            public string Undertaker { get; set; }
            public string Telephone { get; set; }
            public string Email { get; set; }
            public string WebsiteAdress { get; set; }
            public string IsShowWebSiteAppoint { get; set; }
    }
}
