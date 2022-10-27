using Entity.Sugar;
using System.Collections.Generic;

namespace Entity.ViewModels
{
    public class View_WSM_ExhibitionRules : OTB_WSM_ExhibitionRules
    {
        public View_WSM_ExhibitionRules()
        {
            CurrencyName = "";
            Files = new List<OTB_SYS_Files>();
        }

        public List<OTB_SYS_Files> Files { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyName_EN { get; set; }
    }
}