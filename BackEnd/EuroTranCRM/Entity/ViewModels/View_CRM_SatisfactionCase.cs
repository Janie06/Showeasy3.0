using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entity.Sugar;

namespace Entity.ViewModels
{
    public class View_CRM_SatisfactionCase:OTB_CRM_SatisfactionCase
    {
        private string cstrCustomerName = ""; // 客戶名稱
        private string cstrExhibitioname_TW = ""; // 展覽名稱
        private string cstrResponsiblePerson = ""; // 展覽負責人

        /// <summary>
        /// 客戶名稱
        /// </summary>
        public string CustomerName
        {
            get { return cstrCustomerName; }
            set { cstrCustomerName = value; }
        }

        /// <summary>
        /// 展覽名稱
        /// </summary>
        public string Exhibitioname_TW
        {
            get { return cstrExhibitioname_TW; }
            set { cstrExhibitioname_TW = value; }
        }

        /// <summary>
        /// 展覽負責人
        /// </summary>
        public string ResponsiblePerson
        {
            get { return cstrResponsiblePerson; }
            set { cstrResponsiblePerson = value; }
        }

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? ExhibitionDateStart { get; set; }
        public const string CN_EXHIBITIONDATESTART = "ExhibitionDateStart";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>           
        public DateTime? ExhibitionDateEnd { get; set; }
        public const string CN_EXHIBITIONDATEEND = "ExhibitionDateEnd";

        public string ExhibitioShotName_TW { get; set; }

        public string Feild01 { get; set; }
        public string CustomerSN { get; set; }
    }
}
