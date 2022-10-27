using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_OPM_ExportExhibition")]
    public partial class OTB_OPM_ExportExhibition : ModelContext
    {
           public OTB_OPM_ExportExhibition(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true)]
           public string ExportBillNO {get;set;}
           public const string CN_EXPORTBILLNO = "ExportBillNO";

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
           public string ExportDeclarationNO {get;set;}
           public const string CN_EXPORTDECLARATIONNO = "ExportDeclarationNO";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ImportBillNO {get;set;}
           public const string CN_IMPORTBILLNO = "ImportBillNO";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ImportDeclarationNO {get;set;}
           public const string CN_IMPORTDECLARATIONNO = "ImportDeclarationNO";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ExhibitionClass {get;set;}
           public const string CN_EXHIBITIONCLASS = "ExhibitionClass";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ExportBillName {get;set;}
           public const string CN_EXPORTBILLNAME = "ExportBillName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ExportBillEName {get;set;}
           public const string CN_EXPORTBILLENAME = "ExportBillEName";

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
           public string Organizer {get;set;}
           public const string CN_ORGANIZER = "Organizer";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ResBusiness {get;set;}
           public const string CN_RESBUSINESS = "ResBusiness";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CarriersNumber {get;set;}
           public const string CN_CARRIERSNUMBER = "CarriersNumber";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Exhibitors {get;set;}
           public const string CN_EXHIBITORS = "Exhibitors";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Suppliers {get;set;}
           public const string CN_SUPPLIERS = "Suppliers";

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
           /// Nullable:True
           /// </summary>           
           public string Agent {get;set;}
           public const string CN_AGENT = "Agent";

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
           public string BillLadNO {get;set;}
           public const string CN_BILLLADNO = "BillLadNO";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string BillLadNOSub {get;set;}
           public const string CN_BILLLADNOSUB = "BillLadNOSub";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ContainerNumber {get;set;}
           public const string CN_CONTAINERNUMBER = "ContainerNumber";

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
           public string Destination {get;set;}
           public const string CN_DESTINATION = "Destination";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ReturnBills {get;set;}
           public const string CN_RETURNBILLS = "ReturnBills";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CostData {get;set;}
           public const string CN_COSTDATA = "CostData";

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
           public double? ExchangeRate {get;set;}
           public const string CN_EXCHANGERATE = "ExchangeRate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ShippingCompany {get;set;}
           public const string CN_SHIPPINGCOMPANY = "ShippingCompany";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? DocumentDeadline {get;set;}
           public const string CN_DOCUMENTDEADLINE = "DocumentDeadline";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? ClosingDate {get;set;}
           public const string CN_CLOSINGDATE = "ClosingDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? ETC {get;set;}
           public const string CN_ETC = "ETC";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? ETD {get;set;}
           public const string CN_ETD = "ETD";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? ETA {get;set;}
           public const string CN_ETA = "ETA";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? ReminderAgentExecutionDate {get;set;}
           public const string CN_REMINDERAGENTEXECUTIONDATE = "ReminderAgentExecutionDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? PreExhibitionDate {get;set;}
           public const string CN_PREEXHIBITIONDATE = "PreExhibitionDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? ExitDate {get;set;}
           public const string CN_EXITDATE = "ExitDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Release {get;set;}
           public const string CN_RELEASE = "Release";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string SitiContactor1 {get;set;}
           public const string CN_SITICONTACTOR1 = "SitiContactor1";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string SitiTelephone1 {get;set;}
           public const string CN_SITITELEPHONE1 = "SitiTelephone1";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string SitiContactor2 {get;set;}
           public const string CN_SITICONTACTOR2 = "SitiContactor2";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string SitiTelephone2 {get;set;}
           public const string CN_SITITELEPHONE2 = "SitiTelephone2";

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
           public string IsVoid {get;set;}
           public const string CN_ISVOID = "IsVoid";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string TransportationMode {get;set;}
           public const string CN_TRANSPORTATIONMODE = "TransportationMode";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string DestinationCode {get;set;}
           public const string CN_DESTINATIONCODE = "DestinationCode";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string JhCode {get;set;}
           public const string CN_JHCODE = "JhCode";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string IsSendMail {get;set;}
           public const string CN_ISSENDMAIL = "IsSendMail";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string VoidReason {get;set;}
           public const string CN_VOIDREASON = "VoidReason";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Quote {get;set;}
           public const string CN_QUOTE = "Quote";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string EstimatedCost {get;set;}
           public const string CN_ESTIMATEDCOST = "EstimatedCost";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ActualCost {get;set;}
           public const string CN_ACTUALCOST = "ActualCost";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Bills {get;set;}
           public const string CN_BILLS = "Bills";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Flow_Status {get;set;}
           public const string CN_FLOW_STATUS = "Flow_Status";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string AgentContactor {get;set;}
           public const string CN_AGENTCONTACTOR = "AgentContactor";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string AgentTelephone {get;set;}
           public const string CN_AGENTTELEPHONE = "AgentTelephone";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string AgentEamil {get;set;}
           public const string CN_AGENTEAMIL = "AgentEamil";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CustomsClearance {get;set;}
           public const string CN_CUSTOMSCLEARANCE = "CustomsClearance";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ShipmentPortCode {get;set;}
           public const string CN_SHIPMENTPORTCODE = "ShipmentPortCode";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ShipmentPort {get;set;}
           public const string CN_SHIPMENTPORT = "ShipmentPort";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string AgentContactorName {get;set;}
           public const string CN_AGENTCONTACTORNAME = "AgentContactorName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ExhibitionNO {get;set;}
           public const string CN_EXHIBITIONNO = "ExhibitionNO";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ShipAndVoyage {get;set;}
           public const string CN_SHIPANDVOYAGE = "ShipAndVoyage";

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
           public string REF {get;set;}
           public const string CN_REF = "REF";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string DepartmentID {get;set;}
           public const string CN_DEPARTMENTID = "DepartmentID";

    }
}
