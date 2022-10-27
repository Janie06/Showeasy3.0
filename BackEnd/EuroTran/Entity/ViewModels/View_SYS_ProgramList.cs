using Entity.Sugar;

namespace Entity.ViewModels
{
    public class View_SYS_ProgramList : OTB_SYS_ProgramList
    {
        public string ModuleName { get; set; }
        public int? ModOrderBy { get; set; }
        public string AllModuleID { get; set; }
        public string ProgramTypeName { get; set; }
        public string EffectiveName { get; set; }
        public string ShowInListName { get; set; }
        public int? OrderByValue { get; set; }
        public int OrderCount { get; set; }
    }
}