using SqlSugar;
using System;
using System.Collections.Generic;

namespace EasyBL
{
    public class ExpInfo
    {
        public string ExhibitionType { get; set; }
        public string Id { get; set; }
        public string Bills { get; set; }
        public string ReturnBills { get; set; }
        public string ActualCost { get; set; }
        public string RefNumber { get; set; }

        /// <summary>
        /// 展覽代碼
        /// </summary>
        public string ExpNO { get; set; }

        /// <summary>
        /// 代理
        /// </summary>
        public string Agent { get; set; }

        /// <summary>
        /// 參展廠商，OTB_OPM_OtherExhibitionTG、OTB_OPM_ExportExhibition使用
        /// </summary>
        public string Exhibitors { get; set; }

        /// <summary>
        /// 參展廠商，OTB_OPM_OtherExhibition、OTB_OPM_ImportExhibition使用
        /// </summary>
        public string Supplier { get; set; }

        /// <summary>
        /// 組團單位，僅出口OTB_OPM_ExportExhibition使用
        /// </summary>
        public string Organizer { get; set; }

        /// <summary>
        /// 負責人員
        /// </summary>
        public string ResponsibleMember { get; set; }
        /// <summary>
        /// 負責人員部門
        /// </summary>
        public string DeptOfResponsibleMember { get; set; }

        public string OrgID { get; set; }
    }
}