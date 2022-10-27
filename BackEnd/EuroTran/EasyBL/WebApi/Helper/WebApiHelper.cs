using EasyBL.WebApi.Common;
using EasyBL.WebApi.Message;
using EasyBL.WebApi.Models;
using Entity.Sugar;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace EasyBL.WebApi.Helper
{
    public class WebApiHelper
    {
        /// <summary>
        /// Post请求
        /// </summary>
        /// <param name="url"></param>
        /// <param name="data"></param>
        /// <param name="orgId">todo: describe orgId parameter on Post</param>
        /// <param name="userId">todo: describe userId parameter on Post</param>
        /// <param name="passWd">todo: describe passWd parameter on Post</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Post<T>(string url, string data, string orgId, string userId, string passWd)
        {
            var bytes = Encoding.UTF8.GetBytes(data);
            var request = (HttpWebRequest)WebRequest.Create(url);

            var timeStamp = GetTimeStamp();
            var nonce = GetRandom();
            //加入头信息
            request.Headers.Add("orgid", orgId); //当前请求組織Id
            request.Headers.Add("userid", userId); //当前请求用户StaffId
            request.Headers.Add("passwd", passWd); //当前请求用户StaffId
            request.Headers.Add("timestamp", timeStamp); //发起请求时的时间戳（单位：毫秒）
            request.Headers.Add(nameof(nonce), nonce); //发起请求时的时间戳（单位：毫秒）
            request.Headers.Add("signature", GetSignature(timeStamp, nonce, orgId, userId, passWd)); //当前请求内容的数字签名

            //写数据
            request.Method = "POST";
            request.ContentLength = bytes.Length;
            request.ContentType = "application/x-www-form-urlencoded";
            request.Headers.Set("Pragma", "no-cache");
            request.Headers.Set("Content-Encoding", "gzip,deflate");
            request.AutomaticDecompression = DecompressionMethods.Deflate;
            var reqstream = request.GetRequestStream();
            reqstream.Write(bytes, 0, bytes.Length);

            //读数据
            request.Timeout = 300000;
            var response = (HttpWebResponse)request.GetResponse();
            var streamReceive = response.GetResponseStream();
            using (var streamReader = new StreamReader(streamReceive, Encoding.UTF8))
            {
                var strResult = streamReader.ReadToEnd();

                //关闭流
                reqstream.Close();
                streamReader.Close();
                streamReceive.Close();
                request.Abort();
                response.Close();

                return JsonConvert.DeserializeObject<T>(strResult);
            }
        }

        /// <summary>
        /// Get请求
        /// </summary>
        /// <param name="webApi"></param>
        /// <param name="queryStr"></param>
        /// <param name="query">todo: describe query parameter on Get</param>
        /// <param name="orgId">todo: describe orgId parameter on Get</param>
        /// <param name="userId">todo: describe userId parameter on Get</param>
        /// <param name="passWd">todo: describe passWd parameter on Get</param>
        /// <param name="sign">todo: describe sign parameter on Get</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>(string webApi, string queryStr, string orgId, string userId, string passWd, bool sign = true)
        {
            var request = (HttpWebRequest)WebRequest.Create(webApi + "?" + queryStr);
            var timeStamp = GetTimeStamp();
            var nonce = GetRandom();
            //加入头信息
            request.Headers.Add("orgid", orgId); //当前请求組織Id
            request.Headers.Add("userid", userId); //当前请求用户Id
            request.Headers.Add("passwd", passWd); //当前请求用户密碼
            request.Headers.Add("timestamp", timeStamp); //发起请求时的时间戳（单位：毫秒）
            request.Headers.Add(nameof(nonce), nonce); //发起请求时的时间戳（单位：毫秒）

            if (sign)
                request.Headers.Add("signature", GetSignature(timeStamp, nonce, orgId, userId, passWd)); //当前请求内容的数字签名

            request.Method = "GET";
            request.ContentType = "application/json";
            request.Timeout = 90000;
            request.Headers.Set("Pragma", "no-cache");
            request.Headers.Set("Content-Encoding", "gzip,deflate");
            request.AutomaticDecompression = DecompressionMethods.Deflate;
            var response = (HttpWebResponse)request.GetResponse();
            var streamReceive = response.GetResponseStream();
            using (var streamReader = new StreamReader(streamReceive, Encoding.UTF8))
            {
                var strResult = streamReader.ReadToEnd();

                streamReader.Close();
                streamReceive.Close();
                request.Abort();
                response.Close();

                return JsonConvert.DeserializeObject<T>(strResult);
            }
        }

        /// <summary>
        /// Base64編碼參數
        /// </summary>
        /// <param name="rm">todo: describe rm parameter on Base64</param>
        /// <returns></returns>
        public static string Base64(RequestMessage rm)
        {
            var pm = HttpUtility.UrlEncode(JsonConvert.SerializeObject(rm));
            var bytes = ASCIIEncoding.ASCII.GetBytes(pm);
            return "=" + Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 获取token
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="userId"></param>
        /// <param name="passWd"></param>
        /// <returns></returns>
        public static TokenResult GetSignToken(string orgId, string userId, string passWd)
        {
            var tokenApi = WebSettingsConfig.TokenApi;
            var parames = new Dictionary<string, string>
            {
                { nameof(orgId), orgId },
                { nameof(userId), userId },
                { "pasWd", passWd }
            };
            var parameters = GetQueryString(parames);
            var token = WebApiHelper.Get<TokenResult>(tokenApi, parameters.Item2, orgId, userId, passWd, false);
            return token;
        }

        /// <summary>
        /// 计算签名
        /// </summary>
        /// <param name="timeStamp"></param>
        /// <param name="nonce"></param>
        /// <param name="orgId"></param>
        /// <param name="userId"></param>
        /// <param name="passWd">todo: describe passWd parameter on GetSignature</param>
        /// <returns></returns>
        private static string GetSignature(string timeStamp, string nonce, string orgId, string userId, string passWd)
        {
            TicketAuth token = null;
            var resultMsg = GetSignToken(orgId, userId, passWd);
            if (resultMsg != null)
            {
                if (resultMsg.StatusCode == (int)StatusCodeEnum.Success)
                {
                    token = resultMsg.Result;
                }
                else
                {
                    throw new Exception(resultMsg.Data.ToString());
                }
            }
            else
            {
                throw new Exception("token为null，組織帳號：" + orgId + "帳號：" + userId + "密碼：" + passWd);
            }

            var hash = System.Security.Cryptography.MD5.Create();
            //拼接签名数据
            var signStr = token.Token + timeStamp + nonce;
            //将字符串中字符按升序排序
            var sortStr = string.Concat(signStr.OrderBy(c => c));
            var bytes = Encoding.UTF8.GetBytes(sortStr);
            //使用MD5加密
            var md5Val = hash.ComputeHash(bytes);
            //把二进制转化为大写的十六进制
            var result = new StringBuilder();
            foreach (var c in md5Val)
            {
                result.Append(c.ToString("X2"));
            }
            return result.ToString().ToUpper();
        }

        /// <summary>
        /// 获取时间戳
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            var ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalMilliseconds).ToString();
        }

        /// <summary>
        /// 获取随机数
        /// </summary>
        /// <returns></returns>
        public static string GetRandom()
        {
            var rd = new Random(DateTime.Now.Millisecond);
            var i = rd.Next(0, int.MaxValue);
            return i.ToString();
        }

        /// <summary>
        /// 拼接get参数
        /// </summary>
        /// <param name="parames"></param>
        /// <returns></returns>
        public static Tuple<string, string> GetQueryString(Dictionary<string, string> parames)
        {
            // 第一步：把字典按Key的字母顺序排序
            IDictionary<string, string> sortedParams = new SortedDictionary<string, string>(parames);
            var dem = sortedParams.GetEnumerator();

            // 第二步：把所有参数名和参数值串在一起
            var query = new StringBuilder("");  //签名字符串
            var queryStr = new StringBuilder(""); //url参数
            if (parames == null || parames.Count == 0)
                return new Tuple<string, string>("", "");

            while (dem.MoveNext())
            {
                var key = dem.Current.Key;
                var value = dem.Current.Value;
                if (!string.IsNullOrEmpty(key))
                {
                    query.Append(key).Append(value);
                    queryStr.Append("&").Append(key).Append("=").Append(value);
                }
            }

            return new Tuple<string, string>(query.ToString(), queryStr.ToString().Substring(1, queryStr.Length - 1));
        }
    }
}