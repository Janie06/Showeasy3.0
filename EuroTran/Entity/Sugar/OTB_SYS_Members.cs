using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_SYS_Members")]
    public partial class OTB_SYS_Members : ModelContext
    {
           public OTB_SYS_Members(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true)]
           public string MemberID {get;set;}
           public const string CN_MEMBERID = "MemberID";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Password {get;set;}
           public const string CN_PASSWORD = "Password";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string MemberName {get;set;}
           public const string CN_MEMBERNAME = "MemberName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ServiceCode {get;set;}
           public const string CN_SERVICECODE = "ServiceCode";

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
           public string GoogleAccount {get;set;}
           public const string CN_GOOGLEACCOUNT = "GoogleAccount";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ContectTEL {get;set;}
           public const string CN_CONTECTTEL = "ContectTEL";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ContectExt {get;set;}
           public const string CN_CONTECTEXT = "ContectExt";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ContectFax {get;set;}
           public const string CN_CONTECTFAX = "ContectFax";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ContectCell {get;set;}
           public const string CN_CONTECTCELL = "ContectCell";

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
           public DateTime? BirthDate {get;set;}
           public const string CN_BIRTHDATE = "BirthDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? ArriveDate {get;set;}
           public const string CN_ARRIVEDATE = "ArriveDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string EmergencyContect {get;set;}
           public const string CN_EMERGENCYCONTECT = "EmergencyContect";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string EmergencyTEL {get;set;}
           public const string CN_EMERGENCYTEL = "EmergencyTEL";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string EmergencyExt {get;set;}
           public const string CN_EMERGENCYEXT = "EmergencyExt";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string EmergencyFax {get;set;}
           public const string CN_EMERGENCYFAX = "EmergencyFax";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string EmergencyCell {get;set;}
           public const string CN_EMERGENCYCELL = "EmergencyCell";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string EmergencyEMail {get;set;}
           public const string CN_EMERGENCYEMAIL = "EmergencyEMail";

           /// <summary>
           /// Desc:與SYS_ Jobtitle關聯
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string JobTitle {get;set;}
           public const string CN_JOBTITLE = "JobTitle";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string JobClass {get;set;}
           public const string CN_JOBCLASS = "JobClass";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ChiefID {get;set;}
           public const string CN_CHIEFID = "ChiefID";

           /// <summary>
           /// Desc:與SYS_ Department關聯
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
           public DateTime? LeaveDate {get;set;}
           public const string CN_LEAVEDATE = "LeaveDate";

           /// <summary>
           /// Desc:Y：帳號有效 N：帳號無效
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
           public string CalColor {get;set;}
           public const string CN_CALCOLOR = "CalColor";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Skype {get;set;}
           public const string CN_SKYPE = "Skype";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Line {get;set;}
           public const string CN_LINE = "Line";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Wechat {get;set;}
           public const string CN_WECHAT = "Wechat";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string MemberPic {get;set;}
           public const string CN_MEMBERPIC = "MemberPic";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string QQ {get;set;}
           public const string CN_QQ = "QQ";

           /// <summary>
           /// Desc:
           /// Default:M
           /// Nullable:True
           /// </summary>           
           public string SysShowMode {get;set;}
           public const string CN_SYSSHOWMODE = "SysShowMode";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string Country {get;set;}
           public const string CN_COUNTRY = "Country";

           /// <summary>
           /// Desc:
           /// Default:0
           /// Nullable:False
           /// </summary>           
           public bool NetworkLogin {get;set;}
           public const string CN_NETWORKLOGIN = "NetworkLogin";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ImmediateSupervisor {get;set;}
           public const string CN_IMMEDIATESUPERVISOR = "ImmediateSupervisor";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true)]
           public string OrgID {get;set;}
           public const string CN_ORGID = "OrgID";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string WenZhongAcount {get;set;}
           public const string CN_WENZHONGACOUNT = "WenZhongAcount";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CardId {get;set;}
           public const string CN_CARDID = "CardId";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public bool? IsAttendance {get;set;}
           public const string CN_ISATTENDANCE = "IsAttendance";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string OutlookAccount {get;set;}
           public const string CN_OUTLOOKACCOUNT = "OutlookAccount";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string NickName {get;set;}
           public const string CN_NICKNAME = "NickName";

    }
}
