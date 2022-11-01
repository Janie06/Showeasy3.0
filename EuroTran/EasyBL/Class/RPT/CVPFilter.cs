using SqlSugar;
using System;

namespace EasyBL
{
    public class CVPFilter
    {
        #region BillType
        ///0:未審核、1:審核中、2:已審核、3:不通過、4:已銷帳、5:已過帳、6:已作廢、7:抽單中                                      
        public string PassStatus { set; get; }
        public string sBillAuditDateStart { set; get; }
        public string sBillAuditDateEnd { set; get; }
        public string sBillWriteOffDateStart { set; get; }
        public string sBillWriteOffDateEnd { set; get; }

        public DateTime rBillAuditDateStart { set; get; }
        public DateTime rBillAuditDateEnd { set; get; }
        public DateTime rBillWriteOffDateStart { set; get; }
        public DateTime rBillWriteOffDateEnd { set; get; }

        #endregion

        #region ExhibitionType
        public string sExhibitionDateStart { set; get; }
        public string sExhibitionDateEnd { set; get; }
        public DateTime rExhibitionDateStart { set; get; }
        public DateTime rExhibitionDateEnd { set; get; }
        #endregion
        public CVPFilter()
        {
            PassStatus = "2,4,5";
        }

        public void SetBill(string BillAuditDateStart, string BillAuditDateEnd, string BillWriteOffDateStart, string BillWriteOffDateEnd, string BillStatus = "")
        {
            sBillAuditDateStart = BillAuditDateStart;
            sBillAuditDateEnd = BillAuditDateEnd;
            sBillWriteOffDateStart = BillWriteOffDateStart;
            sBillWriteOffDateEnd = BillWriteOffDateEnd;
            rBillAuditDateStart = new DateTime();
            rBillAuditDateEnd = new DateTime();
            rBillWriteOffDateStart = new DateTime();
            rBillWriteOffDateEnd = new DateTime();

            var AuditStatus = false;
            var WriteOffStatus = false;
            if (!string.IsNullOrEmpty(sBillAuditDateStart))
            {
                AuditStatus = true;
                rBillAuditDateStart = SqlFunc.ToDate(sBillAuditDateStart);
            }
            if (!string.IsNullOrEmpty(sBillAuditDateEnd))
            {
                AuditStatus = true;
                rBillAuditDateEnd = SqlFunc.ToDate(sBillAuditDateEnd).AddDays(1);
            }
            if (!string.IsNullOrEmpty(sBillWriteOffDateStart))
            {
                WriteOffStatus = true;
                rBillWriteOffDateStart = SqlFunc.ToDate(sBillWriteOffDateStart);
            }
            if (!string.IsNullOrEmpty(sBillWriteOffDateEnd))
            {
                WriteOffStatus = true;
                rBillWriteOffDateEnd = SqlFunc.ToDate(sBillWriteOffDateEnd).AddDays(1);
            }
            //status: 任意status => 若有AuditDate 則2,4,5 =>  若有WriteOff 則4,5
            PassStatus = string.IsNullOrWhiteSpace(BillStatus) ? "2,4,5" : BillStatus;
            if(AuditStatus)
                PassStatus = "2,4,5";
            if (WriteOffStatus) 
                PassStatus = "4,5";
        }

        public void SetExhibition(string ExhibitionDateStart, string ExhibitionDateEnd)
        {
            sExhibitionDateStart = ExhibitionDateStart;
            sExhibitionDateEnd = ExhibitionDateEnd;
            rExhibitionDateStart = new DateTime();
            rExhibitionDateEnd = new DateTime();

            if (!string.IsNullOrEmpty(sExhibitionDateStart))
            {
                rExhibitionDateStart = SqlFunc.ToDate(sExhibitionDateStart);
            }
            if (!string.IsNullOrEmpty(sExhibitionDateEnd))
            {
                rExhibitionDateEnd = SqlFunc.ToDate(sExhibitionDateEnd).AddDays(1);
            }
        }
    }
}