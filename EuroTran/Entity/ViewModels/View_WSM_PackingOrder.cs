using Entity.Sugar;

namespace Entity.ViewModels
{
    public class View_WSM_PackingOrder : OTB_WSM_PackingOrder
    {
        public string Exhibitioname_TW { get; set; }
        public string Exhibitioname_EN { get; set; }
        public bool? IsFormal { get; set; }
    }
}