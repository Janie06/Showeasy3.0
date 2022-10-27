using Entity.Sugar;

namespace Entity.ViewModels
{
    public class View_OPM_ExportExhibition : OTB_OPM_ExportExhibition
    {
        public string Exhibitioname_TW { get; set; }
        public string Exhibitioname_EN { get; set; }
        public string ExhibitioShotName_TW { get; set; }
        public string IsAlert { get; set; }
        public string ExhibitionDate { get; set; }
        public string OrganizerName { get; set; }
        public string AgentName { get; set; }
        public string ResponsiblePersonName { get; set; }
        public string TransportationModeName { get; set; }
        public string _DocumentDeadline { get; set; }
        public string _ClosingDate { get; set; }
        public string _ETC { get; set; }
        public string _ETD { get; set; }
        public string _ETA { get; set; }
        public string _ReminderAgentExecutionDate { get; set; }
        public string _PreExhibitionDate { get; set; }
        public string _ExitDate { get; set; }
    }
}