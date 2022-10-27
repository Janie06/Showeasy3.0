using Entity.Sugar;

namespace Entity.ViewModels
{
    public class View_CRM_Contactors : OTB_CRM_Contactors
    {
        public string UniCode { get; set; }
        public string CustomerShotCName { get; set; }
        public string CustomerEName { get; set; }
        public string IsMain { get; set; }
        public string SourceType { get; set; }
        public string SN { get; set; }
        public string CustomerCName { get; set; }
        public int? OrderCount { get; set; }
    }
}