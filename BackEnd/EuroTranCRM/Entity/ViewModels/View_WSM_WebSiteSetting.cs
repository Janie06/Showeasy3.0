using Entity.Sugar;
using System.Collections.Generic;

namespace Entity.ViewModels
{
    public class View_WSM_WebSiteSetting : OTB_WSM_WebSiteSetting
    {
        public View_WSM_WebSiteSetting()
        {
            Infos = new List<View_WSM_WebSiteSetting>();
        }

        public string IconFileName { get; set; }
        public string IconFilePath { get; set; }
        public string SubIconFileName { get; set; }
        public string SubIconFilePath { get; set; }
        public string CoverFileName { get; set; }
        public string CoverPath { get; set; }
        public int OrderCount { get; set; }
        public List<View_WSM_WebSiteSetting> Infos { get; set; }
    }
}