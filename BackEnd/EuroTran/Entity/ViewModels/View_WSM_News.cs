using Entity.Sugar;

namespace Entity.ViewModels
{
    public class View_WSM_News : OTB_WSM_News
    {
        public string News_TypeName { get; set; }
        public string News_LanguageTypeName { get; set; }
        public int OrderCount { get; set; }
    }
}