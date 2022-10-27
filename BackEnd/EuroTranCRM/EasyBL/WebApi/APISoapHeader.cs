namespace EasyBL.WebApi.WebApi
{
    /// <summary>

    /// WebService接口 SoapHeader类 </summary>
    public class APISoapHeader : System.Web.Services.Protocols.SoapHeader
    {
        /// <summary>
        /// token
        /// </summary>
        public string token { get; set; }

        /// <summary>
        /// 加密签名
        /// </summary>
        public string signature { get; set; }

        /// <summary>
        /// 时间戳
        /// </summary>
        public string timestamp { get; set; }

        /// <summary>
        /// 随机数
        /// </summary>
        public string nonce { get; set; }
    }
}