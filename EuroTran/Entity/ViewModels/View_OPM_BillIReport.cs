namespace Entity.ViewModels
{
    public class View_OPM_BillIReport
    {
        public string BillNO { get; set; }
        public string BillType { get; set; }
        public string ParentId { get; set; }
        public string ProjectNumber { get; set; }
        public string CustomerCode { get; set; }
        public string CustomerName { get; set; }

        public string ResponsiblePerson { get; set; }
        public string Currency { get; set; }
        public string ExchangeRate { get; set; }
        public decimal? InCome { get; set; }
        public string IsReturn { get; set; }
        public string Weight { get; set; }
        public string Volume { get; set; }
        public string AuditVal { get; set; }
        public string OrgID { get; set; }
        public string ReFlow { get; set; }
        public string FeeItems { get; set; }
    }
}