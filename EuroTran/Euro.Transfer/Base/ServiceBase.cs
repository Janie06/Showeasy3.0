using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Euro.Transfer
{
    public class ServiceBase
    {
        //log存放路徑
        public static string Errorlog_Path = Common.GetAppSetting("Errorlog_location");
        public static string Debuglog_Path = Common.GetAppSetting("Debuglog_location");

        #region JsonToString
        /// <summary>
        /// 轉換json字串
        /// </summary>
        /// <param name="o">要轉化的物件</param>
        public static string JsonToString(object o)
        {
            if (o != null)
            {
                return JsonConvert.SerializeObject(o, Formatting.Indented);//序列化，將物件轉化為字符串
            }
            return "";
        }
        #endregion
    }
    public class KeyValue
    {
        public string key { get; set; }
        public string value { get; set; }
    }
}
