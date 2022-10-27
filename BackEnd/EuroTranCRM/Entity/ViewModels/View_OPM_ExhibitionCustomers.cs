using System;
using Entity.Sugar;

namespace Entity.ViewModels
{
    public class View_OPM_ExhibitionCustomers : OTB_OPM_ExhibitionCustomers
    {
        public string guid { get; set; }
        public string CustomerNO { get; set; }
        public string CustomerCName { get; set; }
        public string CustomerEName { get; set; }
        public string CustomerShotCName { get; set; }
        public string ContactorName { get; set; }
        public string Telephone1 { get; set; }
        public string IsFormal { get; set; }
        public string Telephone { get; set; }
        public string FAX { get; set; }
        public string Address { get; set; }
        public string IsAudit { get; set; }
        public string UniCode { get; set; }
        public string ListSourceName { get; set; }
        public string CalloutLog { get; set; }
        public string ExhibitionName { get; set; }
        public string ContactorId { get; set; }
        public string Email { get; set; }
        public DateTime? ExhibitionDateStart { get; set; }
        public string Ext { get; set; }
        public string IsDeal { get; set; }
        public string IsImporter { get; set; }
    }
}