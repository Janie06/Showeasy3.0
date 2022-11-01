using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_OPM_ExhibitionsTransfer")]
    public partial class OTB_OPM_ExhibitionsTransfer : ModelContext
    {
           public OTB_OPM_ExhibitionsTransfer(){


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
           public string PrjNO {get;set;}
           public const string CN_PRJNO = "PrjNO";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string PrjName {get;set;}
           public const string CN_PRJNAME = "PrjName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string PrjCharger {get;set;}
           public const string CN_PRJCHARGER = "PrjCharger";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string EndDate {get;set;}
           public const string CN_ENDDATE = "EndDate";

    }
}
