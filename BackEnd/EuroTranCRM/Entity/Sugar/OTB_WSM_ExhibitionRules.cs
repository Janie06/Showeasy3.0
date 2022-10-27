using System;
using System.Linq;
using System.Text;
using SqlSugar;

namespace Entity.Sugar
{
    ///<summary>
    ///
    ///</summary>
    [SugarTable("OTB_WSM_ExhibitionRules")]
    public partial class OTB_WSM_ExhibitionRules : ModelContext
    {
           public OTB_WSM_ExhibitionRules(){


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
           public string Title {get;set;}
           public const string CN_TITLE = "Title";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CostRules {get;set;}
           public const string CN_COSTRULES = "CostRules";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal? PackingPrice {get;set;}
           public const string CN_PACKINGPRICE = "PackingPrice";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal? FeedingPrice {get;set;}
           public const string CN_FEEDINGPRICE = "FeedingPrice";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public decimal? StoragePrice {get;set;}
           public const string CN_STORAGEPRICE = "StoragePrice";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string CostInstruction {get;set;}
           public const string CN_COSTINSTRUCTION = "CostInstruction";

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
           public string ServiceInstruction {get;set;}
           public const string CN_SERVICEINSTRUCTION = "ServiceInstruction";

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
           public string CostInstruction_EN {get;set;}
           public const string CN_COSTINSTRUCTION_EN = "CostInstruction_EN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string ServiceInstruction_EN {get;set;}
           public const string CN_SERVICEINSTRUCTION_EN = "ServiceInstruction_EN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string FileId_EN {get;set;}
           public const string CN_FILEID_EN = "FileId_EN";

           /// <summary>
           /// Desc:
           /// Default:
           /// Nullable:True
           /// </summary>           
           public string IsMerge {get;set;}
           public const string CN_ISMERGE = "IsMerge";

            /// <summary>
            /// Desc:
            /// Default:1
            /// Nullable:True
            /// </summary>           
            public double? FeedingRequiredMinCBM { get; set; }
            public const string CN_FEEDINGREQUIREDMINCBM = "FeedingRequiredMinCBM";

            /// <summary>
            /// Desc:
            /// Default:
            /// Nullable:True
            /// </summary>           
            public string FeedingMinMode { get; set; }
            public const string CN_FEEDINGMINMODE = "FeedingMinMode";

            /// <summary>
            /// Desc:
            /// Default:1
            /// Nullable:True
            /// </summary>           
            public double? PackingRequiredMinCBM { get; set; }
            public const string CN_PACKINGREQUIREDMINCBM = "PackingRequiredMinCBM";

            /// <summary>
            /// Desc:
            /// Default:
            /// Nullable:True
            /// </summary>           
            public string PackingMinMode { get; set; }
            public const string CN_PACKINGMINMODE = "PackingMinMode";

            public string Effective { get; set; }
    }
}
