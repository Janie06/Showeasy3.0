using Entity.Sugar;

namespace Entity.ViewModels
{
    public class View_CRM_Complaint : OTB_CRM_Complaint
    {
        public string ExhibitionName { get; set; }
        public const string CN_ExhibitionName = "ExhibitionName";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string ExhibitioShotName_TW { get; set; }
        public const string CN_EXHIBITIONSHOTNAME_TW = "ExhibitioShotName_TW";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string CustomerCName { get; set; }
        public const string CN_CUSTOMERCNAME = "CustomerCName";

        /// <summary>
        /// Desc:
        /// Default:
        /// Nullable:True
        /// </summary>
        public string DepartmentName { get; set; }
        public const string CN_DEPARTMENTNAME = "DepartmentName";
    }
}
