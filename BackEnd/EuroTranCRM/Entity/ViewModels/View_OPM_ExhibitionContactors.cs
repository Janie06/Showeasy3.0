using Entity.Sugar;

namespace Entity.ViewModels
{
    public class View_OPM_ExhibitionContactors : OTB_OPM_ExhibitionContactors
    {
        public string guid { get; set; }
        public string ContactorName { get; set; }
        public string IsAudit { get; set; }
    }
}