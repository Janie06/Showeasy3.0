using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_CRM_CustomersTransfer")]
    public partial class OTB_CRM_CustomersTransfer : ModelContext
    {
           public OTB_CRM_CustomersTransfer(){


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
           public string Feild01 {get;set;}
           public const string CN_FEILD01 = "Feild01";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild02 {get;set;}
           public const string CN_FEILD02 = "Feild02";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Feild03 {get;set;}
           public const string CN_FEILD03 = "Feild03";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Feild04 {get;set;}
           public const string CN_FEILD04 = "Feild04";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild05 {get;set;}
           public const string CN_FEILD05 = "Feild05";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild06 {get;set;}
           public const string CN_FEILD06 = "Feild06";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild07 {get;set;}
           public const string CN_FEILD07 = "Feild07";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild08 {get;set;}
           public const string CN_FEILD08 = "Feild08";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild09 {get;set;}
           public const string CN_FEILD09 = "Feild09";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Feild10 {get;set;}
           public const string CN_FEILD10 = "Feild10";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Feild11 {get;set;}
           public const string CN_FEILD11 = "Feild11";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Feild12 {get;set;}
           public const string CN_FEILD12 = "Feild12";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild13 {get;set;}
           public const string CN_FEILD13 = "Feild13";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild14 {get;set;}
           public const string CN_FEILD14 = "Feild14";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild15 {get;set;}
           public const string CN_FEILD15 = "Feild15";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild16 {get;set;}
           public const string CN_FEILD16 = "Feild16";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild17 {get;set;}
           public const string CN_FEILD17 = "Feild17";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild18 {get;set;}
           public const string CN_FEILD18 = "Feild18";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild19 {get;set;}
           public const string CN_FEILD19 = "Feild19";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild20 {get;set;}
           public const string CN_FEILD20 = "Feild20";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild21 {get;set;}
           public const string CN_FEILD21 = "Feild21";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild22 {get;set;}
           public const string CN_FEILD22 = "Feild22";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Feild23 {get;set;}
           public const string CN_FEILD23 = "Feild23";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild24 {get;set;}
           public const string CN_FEILD24 = "Feild24";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild25 {get;set;}
           public const string CN_FEILD25 = "Feild25";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild26 {get;set;}
           public const string CN_FEILD26 = "Feild26";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild27 {get;set;}
           public const string CN_FEILD27 = "Feild27";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild28 {get;set;}
           public const string CN_FEILD28 = "Feild28";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild29 {get;set;}
           public const string CN_FEILD29 = "Feild29";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild30 {get;set;}
           public const string CN_FEILD30 = "Feild30";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild31 {get;set;}
           public const string CN_FEILD31 = "Feild31";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild32 {get;set;}
           public const string CN_FEILD32 = "Feild32";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild33 {get;set;}
           public const string CN_FEILD33 = "Feild33";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild34 {get;set;}
           public const string CN_FEILD34 = "Feild34";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild35 {get;set;}
           public const string CN_FEILD35 = "Feild35";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild36 {get;set;}
           public const string CN_FEILD36 = "Feild36";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild37 {get;set;}
           public const string CN_FEILD37 = "Feild37";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild38 {get;set;}
           public const string CN_FEILD38 = "Feild38";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild39 {get;set;}
           public const string CN_FEILD39 = "Feild39";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild40 {get;set;}
           public const string CN_FEILD40 = "Feild40";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild41 {get;set;}
           public const string CN_FEILD41 = "Feild41";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild42 {get;set;}
           public const string CN_FEILD42 = "Feild42";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild43 {get;set;}
           public const string CN_FEILD43 = "Feild43";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild44 {get;set;}
           public const string CN_FEILD44 = "Feild44";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild45 {get;set;}
           public const string CN_FEILD45 = "Feild45";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild46 {get;set;}
           public const string CN_FEILD46 = "Feild46";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild47 {get;set;}
           public const string CN_FEILD47 = "Feild47";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild48 {get;set;}
           public const string CN_FEILD48 = "Feild48";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild49 {get;set;}
           public const string CN_FEILD49 = "Feild49";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild50 {get;set;}
           public const string CN_FEILD50 = "Feild50";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild51 {get;set;}
           public const string CN_FEILD51 = "Feild51";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild52 {get;set;}
           public const string CN_FEILD52 = "Feild52";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild53 {get;set;}
           public const string CN_FEILD53 = "Feild53";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild54 {get;set;}
           public const string CN_FEILD54 = "Feild54";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild55 {get;set;}
           public const string CN_FEILD55 = "Feild55";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild56 {get;set;}
           public const string CN_FEILD56 = "Feild56";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild57 {get;set;}
           public const string CN_FEILD57 = "Feild57";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild58 {get;set;}
           public const string CN_FEILD58 = "Feild58";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild59 {get;set;}
           public const string CN_FEILD59 = "Feild59";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild60 {get;set;}
           public const string CN_FEILD60 = "Feild60";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild61 {get;set;}
           public const string CN_FEILD61 = "Feild61";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild62 {get;set;}
           public const string CN_FEILD62 = "Feild62";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild63 {get;set;}
           public const string CN_FEILD63 = "Feild63";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild64 {get;set;}
           public const string CN_FEILD64 = "Feild64";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild65 {get;set;}
           public const string CN_FEILD65 = "Feild65";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild66 {get;set;}
           public const string CN_FEILD66 = "Feild66";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild67 {get;set;}
           public const string CN_FEILD67 = "Feild67";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild68 {get;set;}
           public const string CN_FEILD68 = "Feild68";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild69 {get;set;}
           public const string CN_FEILD69 = "Feild69";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild70 {get;set;}
           public const string CN_FEILD70 = "Feild70";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild71 {get;set;}
           public const string CN_FEILD71 = "Feild71";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild72 {get;set;}
           public const string CN_FEILD72 = "Feild72";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild73 {get;set;}
           public const string CN_FEILD73 = "Feild73";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild74 {get;set;}
           public const string CN_FEILD74 = "Feild74";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild75 {get;set;}
           public const string CN_FEILD75 = "Feild75";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild76 {get;set;}
           public const string CN_FEILD76 = "Feild76";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild77 {get;set;}
           public const string CN_FEILD77 = "Feild77";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild78 {get;set;}
           public const string CN_FEILD78 = "Feild78";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild79 {get;set;}
           public const string CN_FEILD79 = "Feild79";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild80 {get;set;}
           public const string CN_FEILD80 = "Feild80";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild81 {get;set;}
           public const string CN_FEILD81 = "Feild81";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild82 {get;set;}
           public const string CN_FEILD82 = "Feild82";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild83 {get;set;}
           public const string CN_FEILD83 = "Feild83";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild84 {get;set;}
           public const string CN_FEILD84 = "Feild84";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild85 {get;set;}
           public const string CN_FEILD85 = "Feild85";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild86 {get;set;}
           public const string CN_FEILD86 = "Feild86";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild87 {get;set;}
           public const string CN_FEILD87 = "Feild87";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild88 {get;set;}
           public const string CN_FEILD88 = "Feild88";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild89 {get;set;}
           public const string CN_FEILD89 = "Feild89";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild90 {get;set;}
           public const string CN_FEILD90 = "Feild90";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild91 {get;set;}
           public const string CN_FEILD91 = "Feild91";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild92 {get;set;}
           public const string CN_FEILD92 = "Feild92";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild93 {get;set;}
           public const string CN_FEILD93 = "Feild93";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild94 {get;set;}
           public const string CN_FEILD94 = "Feild94";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild95 {get;set;}
           public const string CN_FEILD95 = "Feild95";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild96 {get;set;}
           public const string CN_FEILD96 = "Feild96";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild97 {get;set;}
           public const string CN_FEILD97 = "Feild97";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild98 {get;set;}
           public const string CN_FEILD98 = "Feild98";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Feild99 {get;set;}
           public const string CN_FEILD99 = "Feild99";

    }
}
