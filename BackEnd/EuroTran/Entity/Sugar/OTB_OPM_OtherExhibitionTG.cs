using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_OPM_OtherExhibitionTG")]
    public partial class OTB_OPM_OtherExhibitionTG : ModelContext
    {
           public OTB_OPM_OtherExhibitionTG(){


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
           [SugarColumn(IsPrimaryKey=true)]
           public string Guid {get;set;}
           public const string CN_GUID = "Guid";

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
           public string ImportDeclarationNO {get;set;}
           public const string CN_IMPORTDECLARATIONNO = "ImportDeclarationNO";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ImportBillName {get;set;}
           public const string CN_IMPORTBILLNAME = "ImportBillName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ImportBillEName {get;set;}
           public const string CN_IMPORTBILLENAME = "ImportBillEName";

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
           public string Hall {get;set;}
           public const string CN_HALL = "Hall";

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
           public string DeclarationClass {get;set;}
           public const string CN_DECLARATIONCLASS = "DeclarationClass";

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
           public string AgentContactor {get;set;}
           public const string CN_AGENTCONTACTOR = "AgentContactor";

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
           public string AgentTelephone {get;set;}
           public const string CN_AGENTTELEPHONE = "AgentTelephone";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string AgentEmail {get;set;}
           public const string CN_AGENTEMAIL = "AgentEmail";

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
           public string DestinationPort {get;set;}
           public const string CN_DESTINATIONPORT = "DestinationPort";

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
           public DateTime? ArrivalTime {get;set;}
           public const string CN_ARRIVALTIME = "ArrivalTime";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string StoragePlace {get;set;}
           public const string CN_STORAGEPLACE = "StoragePlace";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? BoxNo {get;set;}
           public const string CN_BOXNO = "BoxNo";

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
           public decimal? Weight {get;set;}
           public const string CN_WEIGHT = "Weight";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Size {get;set;}
           public const string CN_SIZE = "Size";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string VolumeWeight {get;set;}
           public const string CN_VOLUMEWEIGHT = "VolumeWeight";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string GoodsType {get;set;}
           public const string CN_GOODSTYPE = "GoodsType";

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
           public DateTime? FreePeriod {get;set;}
           public const string CN_FREEPERIOD = "FreePeriod";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string SitiContactor {get;set;}
           public const string CN_SITICONTACTOR = "SitiContactor";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string SitiTelephone {get;set;}
           public const string CN_SITITELEPHONE = "SitiTelephone";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? ApproachTime {get;set;}
           public const string CN_APPROACHTIME = "ApproachTime";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? ExitTime {get;set;}
           public const string CN_EXITTIME = "ExitTime";

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
           public string CustomsClearance {get;set;}
           public const string CN_CUSTOMSCLEARANCE = "CustomsClearance";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string SignatureFileId {get;set;}
           public const string CN_SIGNATUREFILEID = "SignatureFileId";

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
           public string IsVoid {get;set;}
           public const string CN_ISVOID = "IsVoid";

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
           public string Price {get;set;}
           public const string CN_PRICE = "Price";

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
           public string DestinationPortCode {get;set;}
           public const string CN_DESTINATIONPORTCODE = "DestinationPortCode";

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
           public string SupplierEamil {get;set;}
           public const string CN_SUPPLIEREAMIL = "SupplierEamil";

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
           public string ImportPerson {get;set;}
           public const string CN_IMPORTPERSON = "ImportPerson";

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
           /// Nullable:True
           /// </summary>           
           public string DepartmentID {get;set;}
           public const string CN_DEPARTMENTID = "DepartmentID";

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

    }
}
