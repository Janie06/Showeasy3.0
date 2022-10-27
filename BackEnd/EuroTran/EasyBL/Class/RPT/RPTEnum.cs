using System.ComponentModel;

namespace EasyBL
{
    public  enum RPTEnum
    {
        [Description("利潤明細表(依業務)")]
        CVPAnalysisBySaler,
        [Description("利潤明細表(依展覽)")]
        CVPAnalysisByExhibition,
        [Description("金流帳單")]
        CashFlow,
        [Description("貢獻度報表(代理)")]
        DegreeOfContributionByAgent,
        [Description("貢獻度報表(客戶) ")]
        DegreeOfContributionByCustomer,
        [Description("現有客戶資訊")]
        ExistingCustomer,
        [Description("展覽資訊")]
        ExhibitionInfo,
        [Description("成本費用報表")]
        CostFeeItemReport

    }
}