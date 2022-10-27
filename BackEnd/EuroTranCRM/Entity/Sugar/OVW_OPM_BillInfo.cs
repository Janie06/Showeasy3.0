using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OVW_OPM_BillInfo")]
    public partial class OVW_OPM_BillInfo : ModelContext
    {
           public OVW_OPM_BillInfo(){


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
           public int SN {get;set;}
           public const string CN_SN = "SN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string BillType {get;set;}
           public const string CN_BILLTYPE = "BillType";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string BillGuid {get;set;}
           public const string CN_BILLGUID = "BillGuid";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string ParentId {get;set;}
           public const string CN_PARENTID = "ParentId";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string ExhibitionNO {get;set;}
           public const string CN_EXHIBITIONNO = "ExhibitionNO";

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
           /// Nullable:False
           /// </summary>           
           public string AuditVal {get;set;}
           public const string CN_AUDITVAL = "AuditVal";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string ResponsiblePerson {get;set;}
           public const string CN_RESPONSIBLEPERSON = "ResponsiblePerson";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string BillCreateDate {get;set;}
           public const string CN_BILLCREATEDATE = "BillCreateDate";

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
           /// Nullable:True
           /// </summary>           
           public string BillCheckDate {get;set;}
           public const string CN_BILLCHECKDATE = "BillCheckDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Currency {get;set;}
           public const string CN_CURRENCY = "Currency";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ExchangeRate {get;set;}
           public const string CN_EXCHANGERATE = "ExchangeRate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Advance {get;set;}
           public const string CN_ADVANCE = "Advance";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string FeeItems {get;set;}
           public const string CN_FEEITEMS = "FeeItems";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string InvoiceNumber {get;set;}
           public const string CN_INVOICENUMBER = "InvoiceNumber";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string InvoiceDate {get;set;}
           public const string CN_INVOICEDATE = "InvoiceDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ReceiptNumber {get;set;}
           public const string CN_RECEIPTNUMBER = "ReceiptNumber";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ReceiptDate {get;set;}
           public const string CN_RECEIPTDATE = "ReceiptDate";

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
           /// Nullable:True
           /// </summary>           
           public string Number {get;set;}
           public const string CN_NUMBER = "Number";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Unit {get;set;}
           public const string CN_UNIT = "Unit";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Weight {get;set;}
           public const string CN_WEIGHT = "Weight";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Volume {get;set;}
           public const string CN_VOLUME = "Volume";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string RefNumber {get;set;}
           public const string CN_REFNUMBER = "RefNumber";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ContactorName {get;set;}
           public const string CN_CONTACTORNAME = "ContactorName";

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
           public string ReFlow {get;set;}
           public const string CN_REFLOW = "ReFlow";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Index {get;set;}
           public const string CN_INDEX = "Index";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string AmountSum {get;set;}
           public const string CN_AMOUNTSUM = "AmountSum";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string TaxSum {get;set;}
           public const string CN_TAXSUM = "TaxSum";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string AmountTaxSum {get;set;}
           public const string CN_AMOUNTTAXSUM = "AmountTaxSum";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string TotalReceivable {get;set;}
           public const string CN_TOTALRECEIVABLE = "TotalReceivable";

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
           public string IsRetn {get;set;}
           public const string CN_ISRETN = "IsRetn";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ExhibitioShotName {get;set;}
           public const string CN_EXHIBITIOSHOTNAME = "ExhibitioShotName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string ExhibitioName {get;set;}
           public const string CN_EXHIBITIONAME = "ExhibitioName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string PayerName {get;set;}
           public const string CN_PAYERNAME = "PayerName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string ResponsiblePersonName {get;set;}
           public const string CN_RESPONSIBLEPERSONNAME = "ResponsiblePersonName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string ExhibitionCode { get; set; }
           public const string CN_EXHIBITIONCODE = "ExhibitionCode";
    }
}
