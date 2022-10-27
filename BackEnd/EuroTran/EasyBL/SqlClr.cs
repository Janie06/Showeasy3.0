using Microsoft.SqlServer.Server;
using System;
using System.Data.SqlTypes;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace EasyBL
{
    /// <summary>
    /// Author : John Date : 2018-04-31
    /// Description: 在SQL Server环境中执行的CLR方法，注意提供给SQL Server调用的方法必须有SqlFunction/SqlProcedure Attribute
    /// </summary>
    public sealed class SqlCLR
    {
        #region [函数]

        /// <param name="pattern">todo: describe pattern parameter on IsMatch</param>
        /// <param name="options">todo: describe options parameter on IsMatch</param>
        /// <param name="source">todo: describe source parameter on IsMatch</param>
        [SqlFunction(IsDeterministic = true)]
        public static SqlBoolean IsMatch(string source, string pattern, int options)
        {
            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(pattern))
            {
                return SqlBoolean.False;
            }
            var regexOptions = RegexOptions.None;
            const int optionIgnoreCase = 1;
            const int optionMultiline = 2;
            if ((options & optionIgnoreCase) != 0)
            {
                regexOptions = regexOptions | RegexOptions.IgnoreCase;
            }
            if ((options & optionMultiline) != 0)
            {
                regexOptions = regexOptions | RegexOptions.Multiline;
            }
            return (SqlBoolean)(Regex.IsMatch(source, pattern, regexOptions));
        }

        /// <summary>
        /// 判断是否为中文
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        [SqlFunction]
        public static SqlBoolean IsChinese(string source)
        {
            if (string.IsNullOrEmpty(source) || source.Trim() == string.Empty)
            {
                return false;
            }
            source = source.Trim();
            var r = System.Text.RegularExpressions.Regex.IsMatch(source, @"[\u4e00-\u9fa5]+$");
            return (SqlBoolean)r;
        }

        /// <summary>
        /// 根据url获取html
        /// </summary>
        /// <param name="url">url</param>
        /// <returns></returns>
        [SqlFunction(IsDeterministic = true)]
        public static string Fun_GetHTML(string url)
        {
            var html = string.Empty;
            html = GetAccess(url, "");
            return html;
        }

        /// <summary>
        /// "GET"
        /// </summary>
        /// <param name="url">web url</param>
        /// <param name="Referer">web referer</param>
        /// <returns>return the web access result</returns>
        public static string GetAccess(string url, string Referer)
        {
            try
            {
                var res = (HttpWebRequest)WebRequest.Create(url);
                var mycookiecontainer = new CookieContainer();
                res.CookieContainer = mycookiecontainer;
                res.Method = "GET";
                //res.Proxy = null;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
                //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback();
                res.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                //res.Headers.Add("Accept-Encoding", "gzip, deflate");
                res.Headers.Add("Accept-Language", "zh-cn,zh;q=0.8,en-us;q=0.5,en;q=0.3");
                //res.KeepAlive = false;
                res.ProtocolVersion = HttpVersion.Version11;
                if (Referer != "")
                {
                    res.Referer = Referer;
                }
                res.UserAgent = "Mozilla/5.0 (Windows NT 5.1; rv:30.0) Gecko/20100101 Firefox/30.0";
                using (HttpWebResponse resp = (HttpWebResponse)res.GetResponse())
                {
                    //resp.Cookies = Form1.mycookiecontainer.GetCookies(res.RequestUri);
                    using (Stream responseStream = resp.GetResponseStream())
                    {
                        //如果网页流压缩了，要加下面一句
                        //responseStream = new GZipStream(responseStream, CompressionMode.Decompress);
                        using (StreamReader mySreamReader = new StreamReader(responseStream, Encoding.Default))//GB2312,utf-8,GBK
                        {
                            var responseData = mySreamReader.ReadToEnd();
                            //MessageBox.Show(responseData);
                            responseStream.Close();
                            mySreamReader.Close();
                            resp.Close();
                            return responseData;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        #endregion [函数]
    }//end of class
}