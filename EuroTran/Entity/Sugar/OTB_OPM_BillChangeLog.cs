using Entity.Sugar;
using System;

namespace Entity.Sugar
{
    public class OTB_OPM_BillChangeLog
    {

        public string OrgID { get; set; }
        public int? SN { get; set; }
        public string BillNO { get; set; }
        public string ExhibitioName { get; set; }
        public string PayerName { get; set; }
        public string ResponsiblePersonName { get; set; }
        public string Currency { get; set; }
        public string ExchangeRate { get; set; }
        /// <summary>
        /// 預收
        /// </summary>
        public string Advance { get; set; }
        /// <summary>
        /// 未稅金額
        /// </summary>
        public string AmountSum { get; set; }
        /// <summary>
        /// 稅金
        /// </summary>
        public string TaxSum { get; set; }
        /// <summary>
        /// 合計(未稅金額+稅金)
        /// </summary>
        public string AmountTaxSum { get; set; }
        /// <summary>
        /// 總應收
        /// </summary>
        public string TotalReceivable { get; set; }
        /// <summary>
        /// 建立進出口單據人員
        /// </summary>
        public string OpmBillCreateUserName { get; set; }
        public string Operation { get; set; }
        public string ModifyUser { get; set; }
        public DateTime? ModifyDate { get; set; }
    }
}