using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_OPM_Bills")]
    public partial class OTB_OPM_Bills : ModelContext
    {
           public OTB_OPM_Bills(){


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
           public string BillNO {get;set;}
           public const string CN_BILLNO = "BillNO";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string CheckDate {get;set;}
           public const string CN_CHECKDATE = "CheckDate";

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
           public string CustomerCode {get;set;}
           public const string CN_CUSTOMERCODE = "CustomerCode";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ResponsiblePersonCode {get;set;}
           public const string CN_RESPONSIBLEPERSONCODE = "ResponsiblePersonCode";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string LastGetBillDate {get;set;}
           public const string CN_LASTGETBILLDATE = "LastGetBillDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string LastGetBillNO {get;set;}
           public const string CN_LASTGETBILLNO = "LastGetBillNO";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string TaxType {get;set;}
           public const string CN_TAXTYPE = "TaxType";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string NOTaxAmount {get;set;}
           public const string CN_NOTAXAMOUNT = "NOTaxAmount";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string BillAmount {get;set;}
           public const string CN_BILLAMOUNT = "BillAmount";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string PaymentAmount {get;set;}
           public const string CN_PAYMENTAMOUNT = "PaymentAmount";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Allowance {get;set;}
           public const string CN_ALLOWANCE = "Allowance";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string DebtAmount {get;set;}
           public const string CN_DEBTAMOUNT = "DebtAmount";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ExchangeAmount {get;set;}
           public const string CN_EXCHANGEAMOUNT = "ExchangeAmount";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string Settle {get;set;}
           public const string CN_SETTLE = "Settle";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string InvoiceStartNumber {get;set;}
           public const string CN_INVOICESTARTNUMBER = "InvoiceStartNumber";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string InvoiceEndNumber {get;set;}
           public const string CN_INVOICEENDNUMBER = "InvoiceEndNumber";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Category {get;set;}
           public const string CN_CATEGORY = "Category";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string OrderNo {get;set;}
           public const string CN_ORDERNO = "OrderNo";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string ClosingNote {get;set;}
           public const string CN_CLOSINGNOTE = "ClosingNote";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string GeneralInvoiceNumber {get;set;}
           public const string CN_GENERALINVOICENUMBER = "GeneralInvoiceNumber";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string GeneralSerialNumber {get;set;}
           public const string CN_GENERALSERIALNUMBER = "GeneralSerialNumber";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Remark1 {get;set;}
           public const string CN_REMARK1 = "Remark1";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string AccountSource {get;set;}
           public const string CN_ACCOUNTSOURCE = "AccountSource";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string UpdateDate {get;set;}
           public const string CN_UPDATEDATE = "UpdateDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string UpdatePersonnel {get;set;}
           public const string CN_UPDATEPERSONNEL = "UpdatePersonnel";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string DepartmentSiteNumber {get;set;}
           public const string CN_DEPARTMENTSITENUMBER = "DepartmentSiteNumber";

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
           /// Nullable:True
           /// </summary>           
           public string TransferBNotes {get;set;}
           public const string CN_TRANSFERBNOTES = "TransferBNotes";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ABNumber {get;set;}
           public const string CN_ABNUMBER = "ABNumber";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string EnterNumber {get;set;}
           public const string CN_ENTERNUMBER = "EnterNumber";

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
           /// Nullable:False
           /// </summary>           
           public string ForeignAmount {get;set;}
           public const string CN_FOREIGNAMOUNT = "ForeignAmount";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string PayAmount {get;set;}
           public const string CN_PAYAMOUNT = "PayAmount";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string RefundAmount {get;set;}
           public const string CN_REFUNDAMOUNT = "RefundAmount";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string PaymentTerms {get;set;}
           public const string CN_PAYMENTTERMS = "PaymentTerms";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string AccountDate {get;set;}
           public const string CN_ACCOUNTDATE = "AccountDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string DCreditCardNumber {get;set;}
           public const string CN_DCREDITCARDNUMBER = "DCreditCardNumber";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ClosingDate {get;set;}
           public const string CN_CLOSINGDATE = "ClosingDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CusField1 {get;set;}
           public const string CN_CUSFIELD1 = "CusField1";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CusField2 {get;set;}
           public const string CN_CUSFIELD2 = "CusField2";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CusField3 {get;set;}
           public const string CN_CUSFIELD3 = "CusField3";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CusField4 {get;set;}
           public const string CN_CUSFIELD4 = "CusField4";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CusField5 {get;set;}
           public const string CN_CUSFIELD5 = "CusField5";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CusField6 {get;set;}
           public const string CN_CUSFIELD6 = "CusField6";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CusField7 {get;set;}
           public const string CN_CUSFIELD7 = "CusField7";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CusField8 {get;set;}
           public const string CN_CUSFIELD8 = "CusField8";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CusField9 {get;set;}
           public const string CN_CUSFIELD9 = "CusField9";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CusField10 {get;set;}
           public const string CN_CUSFIELD10 = "CusField10";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CusField11 {get;set;}
           public const string CN_CUSFIELD11 = "CusField11";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CusField12 {get;set;}
           public const string CN_CUSFIELD12 = "CusField12";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Remark2 {get;set;}
           public const string CN_REMARK2 = "Remark2";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string TWNOTaxAmount {get;set;}
           public const string CN_TWNOTAXAMOUNT = "TWNOTaxAmount";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string TWAmount {get;set;}
           public const string CN_TWAMOUNT = "TWAmount";

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
           public string CreateDate {get;set;}
           public const string CN_CREATEDATE = "CreateDate";

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
           public string BillFirstCheckDate {get;set;}
           public const string CN_BILLFIRSTCHECKDATE = "BillFirstCheckDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ResponsiblePersonFullCode {get;set;}
           public const string CN_RESPONSIBLEPERSONFULLCODE = "ResponsiblePersonFullCode";

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
           public string TaxSum {get;set;}
           public const string CN_TAXSUM = "TaxSum";

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
           public string IsRetn {get;set;}
           public const string CN_ISRETN = "IsRetn";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Url {get;set;}
           public const string CN_URL = "Url";

    }
}
