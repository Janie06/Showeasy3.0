using Entity.Sugar;

namespace Entity.ViewModels
{
    public class View_OPM_BillInfo : OTB_OPM_BillInfo
    {
        public string ExhibitioShotName { get; set; }
        public string ExhibitioName { get; set; }
        public string PayerName { get; set; }
        public string ResponsiblePersonName { get; set; }
        public decimal _ExchangeRate { get; set; }
        public decimal _Advance { get; set; }
        public decimal _AmountSum { get; set; }
        public decimal _TaxSum { get; set; }
        public decimal _AmountTaxSum { get; set; }
        public decimal _TotalReceivable { get; set; }
    }
}