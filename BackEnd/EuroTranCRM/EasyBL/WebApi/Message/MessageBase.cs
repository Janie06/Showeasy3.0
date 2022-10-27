using System.Collections.Generic;

namespace EasyBL.WebApi.Message
{
    /// <summary>
    /// Base of all message
    /// </summary>
    public class MessageBase
    {
        public MessageBase()
        {
            DATA = new Dictionary<string, object>();
        }

        /// <summary>
        /// 輔助Plugin Routing
        /// </summary>
        public string PROJECT { get; set; }

        /// <summary>
        /// 輔助Plugin Routing
        /// </summary>
        public string PROJECTVER { get; set; }

        /// <summary>
        /// 區分此Message主要用途分類
        /// </summary>
        public string MODULE { get; set; }

        /// 區分此Message次要用途分類 </summary>
        public string TYPE { get; set; }

        public Dictionary<string, object> DATA { get; set; }
    }
}