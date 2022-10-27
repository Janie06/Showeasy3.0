using EasyBL.WebApi.Message;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace EasyBL
{
    public class ServiceBase : MessageBase
    {
        public const string SERVICE = "service";

        protected Dictionary<string, object> _dicInitData;

        private static string _sRootPath = null;
        public static string RootPath { get { return _sRootPath; } }

        public static void SetRootPath(string i_sRootPath)
        {
            _sRootPath = i_sRootPath;
        }

        #region 1-1-1

        /// <summary>
        /// 1-1-1 JS路徑
        /// </summary>
        protected virtual string JSPath
        {
            get { return "NOT SET"; }
        }

        /// <summary>
        /// 1-1-1 HTML路徑
        /// </summary>
        protected virtual string HTMLPath
        {
            get { return "NOT SET"; }
        }

        /// <summary>
        /// 1-1-1 JS Version
        /// </summary>
        protected string JSVersion
        {
            get { return _getVersion(JSPath, "g_JSRevisionVersioin"); }
        }

        /// <summary>
        /// 1-1-1 JS Version
        /// </summary>
        protected string HTMLVersion
        {
            get { return _getVersion(HTMLPath, "g_HTMLRevisionVersioin"); }
        }

        /// <summary>
        /// 取得Service版本號
        /// </summary>
        protected string ServiceVersion
        {
            get { return ParseServiceVersion(); }
        }

        private static string _getVersion(string i_sPath, string i_sPattern)
        {
            var sRes = "-1";

            try
            {
                do
                {
                    if (i_sPath == "NOT SET")
                    {
                        break;
                    }

                    var sPath = System.IO.Path.Combine(RootPath, i_sPath);
                    if (!System.IO.File.Exists(sPath))
                    {
                        break;
                    }

                    var sContent = System.IO.File.ReadAllText(sPath);
                    var nIdx = sContent.LastIndexOf(i_sPattern, StringComparison.Ordinal);
                    if (nIdx == -1)
                    {
                        break;
                    }

                    var nVersionStart = sContent.IndexOf('"', nIdx);

                    if (nVersionStart == -1)
                    {
                        break;
                    }

                    nVersionStart++;

                    var nVersionEnd = sContent.IndexOf('"', nVersionStart);

                    if (nVersionEnd == -1)
                    {
                        break;
                    }

                    sRes = sContent.Substring(nVersionStart, nVersionEnd - nVersionStart);
                }
                while (false);
            }
            catch (Exception ex)
            {
                sRes = Util.GetLastExceptionMsg(ex);
            }

            return sRes;
        }

        /// <summary>
        /// 取得Service版本號
        /// </summary>
        /// <returns>版號。 -1表示沒有取得版本號</returns>
        protected string ParseServiceVersion()
        {
            var sRes = "-1";

            var pi = this.GetType().GetProperty("ServiceRevisionVersion");

            if (pi != null)
            {
                var oValue = pi.GetValue(this, null);

                if (oValue != null)
                {
                    sRes = oValue.ToString();
                }
            }

            return sRes;
        }

        public Dictionary<string, string> VersionInfo
        {
            get
            {
                return new Dictionary<string, string>() {
                { "html", HTMLVersion },
                { "js", JSVersion },
                { "service", ServiceVersion },
                { "htmlpath", HTMLPath },
                { "jspath", JSPath },
                { "servicename", this.GetType().Name }
            };
            }
        }

        #endregion 1-1-1

        public ResponseMessage Entry(RequestMessage i_crm)
        {
            string sMsg = null;
            ResponseMessage rm = null;

            try
            {
                do
                {
                    // Find Native Method
                    var sFunctionName = i_crm.TYPE;

                    if (null == sFunctionName)
                    {
                        //sMsg = string.Format("NO METHOD - {0}", sFunctionName);
                        sMsg = BaseExceptionWord.ex000001; //NO METHOD
                        break;
                    }

                    var mi = this.GetType().GetMethod(sFunctionName);
                    if (null == mi)
                    {
                        //sMsg = string.Format("NO MATCH METHOD - {0}", sFunctionName);
                        sMsg = BaseExceptionWord.ex000002; //NO MATCH METHOD
                        break;
                    }

                    object[] ao = { i_crm };

                    rm = (ResponseMessage)mi.Invoke(this, ao);
                }
                while (false);
            }
            catch (Exception ex)
            {
                sMsg = Util.GetLastExceptionMsg(ex);
            }

            if (null != sMsg)
            {
                //crm = new ErrorResponseMessage(sMsg, i_crm);
            }

            return rm;
        }

        public static string MakeDebugFullDump(string i_sMsg, RequestMessage i_crm)
        {
            var sRes = Environment.NewLine;

            sRes += "**** Start Dump **************************" + Environment.NewLine;
            sRes += $"Custom Message:{i_sMsg}" + Environment.NewLine;
            sRes += $"Parameter Json:{JsonConvert.SerializeObject(i_crm)}" + Environment.NewLine;
            sRes += "Call Stack Dump:" + Environment.NewLine;
            sRes += Environment.StackTrace + Environment.NewLine;
            sRes += "*****End   Dump *************************" + Environment.NewLine;

            return sRes;
        }

        public static string JProperty2Dic(JObject i_jpData, ref Dictionary<string, object> io_dicData)
        {
            string sRes = null;
            var dicRes = new Dictionary<string, object>();

            try
            {
                foreach (JProperty property in i_jpData.Properties())
                {
                    var jv = property.Value;
                    dicRes.Add(property.Name, jv.Value<string>());
                }
            }
            catch (Exception ex)
            {
                sRes = Util.GetLastExceptionMsg(ex);
            }

            io_dicData = dicRes;
            return sRes;
        }

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

        #endregion JsonToString
    }
}