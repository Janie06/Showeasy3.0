using Entity.Sugar;

namespace Entity.ViewModels
{
    public class View_SYS_Arguments : OTB_SYS_Arguments
    {
        public string ArgumentClassName { get; set; }
        public string ParentArgumentName { get; set; }
        public int OrderCount { get; set; }
    }
}