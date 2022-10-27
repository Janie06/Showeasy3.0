using Entity.Sugar;

namespace Entity  
{  
	 public class EntityHelper
	 {
        public const string COMPLAINT = "complaint";

        public const string BUSINESSOPPORTUNITY = "businessopportunity";

		public const string CUSTOMERS = "customers";

		public const string CUSTOMERSTRANSFER = "customerstransfer";

		public const string CUSTOMERSTRANSFERBAK = "customerstransferbak";

		public const string IMPORTCUSTOMERS = "importcustomers";

		public const string ATTENDANCE = "attendance";

		public const string ATTENDANCEDIFF = "attendancediff";

		public const string BILLCHANGEAPPLY = "billchangeapply";

		public const string BUSINESSTRAVEL = "businesstravel";

		public const string CHECKFLOW = "checkflow";

		public const string INVOICEAPPLYINFO = "invoiceapplyinfo";

		public const string LEAVE = "leave";

		public const string LEAVESET = "leaveset";

		public const string OVERTIME = "overtime";

		public const string TRAVELEXPENSE = "travelexpense";

		public const string WENZHONG = "wenzhong";

		public const string BILLINFO = "billinfo";

		public const string BILLS = "bills";

		public const string BILLSBAK = "billsbak";

		public const string EXHIBITION = "exhibition";

		public const string EXHIBITIONSTRANSFER = "exhibitionstransfer";

		public const string EXHIBITIONSTRANSFERBAK = "exhibitionstransferbak";

		public const string EXPORTEXHIBITION = "exportexhibition";

		public const string IMPORTEXHIBITION = "importexhibition";

		public const string OTHEREXHIBITION = "otherexhibition";

		public const string OTHEREXHIBITIONTG = "otherexhibitiontg";

		public const string ANNOUNCEMENT = "announcement";

		public const string READ = "read";

		public const string ARGUMENTCLASS = "argumentclass";

		public const string ARGUMENTS = "arguments";

		public const string ARGUMENTSRELATED = "argumentsrelated";

		public const string AUTHORIZE = "authorize";

		public const string AUTHORIZEFORDEPT = "authorizefordept";

		public const string AUTHORIZEFORMEMBER = "authorizeformember";

		public const string CALENDAR = "calendar";

		public const string CLOCKTIPS = "clocktips";

		public const string CLOCKTIPSBAK = "clocktipsbak";

		public const string DEPARTMENTS = "departments";

		public const string DOCUMENT = "document";

		public const string EMAIL = "email";

		public const string FILES = "files";

		public const string FORGETPASSWORD = "forgetpassword";

		public const string HOLIDAYS = "holidays";

		public const string JOBTITLE = "jobtitle";

		public const string LANGUAGE = "language";

		public const string LOGINFO = "loginfo";

		public const string LOGINLOG = "loginlog";

		public const string MAXNUMBER = "maxnumber";

		public const string MEMBERS = "members";

		public const string MEMBERSTORULE = "memberstorule";

		public const string MODULEFORPROGRAM = "moduleforprogram";

		public const string MODULELIST = "modulelist";

		public const string OFFICETEMPLATE = "officetemplate";

		public const string ONLINEUSERS = "onlineusers";

		public const string ORGANIZATION = "organization";

		public const string OUTERUSERS = "outerusers";

		public const string PROFILES = "profiles";

		public const string PROGRAMLIST = "programlist";

		public const string RULES = "rules";

		public const string SYSTEMSETTING = "systemsetting";

		public const string TASK = "task";

		public const string TASKREPLY = "taskreply";

		public const string TICKETAUTH = "ticketauth";

		public const string TIPS = "tips";

		public const string TIPSBAK = "tipsbak";

		public const string EXHIBITIONRULES = "exhibitionrules";

		public const string NEWS = "news";

		public const string PACKINGORDER = "packingorder";

		public const string TRACKINGLOG = "trackinglog";

		public const string WEBSITEFILES = "websitefiles";

		public const string WEBSITEMAILLOG = "websitemaillog";

		public const string WEBSITESETTING = "websitesetting";

		public const string OVW_IMPORTCUSTOMERS = "ovw_importcustomers";

		public const string OVW_BILLINFO = "ovw_billinfo";

		public const string OVW_BILLS = "ovw_bills";

		/// <summary>
		/// get the entity object
		/// </summary>
		/// <param name="type"/>type{String}</param>
		/// <returns>entity{Object}entity</returns>
		public static object GetEntity(string type)
		{
				var entity = new object();
				entity = "";
				switch (type.ToLower())
				{

                case COMPLAINT:

                        entity = new OTB_CRM_Complaint();

                        break;

                case BUSINESSOPPORTUNITY:

                        entity = new OTB_CRM_BusinessOpportunity();    

                        break;
				case CUSTOMERS:

						entity = new OTB_CRM_Customers();

						break;
				case CUSTOMERSTRANSFER:

						entity = new OTB_CRM_CustomersTransfer();

						break;
				case CUSTOMERSTRANSFERBAK:

						entity = new OTB_CRM_CustomersTransferBak();

						break;
				case IMPORTCUSTOMERS:

						entity = new OTB_CRM_ImportCustomers();

						break;
				case ATTENDANCE:

						entity = new OTB_EIP_Attendance();

						break;
				case ATTENDANCEDIFF:

						entity = new OTB_EIP_AttendanceDiff();

						break;
				case BILLCHANGEAPPLY:

						entity = new OTB_EIP_BillChangeApply();

						break;
				case BUSINESSTRAVEL:

						entity = new OTB_EIP_BusinessTravel();

						break;
				case CHECKFLOW:

						entity = new OTB_EIP_CheckFlow();

						break;
				case INVOICEAPPLYINFO:

						entity = new OTB_EIP_InvoiceApplyInfo();

						break;
				case LEAVE:

						entity = new OTB_EIP_Leave();

						break;
				case LEAVESET:

						entity = new OTB_EIP_LeaveSet();

						break;
				case OVERTIME:

						entity = new OTB_EIP_OverTime();

						break;
				case TRAVELEXPENSE:

						entity = new OTB_EIP_TravelExpense();

						break;
				case WENZHONG:

						entity = new OTB_EIP_WenZhong();

						break;
				case BILLINFO:

						entity = new OTB_OPM_BillInfo();

						break;
				case BILLS:

						entity = new OTB_OPM_Bills();

						break;
				case BILLSBAK:

						entity = new OTB_OPM_BillsBak();

						break;
				case EXHIBITION:

						entity = new OTB_OPM_Exhibition();

						break;
				case EXHIBITIONSTRANSFER:

						entity = new OTB_OPM_ExhibitionsTransfer();

						break;
				case EXHIBITIONSTRANSFERBAK:

						entity = new OTB_OPM_ExhibitionsTransferBak();

						break;
				case EXPORTEXHIBITION:

						entity = new OTB_OPM_ExportExhibition();

						break;
				case IMPORTEXHIBITION:

						entity = new OTB_OPM_ImportExhibition();

						break;
				case OTHEREXHIBITION:

						entity = new OTB_OPM_OtherExhibition();

						break;
				case OTHEREXHIBITIONTG:

						entity = new OTB_OPM_OtherExhibitionTG();

						break;
				case ANNOUNCEMENT:

						entity = new OTB_SYS_Announcement();

						break;
				case READ:

						entity = new OTB_SYS_Announcement_Read();

						break;
				case ARGUMENTCLASS:

						entity = new OTB_SYS_ArgumentClass();

						break;
				case ARGUMENTS:

						entity = new OTB_SYS_Arguments();

						break;
				case ARGUMENTSRELATED:

						entity = new OTB_SYS_ArgumentsRelated();

						break;
				case AUTHORIZE:

						entity = new OTB_SYS_Authorize();

						break;
				case AUTHORIZEFORDEPT:

						entity = new OTB_SYS_AuthorizeForDept();

						break;
				case AUTHORIZEFORMEMBER:

						entity = new OTB_SYS_AuthorizeForMember();

						break;
				case CALENDAR:

						entity = new OTB_SYS_Calendar();

						break;
				case CLOCKTIPS:

						entity = new OTB_SYS_ClockTips();

						break;
				case CLOCKTIPSBAK:

						entity = new OTB_SYS_ClockTipsBak();

						break;
				case DEPARTMENTS:

						entity = new OTB_SYS_Departments();

						break;
				case DOCUMENT:

						entity = new OTB_SYS_Document();

						break;
				case EMAIL:

						entity = new OTB_SYS_Email();

						break;
				case FILES:

						entity = new OTB_SYS_Files();

						break;
				case FORGETPASSWORD:

						entity = new OTB_SYS_ForgetPassword();

						break;
				case HOLIDAYS:

						entity = new OTB_SYS_Holidays();

						break;
				case JOBTITLE:

						entity = new OTB_SYS_Jobtitle();

						break;
				case LANGUAGE:

						entity = new OTB_SYS_Language();

						break;
				case LOGINFO:

						entity = new OTB_SYS_LogInfo();

						break;
				case LOGINLOG:

						entity = new OTB_SYS_LoginLog();

						break;
				case MAXNUMBER:

						entity = new OTB_SYS_MaxNumber();

						break;
				case MEMBERS:

						entity = new OTB_SYS_Members();

						break;
				case MEMBERSTORULE:

						entity = new OTB_SYS_MembersToRule();

						break;
				case MODULEFORPROGRAM:

						entity = new OTB_SYS_ModuleForProgram();

						break;
				case MODULELIST:

						entity = new OTB_SYS_ModuleList();

						break;
				case OFFICETEMPLATE:

						entity = new OTB_SYS_OfficeTemplate();

						break;
				case ONLINEUSERS:

						entity = new OTB_SYS_OnlineUsers();

						break;
				case ORGANIZATION:

						entity = new OTB_SYS_Organization();

						break;
				case OUTERUSERS:

						entity = new OTB_SYS_OuterUsers();

						break;
				case PROFILES:

						entity = new OTB_SYS_Profiles();

						break;
				case PROGRAMLIST:

						entity = new OTB_SYS_ProgramList();

						break;
				case RULES:

						entity = new OTB_SYS_Rules();

						break;
				case SYSTEMSETTING:

						entity = new OTB_SYS_SystemSetting();

						break;
				case TASK:

						entity = new OTB_SYS_Task();

						break;
				case TASKREPLY:

						entity = new OTB_SYS_TaskReply();

						break;
				case TICKETAUTH:

						entity = new OTB_SYS_TicketAuth();

						break;
				case TIPS:

						entity = new OTB_SYS_Tips();

						break;
				case TIPSBAK:

						entity = new OTB_SYS_TipsBak();

						break;
				case EXHIBITIONRULES:

						entity = new OTB_WSM_ExhibitionRules();

						break;
				case NEWS:

						entity = new OTB_WSM_News();

						break;
				case PACKINGORDER:

						entity = new OTB_WSM_PackingOrder();

						break;
				case TRACKINGLOG:

						entity = new OTB_WSM_TrackingLog();

						break;
				case WEBSITEFILES:

						entity = new OTB_WSM_WebSiteFiles();

						break;
				case WEBSITEMAILLOG:

						entity = new OTB_WSM_WebSiteMailLog();

						break;
				case WEBSITESETTING:

						entity = new OTB_WSM_WebSiteSetting();

						break;
				case OVW_IMPORTCUSTOMERS:

						entity = new OVW_CRM_ImportCustomers();

						break;
				case OVW_BILLINFO:

						entity = new OVW_OPM_BillInfo();

						break;
				case OVW_BILLS:

						entity = new OVW_OPM_Bills();

						break;

				}
				return entity;
		}
	}
}

