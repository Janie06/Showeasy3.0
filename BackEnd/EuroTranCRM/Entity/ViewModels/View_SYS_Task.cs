using Entity.Sugar;

namespace Entity.ViewModels
{
    public class View_SYS_Task : OTB_SYS_Task
    {
        public string OwnerName { get; set; }
        public string ProgressShow { get; set; }
        public string ImportantName { get; set; }
        public string SourceFromName { get; set; }
    }
}