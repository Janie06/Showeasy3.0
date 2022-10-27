using System;

namespace EasyBL
{
    public class ProfitInfo
    {
        #region BasicInfo
        /// <summary>
        /// Guid
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// 帳單號碼
        /// </summary>
        public string BillNO { get; set; }

        /// <summary>
        /// 展覽簡稱
        /// </summary>
        public string ExhibitionName { get; set; }

        /// <summary>
        /// 客戶簡稱
        /// </summary>
        public string CustomerName { get; set; }

        /// <summary>
        /// 業務員
        /// </summary>
        public string MemberID { get; set; }

        #endregion
        #region 帳款部分

        /// <summary>
        /// 收入(A)[帳單:未稅金額欄位]
        /// </summary>
        public decimal BillUntaxAmt { get; set; }
        /// <summary>
        /// (權重)實際成本(B):一筆帳單實際成本=（整票貨總成本/整票貨CBM）＊單家廠商CBM
        /// </summary>
        public decimal SharedActualCost { get; set; }
        /// <summary>
        /// 毛利(C) = A-B
        /// </summary>
        public decimal GrossProfit
        {
            get
            {
                return BillUntaxAmt - Math.Abs(SharedActualCost);
            }
        }

        /// <summary>
        /// 毛利率(C)/(A)
        /// </summary>
        public decimal GrossProfitPercent
        {
            get
            {
                if (BillUntaxAmt == 0)
                    return 0;
                return GrossProfit / BillUntaxAmt;
            }
        }

        /// <summary>
        /// 帳單代墊款D:bill項目(TE188，TG188)
        /// </summary>
        public decimal BillReimburseAmount { get; set; }
        /// <summary>
        /// 實際代墊款E:成本裡面的99-12、99-16等
        /// </summary>
        public decimal ActualBillReimburseAmount { get; set; }

        /// <summary>
        /// 淨收入(F) =(A)-(D)
        /// </summary>
        public decimal NetIncome
        {
            get
            {
                return (BillUntaxAmt - Math.Abs(BillReimburseAmount));
            }
        }

        /// <summary>
        /// 淨成本(G) =(B)-(E)
        /// </summary>
        public decimal NetCost
        {
            get
            {
                return (SharedActualCost - Math.Abs(ActualBillReimburseAmount));
            }
        }

        /// <summary>
        /// 淨毛利(F)= F-G  or [(A-D)]-[(B)-(E)]
        /// </summary>
        public decimal NetProfit
        {
            get
            {
                return NetIncome - Math.Abs(NetCost);
            }
        }
        /// <summary>
        /// 淨毛利率 (H)/(F)
        /// </summary>
        public decimal NetProfitPercent
        {
            get
            {
                if (NetIncome == 0)
                    return 0;
                return NetProfit / NetIncome;
            }
        }

        /// <summary>
        /// 公斤
        /// </summary>
        public decimal Weight { set; get; }
        /// <summary>
        /// 單位CBM
        /// </summary>
        public decimal Volume { set; get; }

        public long OrderValue { set; get; }

        /// <summary>
        /// 各種值
        /// </summary>
        public object ExField { set; get; }
        #endregion
    }
}