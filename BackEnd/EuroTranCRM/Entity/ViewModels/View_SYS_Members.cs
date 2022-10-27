using Entity.Sugar;

namespace Entity.ViewModels
{
    public class View_SYS_Members : OTB_SYS_Members
    {
        public string DepartmentName { get; set; }
        public string JobtitleName { get; set; }
        public string RuleIDs { get; set; }
        public string ImmediateSupervisorName { get; set; }
    }
}