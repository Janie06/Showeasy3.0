using Entity.Sugar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euro.Transfer.Model
{
    public class ClockTipsInfo : OTB_SYS_ClockTips
    {
        /// <summary>
        /// 系統網頁類別
        /// </summary>
        public int MsgType { get; set; }
        /// <summary>
        /// 頁面跳轉參數
        /// </summary>
        public string Parm { get; set; }
        /// <summary>
        /// 推送多人集合
        /// </summary>
        public IList<string> UserIds { get; set; }
    }
}
