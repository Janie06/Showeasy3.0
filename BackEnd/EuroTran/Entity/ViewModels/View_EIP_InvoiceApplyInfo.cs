using Entity.Sugar;

namespace Entity.ViewModels
{
    public class View_EIP_InvoiceApplyInfo : OTB_EIP_InvoiceApplyInfo
    {
        public string ApplicantName { get; set; }
        public string Handle_PersonName { get; set; }
        public string PayeeCode { get; set; }
        public string CustomerNO { get; set; }
        public string DeptName { get; set; }
    }
}