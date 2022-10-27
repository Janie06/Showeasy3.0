using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OVW_OPM_Bills")]
    public partial class OVW_OPM_Bills : ModelContext
    {
           public OVW_OPM_Bills(){


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
           public string BillNO {get;set;}
           public const string CN_BILLNO = "BillNO";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ForeignCurrencyCode {get;set;}
           public const string CN_FOREIGNCURRENCYCODE = "ForeignCurrencyCode";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string ExchangeRate {get;set;}
           public const string CN_EXCHANGERATE = "ExchangeRate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal? Advance {get;set;}
           public const string CN_ADVANCE = "Advance";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal? TWNOTaxAmount {get;set;}
           public const string CN_TWNOTAXAMOUNT = "TWNOTaxAmount";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal? TaxSum {get;set;}
           public const string CN_TAXSUM = "TaxSum";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal? BillAmount {get;set;}
           public const string CN_BILLAMOUNT = "BillAmount";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal? TotalReceivable {get;set;}
           public const string CN_TOTALRECEIVABLE = "TotalReceivable";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CreateDate {get;set;}
           public const string CN_CREATEDATE = "CreateDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string CreateUser {get;set;}
           public const string CN_CREATEUSER = "CreateUser";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string BillFirstCheckDate {get;set;}
           public const string CN_BILLFIRSTCHECKDATE = "BillFirstCheckDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string ResponsiblePersonCodeName {get;set;}
           public const string CN_RESPONSIBLEPERSONCODENAME = "ResponsiblePersonCodeName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Payer {get;set;}
           public const string CN_PAYER = "Payer";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Url {get;set;}
           public const string CN_URL = "Url";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ProjectNumber {get;set;}
           public const string CN_PROJECTNUMBER = "ProjectNumber";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string CustomerCode {get;set;}
           public const string CN_CUSTOMERCODE = "CustomerCode";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ResponsiblePersonFullCode {get;set;}
           public const string CN_RESPONSIBLEPERSONFULLCODE = "ResponsiblePersonFullCode";

    }
}
