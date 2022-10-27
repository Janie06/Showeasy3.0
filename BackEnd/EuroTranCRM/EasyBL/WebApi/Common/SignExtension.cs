using EasyBL.WebApi.WebApi;
using EasyNet;
using Entity.Sugar;
using SqlSugar.Base;
using System;
using System.Linq;
using System.Text;
using System.Web;

namespace EasyBL.WebApi.Common
{
    public class SignExtension
    {
        /// <summary>
        /// 驗證身份
        /// </summary>
        /// <param name="c">todo: describe c parameter on VerifyIdentity</param>
        /// <param name="header">todo: describe header parameter on VerifyIdentity</param>
        /// <returns></returns>
        public static bool VerifyIdentity(HttpContext c, APISoapHeader header)
        {
            var bValid = true;
            try
            {
                if (header == null)
                {
                    var sOrgid = c.Request.Headers["orgid"];
                    var sUserid = c.Request.Headers["userid"];
                    var sToken = c.Request.Headers["token"];
                    var sSignature = c.Request.Headers["signature"];

                    if (string.IsNullOrWhiteSpace(sSignature))
                    {
                        var oTicket = (OTB_SYS_TicketAuth)HttpRuntimeCache.Get(sOrgid + sUserid);
                        if (oTicket == null)
                        {
                            var db = SugarBase.GetIntance();
                            oTicket = db.Queryable<OTB_SYS_TicketAuth>().Single(it => it.Token == sToken);
                        }
                        if (oTicket == null || oTicket.Token != sToken || (oTicket.IsVerify == "Y" && oTicket.ExpireTime < DateTime.Now))
                        {
                            bValid = false;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrWhiteSpace(sSignature))
                        {
                            bValid = false;
                        }
                        else
                        {
                            var sTimestamp = c.Request.Headers["timestamp"];
                            var sNonce = c.Request.Headers["nonce"];
                            bValid = SignExtension.TokenVerify(sTimestamp, sNonce, sToken, sSignature);
                        }
                    }
                }
                return bValid;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// 验证加密签名
        /// </summary>
        /// <param name="timeStamp">todo: describe timeStamp parameter on TokenVerify</param>
        /// <param name="nonce">todo: describe nonce parameter on TokenVerify</param>
        /// <param name="token">todo: describe token parameter on TokenVerify</param>
        /// <param name="signature">todo: describe signature parameter on TokenVerify</param>
        /// <returns></returns>
        public static bool TokenVerify(string timeStamp, string nonce, string token, string signature)
        {
            var hash = System.Security.Cryptography.MD5.Create();
            //拼接簽名數據
            var signStr = token + timeStamp + nonce;
            //將字符串中字符按升序排序
            var sortStr = string.Concat(signStr.OrderBy(c => c));
            var bytes = Encoding.UTF8.GetBytes(sortStr);
            //使用MD5加密
            var md5Val = hash.ComputeHash(bytes);
            //把二進制轉化為大寫的十六進制的
            var result = new StringBuilder();
            foreach (var c in md5Val)
            {
                result.Append(c.ToString("X2"));
            }

            return result.ToString().ToUpper() == signature;
        }

        // like Ivj6eZRx40MTx2ZvnG8nA
        public static string CreateToken()
        {
            var token = Guid.NewGuid().ToString();
            token = SecurityUtil.SHA256(token);
            return token;
        }
    }
}