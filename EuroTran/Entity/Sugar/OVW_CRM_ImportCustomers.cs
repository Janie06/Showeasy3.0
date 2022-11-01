using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OVW_CRM_ImportCustomers")]
    public partial class OVW_CRM_ImportCustomers : ModelContext
    {
           public OVW_CRM_ImportCustomers(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string guid {get;set;}
           public const string CN_GUID = "guid";

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
           public int ExhibitionNO {get;set;}
           public const string CN_EXHIBITIONNO = "ExhibitionNO";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string AppointNO {get;set;}
           public const string CN_APPOINTNO = "AppointNO";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CustomerNO {get;set;}
           public const string CN_CUSTOMERNO = "CustomerNO";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string UniCode {get;set;}
           public const string CN_UNICODE = "UniCode";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string TransactionType {get;set;}
           public const string CN_TRANSACTIONTYPE = "TransactionType";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string CustomerCName {get;set;}
           public const string CN_CUSTOMERCNAME = "CustomerCName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CustomerEName {get;set;}
           public const string CN_CUSTOMERENAME = "CustomerEName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CustomerShotCName {get;set;}
           public const string CN_CUSTOMERSHOTCNAME = "CustomerShotCName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CustomerShotEName {get;set;}
           public const string CN_CUSTOMERSHOTENAME = "CustomerShotEName";

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
           public string Telephone {get;set;}
           public const string CN_TELEPHONE = "Telephone";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string EXT {get;set;}
           public const string CN_EXT = "EXT";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string FAX {get;set;}
           public const string CN_FAX = "FAX";

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
           public string InvoiceAddress {get;set;}
           public const string CN_INVOICEADDRESS = "InvoiceAddress";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Contactor {get;set;}
           public const string CN_CONTACTOR = "Contactor";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string IsAudit {get;set;}
           public const string CN_ISAUDIT = "IsAudit";

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
           public string NotPassReason {get;set;}
           public const string CN_NOTPASSREASON = "NotPassReason";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string BankName {get;set;}
           public const string CN_BANKNAME = "BankName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string BankAcount {get;set;}
           public const string CN_BANKACOUNT = "BankAcount";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string TaxpayerOrgID {get;set;}
           public const string CN_TAXPAYERORGID = "TaxpayerOrgID";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ToAuditer {get;set;}
           public const string CN_TOAUDITER = "ToAuditer";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string IsApply {get;set;}
           public const string CN_ISAPPLY = "IsApply";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string WebsiteAdress {get;set;}
           public const string CN_WEBSITEADRESS = "WebsiteAdress";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public bool? IsFormal {get;set;}
           public const string CN_ISFORMAL = "IsFormal";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string FormalGuid {get;set;}
           public const string CN_FORMALGUID = "FormalGuid";

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
           public string MuseumMumber {get;set;}
           public const string CN_MUSEUMMUMBER = "MuseumMumber";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ExhibitionArea {get;set;}
           public const string CN_EXHIBITIONAREA = "ExhibitionArea";

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
           /// Nullable:False
           /// </summary>           
           public string IsAppoint {get;set;}
           public const string CN_ISAPPOINT = "IsAppoint";

    }
}
