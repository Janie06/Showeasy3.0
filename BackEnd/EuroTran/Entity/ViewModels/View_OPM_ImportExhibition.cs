using Entity.Sugar;

namespace Entity.ViewModels
{
    public class View_OPM_ImportExhibition : OTB_OPM_ImportExhibition
    {
        public string Exhibitioname_TW { get; set; }
        public string Exhibitioname_EN { get; set; }
        public string ExhibitioShotName_TW { get; set; }
        public string SupplierCName { get; set; }
        public string SupplierEName { get; set; }
        public string AgentName { get; set; }
        public string ResponsiblePersonName { get; set; }
        public string DeclarationClassName { get; set; }
        public string TransportationModeName { get; set; }
        public string CustomsClearanceName { get; set; }
        public string ImportPersonName { get; set; }
        public string IsAlert { get; set; }
        public string ExhibitionDate { get; set; }
        public string _ArrivalTime { get; set; }
        public string _FreePeriod { get; set; }
        public string _ApproachTime { get; set; }
        public string _ExitTime { get; set; }
    }
}