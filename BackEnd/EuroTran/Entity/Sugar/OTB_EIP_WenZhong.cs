using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_EIP_WenZhong")]
    public partial class OTB_EIP_WenZhong : ModelContext
    {
           public OTB_EIP_WenZhong(){


           }
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
           /// Nullable:False
           /// </summary>           
           public string OrgID {get;set;}
           public const string CN_ORGID = "OrgID";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string UserID {get;set;}
           public const string CN_USERID = "UserID";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string WenZhongAcount {get;set;}
           public const string CN_WENZHONGACOUNT = "WenZhongAcount";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:False
           /// </summary>           
           public string UserName {get;set;}
           public const string CN_USERNAME = "UserName";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal? Seniority {get;set;}
           public const string CN_SENIORITY = "Seniority";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? EnableDate {get;set;}
           public const string CN_ENABLEDATE = "EnableDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public DateTime? ExpirationDate {get;set;}
           public const string CN_EXPIRATIONDATE = "ExpirationDate";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal? PaymentHours {get;set;}
           public const string CN_PAYMENTHOURS = "PaymentHours";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal? UsedHours {get;set;}
           public const string CN_USEDHOURS = "UsedHours";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal? RemainHours {get;set;}
           public const string CN_REMAINHOURS = "RemainHours";

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
           /// Desc:是否有被任何資料使用，例如:請假使用特休部分，就不能被刪除。
           /// Default:
           /// Nullable:True
           /// </summary>           
           public bool? DelFlag {get;set;}
           public const string CN_DELFLAG = "DelFlag";


            /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public bool Notice { get;set;}
           public const string CN_NOTICE = "Notice";

    }
}
