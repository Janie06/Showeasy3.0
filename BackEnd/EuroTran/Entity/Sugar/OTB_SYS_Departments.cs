using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_SYS_Departments")]
    public partial class OTB_SYS_Departments : ModelContext
    {
           public OTB_SYS_Departments(){


           }
           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true)]
           public string DepartmentID {get;set;}
           public const string CN_DEPARTMENTID = "DepartmentID";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string DepartmentName {get;set;}
           public const string CN_DEPARTMENTNAME = "DepartmentName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string DepartmentShortName {get;set;}
           public const string CN_DEPARTMENTSHORTNAME = "DepartmentShortName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string ChiefOfDepartmentID {get;set;}
           public const string CN_CHIEFOFDEPARTMENTID = "ChiefOfDepartmentID";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? LevelOfDepartment {get;set;}
           public const string CN_LEVELOFDEPARTMENT = "LevelOfDepartment";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string NameOfLevel {get;set;}
           public const string CN_NAMEOFLEVEL = "NameOfLevel";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ParentDepartmentID {get;set;}
           public const string CN_PARENTDEPARTMENTID = "ParentDepartmentID";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public int? OrderByValue {get;set;}
           public const string CN_ORDERBYVALUE = "OrderByValue";

           /// <summary>
           /// Desc:Y:有效 N:無效
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
           /// Default:N
           /// Nullable:True
           /// </summary>           
           public string DelStatus {get;set;}
           public const string CN_DELSTATUS = "DelStatus";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           [SugarColumn(IsPrimaryKey=true)]
           public string OrgID {get;set;}
           public const string CN_ORGID = "OrgID";

    }
}
