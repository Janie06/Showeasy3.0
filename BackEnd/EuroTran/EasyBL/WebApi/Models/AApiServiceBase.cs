using EasyBL.WebApi.Message;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace EasyBL.WebApi.Models
{
    public abstract class ApiServiceBase
    {
        #region Define

        private const string _TAG_CALLBACK = "callback";

        private class CQS2Json
        {
            private class CQSJsonNode
            {
                public object Data { get; set; }

                public override string ToString()
                {
                    var sb = new StringBuilder();

                    if (null == Data)
                    {
                        sb.Append("null");
                    }
                    else if (Data is string)
                    {
                        var sData = Data.ToString();
                        if (0 < sData.Length && IsNumeric(sData))
                        {
                            sb.Append(Data.ToString());
                        }
                        else if (0 == sData.Length)
                        {
                            sb.Append("null");
                        }
                        else
                        {
                            sb.Append("\"" + Data + "\"");
                        }
                    }
                    else if (Data is Dictionary<string, CQSJsonNode>)
                    {
                        var dic = Data as Dictionary<string, CQSJsonNode>;
                        sb.Append("{");
                        var nCount = 0;
                        foreach (string sKey in dic.Keys)
                        {
                            if (0 != nCount)
                            {
                                sb.Append(",");
                            }
                            sb.Append("\"" + sKey + "\" : ");
                            sb.Append(dic[sKey].ToString());
                            nCount = nCount + 1;
                        }

                        sb.Append("}");
                    }
                    else if (Data is List<CQSJsonNode>)
                    {
                        var l = Data as List<CQSJsonNode>;
                        sb.Append("[");
                        var nCount = 0;
                        foreach (CQSJsonNode n in l)
                        {
                            if (0 != nCount)
                            {
                                sb.Append(",");
                            }
                            sb.Append(n.ToString());
                            nCount = nCount + 1;
                        }

                        sb.Append("]");
                    }
                    else
                    {
                        throw new Exception("No handle");
                    }

                    return sb.ToString();
                }

                public void AddKeyValue(string i_sKey, CQSJsonNode i_Value)
                {
                    if (null == Data)
                    {
                        Data = new Dictionary<string, CQSJsonNode>();
                    }
                    var dic = Data as Dictionary<string, CQSJsonNode>;
                    dic.Add(i_sKey, i_Value);
                }

                public void AppendList(CQSJsonNode i_Value)
                {
                    if (null == Data)
                    {
                        Data = new List<CQSJsonNode>();
                    }
                    var l = Data as List<CQSJsonNode>;
                    l.Add(i_Value);
                }

                public void SetValue(string i_Value)
                {
                    Data = i_Value;
                }

                public CQSJsonNode GetMapValue(string i_sKey)
                {
                    CQSJsonNode res = null;
                    if (null == Data)
                    {
                        Data = new Dictionary<string, CQSJsonNode>();
                    }
                    var dic = Data as Dictionary<string, CQSJsonNode>;
                    if (!dic.Keys.Contains(i_sKey))
                    {
                        res = new CQSJsonNode();
                        dic.Add(i_sKey, res);
                    }
                    else
                    {
                        res = dic[i_sKey];
                    }
                    return res;
                }

                public CQSJsonNode GetListItem(int i_n)
                {
                    CQSJsonNode res = null;
                    if (null == Data)
                    {
                        Data = new List<CQSJsonNode>();
                    }

                    var l = Data as List<CQSJsonNode>;
                    if (i_n >= l.Count)
                    {
                        res = new CQSJsonNode();
                        l.Add(res);
                    }
                    else
                    {
                        res = l[i_n];
                    }

                    return res;
                }

                public CQSJsonNode GetMapItemByKey(string i_sKey)
                {
                    CQSJsonNode nRes = null;
                    if (Data != null && Data is Dictionary<string, CQSJsonNode>)
                    {
                        var dic = Data as Dictionary<string, CQSJsonNode>;
                        nRes = dic[i_sKey];
                    }
                    return nRes;
                }

                public CQSJsonNode GetListItemByIndex(int i_nIdx)
                {
                    CQSJsonNode nRes = null;
                    if (Data != null && Data is List<CQSJsonNode>)
                    {
                        var l = Data as List<CQSJsonNode>;
                        nRes = l[i_nIdx];
                    }
                    return nRes;
                }
            }

            public static bool IsNumeric(string inputString)
            {
                return Regex.IsMatch(inputString, "^[0-9]+$");
            }

            public static string Convert(IEnumerable<KeyValuePair<string, string>> i_iKeyValue)
            {
                var rootNode = new CQSJsonNode();
                CQSJsonNode curNode = null;
                var nCount = 0;
                foreach (KeyValuePair<string, string> kp in i_iKeyValue)
                {
                    var v = new CQSJsonNode { Data = kp.Value };

                    var saToken = kp.Key.Split(new char[] { '[', ']', '.' });
                    curNode = rootNode;
                    nCount = 0;

                    foreach (string sToken in saToken)
                    {
                        if ("" == sToken)
                        {
                            nCount = nCount + 1;
                            continue;
                        }
                        var isList = IsNumeric(sToken);

                        if ((saToken.Length - 1) == nCount)
                        {
                            if (!isList)
                            {
                                curNode.AddKeyValue(sToken, v);
                            }
                            else
                            {
                                curNode.AppendList(v);
                            }
                        }
                        else
                        {
                            curNode = !isList ? curNode.GetMapValue(sToken) : curNode.GetListItem(Int32.Parse(sToken));
                        }
                        nCount = nCount + 1;
                    }
                }
                return rootNode.ToString();
            }
        }

        #endregion Define

        #region Abstract decleare

        protected abstract string HandleRequest(RequestMessage i_jo, HttpRequestMessage i_rRequest);

        #endregion Abstract decleare

        #region helper function

        protected ApiServiceBase()
        {
        }

        /// <summary>
        /// Converter for http query string to JObject
        /// </summary>
        /// <param name="i_iKeyValue"></param>
        /// <returns></returns>
        protected static RequestMessage HttpQueryStringToRequest(IEnumerable<KeyValuePair<string, string>> i_iKeyValue)
        {
            var sJaon = CQS2Json.Convert(i_iKeyValue);

            return JsonConvert.DeserializeObject<RequestMessage>(sJaon);
        }

        protected static string GetHttpRequestValue(IEnumerable<KeyValuePair<string, string>> i_iKeyValue, string i_sTarget)
        {
            string sRes = null;
            foreach (KeyValuePair<string, string> kp in i_iKeyValue)
            {
                if (i_sTarget == kp.Key)
                {
                    sRes = kp.Value;
                    break;
                }
            }
            return sRes;
        }

        /// <summary>
        /// Create json message
        /// </summary>
        /// <param name="i_o"></param>
        /// <returns></returns>
        protected static string MakeMessage(object i_o)
        {
            return JsonConvert.SerializeObject(i_o);
        }

        /// <summary>
        /// Create general error
        /// </summary>
        /// <param name="i_sMsg"></param>
        /// <param name="i_OriRequest">todo: describe i_OriRequest parameter on MakeErrorReturn</param>
        /// <returns></returns>
        protected static string MakeErrorReturn(RequestMessage i_OriRequest, string i_sMsg)
        {
            return MakeMessage(new ErrorResponseMessage(i_sMsg, i_OriRequest));
        }

        /// <summary>
        /// Create general Invalid
        /// </summary>
        /// <param name="i_OriRequest">todo: describe i_OriRequest parameter on MakeInvalidReturn</param>
        /// <param name="i_sMsgCode">todo: describe i_sMsgCode parameter on MakeInvalidReturn</param>
        /// <param name="i_sMsg">todo: describe i_sMsg parameter on MakeInvalidReturn</param>
        /// <returns></returns>
        protected static string MakeInvalidReturn(RequestMessage i_OriRequest, int i_sMsgCode, string i_sMsg)
        {
            return MakeMessage(new InvalidResponseMessage(i_OriRequest, i_sMsgCode, i_sMsg));
        }

        public string ProgessJson(string i_sJson, HttpRequestMessage i_rRequest)
        {
            RequestMessage crm = null;

            crm = null != i_sJson ? JsonConvert.DeserializeObject<RequestMessage>(i_sJson) : new RequestMessage { TYPE = "LITE" };
            crm.ClientIP = GetClientIp(i_rRequest);
            return HandleRequest(crm, i_rRequest);
        }

        /// <summary>
        /// </summary>
        /// <param name="i_rRequest"></param>
        /// <returns></returns>
        public static string GetClientIp(HttpRequestMessage i_rRequest)
        {
            if (i_rRequest.Properties.ContainsKey("MS_HttpContext"))
            {
                return ((HttpContextWrapper)i_rRequest.Properties["MS_HttpContext"]).Request.UserHostAddress;
            }
            else if (HttpContext.Current != null)
            {
                return HttpContext.Current.Request.UserHostAddress;
            }
            else
            {
                return null;
            }
        }

        #endregion helper function
    }
}