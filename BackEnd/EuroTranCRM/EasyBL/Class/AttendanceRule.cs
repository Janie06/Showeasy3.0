using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EasyBL
{
    public class AttendanceRule
    {
        public string OrgID { set; get; }
        public string WorkStart { set; get; }
        public string WorkEnd { set; get; }
        /// <summary>
        /// 最晚上班時間，時間不認定遲到
        /// </summary>
        public string WorkStartBuffer { set; get; }


        /// <summary>
        /// 遲到緩衝，遲到過後扣除WorkStartBuffer量
        /// </summary>
        public int DelayBuffer { set; get; }


        /// <summary>
        /// 上午上下班時間
        /// </summary>
        public string WorkTimePMDeadLine { set; get; }
        /// <summary>
        /// 下午上下班時間
        /// </summary>
        public string WorkTimeAMDeadLine { set; get; }


        public AttendanceRule()
        {
            OrgID = "";
            WorkTimePMDeadLine = "09:00~12:30";
            WorkTimeAMDeadLine = "13:30~17:30";
            WorkStart = "09:00";
            WorkEnd = "17:30";
            WorkStartBuffer = "";
            DelayBuffer = 0;
        }

    }
}
