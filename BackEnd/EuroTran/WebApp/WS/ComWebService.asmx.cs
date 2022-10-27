using EasyBL;
using EasyBL.WebApi.Common;
using EasyBL.WebApi.WebApi;
using EasyNet.DBUtility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;

namespace WebApp.WS
{
    /// <summary>
    /// ComWebService 的摘要说明
    /// </summary>
    [WebService(Namespace = @"http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。
    [ScriptService]
    public class ComWebService : System.Web.Services.WebService
    {
        public APISoapHeader Header { get; set; }

        public ComWebService()
        {
            //如果使用設計的元件，請取消註解下列一行
            //InitializeComponent();
        }

        #region 資料查詢

        #region QueryTableByPrc

        /// <summary>
        /// 資料查詢DataSet
        /// </summary>
        /// <param name="Type">todo: describe Type parameter on QueryTableByPrc</param>
        /// <param name="Params">todo: describe Params parameter on QueryTableByPrc</param>
        [System.Web.Services.Protocols.SoapHeader(@"header")]
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string QueryTableByPrc(string Type, Object Params)
        {
            var sJson = @"";
            try
            {
                if (!SignExtension.VerifyIdentity(HttpContext.Current, Header))
                {
                    return @"-1";
                }
                var ds = EntityBL.GetTableByPrc(Type, Params);
                sJson = JsonConvert.SerializeObject(ds, Formatting.Indented); //把DataSet轉成Json字串
            }
            catch (Exception ex)
            {
                LogService.MailSend(ex.Message + @"Param（Type：" + Type + @"Params：" + ServiceBase.JsonToString(Params) + @"）", ex, @"", @"", nameof(ComWebService), nameof(QueryTableByPrc), nameof(QueryTableByPrc), @"", @"", @"");
            }
            return sJson; //回傳字串
        }

        #endregion QueryTableByPrc

        #region QueryList

        /// <summary>
        /// 資料查詢List
        /// </summary>
        /// <param name="Type">todo: describe Type parameter on QueryList</param>
        /// <param name="Params">todo: describe Params parameter on QueryList</param>
        [System.Web.Services.Protocols.SoapHeader(@"header")]
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string QueryList(string Type, Object Params)
        {
            var sJson = @"";
            try
            {
                if (!SignExtension.VerifyIdentity(HttpContext.Current, Header))
                {
                    return @"-1";
                }
                var lst = EntityBL.QueryList(Type, Params);
                sJson = JsonConvert.SerializeObject(lst, Formatting.Indented); //把list轉成Json字串
            }
            catch (Exception ex)
            {
                LogService.MailSend(ex.Message + @"Param（Type：" + Type + @"Params：" + ServiceBase.JsonToString(Params) + @"）", ex, @"", @"", nameof(ComWebService), nameof(QueryList), nameof(QueryList), @"", @"", @"");
            }
            return sJson; //回傳字串
        }

        #endregion QueryList

        #region QueryOne

        /// <summary>
        /// 資料查詢單筆資料
        /// </summary>
        /// <param name="Type">todo: describe Type parameter on QueryOne</param>
        /// <param name="Params">todo: describe Params parameter on QueryOne</param>
        [System.Web.Services.Protocols.SoapHeader(@"header")]
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string QueryOne(string Type, Object Params)
        {
            var sJson = @"";
            try
            {
                if (!SignExtension.VerifyIdentity(HttpContext.Current, Header))
                {
                    return @"-1";
                }
                var eOne = EntityBL.QueryOne(Type, Params);
                sJson = JsonConvert.SerializeObject(eOne, Formatting.Indented); //把Model轉成Json字串
            }
            catch (Exception ex)
            {
                LogService.MailSend(ex.Message + @"Param（Type：" + Type + @"Params：" + ServiceBase.JsonToString(Params) + @"）", ex, @"", @"", nameof(ComWebService), nameof(QueryOne), nameof(QueryOne), @"", @"", @"");
            }
            return sJson; //回傳字串
        }

        #endregion QueryOne

        #region QueryByPage

        /// <summary>
        /// 資料查詢單筆資料
        /// </summary>
        /// <param name="Params">todo: describe Params parameter on QueryPage</param>
        [System.Web.Services.Protocols.SoapHeader(@"header")]
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string QueryPage(Object Params)
        {
            var sJson = @"";

            try
            {
                if (!SignExtension.VerifyIdentity(HttpContext.Current, Header))
                {
                    return @"-1";
                }
                var pr = EntityBL.QueryPage(Params);

                sJson = JsonConvert.SerializeObject(pr, Formatting.Indented); //把Model轉成Json字串
            }
            catch (Exception ex)
            {
                LogService.MailSend(ex.Message + @"Param（Params：" + ServiceBase.JsonToString(Params) + @"）", ex, @"", @"", nameof(ComWebService), nameof(QueryPage), nameof(QueryPage), @"", @"", @"");
            }
            return sJson; //回傳字串
        }

        #endregion QueryByPage

        #region QueryPageByPrc

        /// <summary>
        /// 資料查詢通過預存函數
        /// </summary>
        /// <param name="Type">todo: describe Type parameter on QueryPageByPrc</param>
        /// <param name="Params">todo: describe Params parameter on QueryPageByPrc</param>
        [System.Web.Services.Protocols.SoapHeader(@"header")]
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string QueryPageByPrc(string Type, Object Params)
        {
            var sJson = @"";

            try
            {
                if (!SignExtension.VerifyIdentity(HttpContext.Current, Header))
                {
                    return @"-1";
                }
                var pr = EntityBL.QueryPageByPrc(Type, Params, true);

                sJson = JsonConvert.SerializeObject(pr, Formatting.Indented); //把Model轉成Json字串
            }
            catch (Exception ex)
            {
                LogService.MailSend(ex.Message + @"Param（Type：" + Type + @"Params：" + ServiceBase.JsonToString(Params) + @"）", ex, @"", @"", nameof(ComWebService), nameof(QueryPageByPrc), nameof(QueryPageByPrc), @"", @"", @"");
            }
            return sJson; //回傳字串
        }

        #endregion QueryPageByPrc

        #region QueryCount

        /// <summary>
        /// 查詢資料筆數
        /// </summary>
        /// <param name="Params">todo: describe Params parameter on QueryCount</param>
        [System.Web.Services.Protocols.SoapHeader(@"header")]
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public int QueryCount(Object Params)
        {
            var iCount = 0;

            try
            {
                if (!SignExtension.VerifyIdentity(HttpContext.Current, Header))
                {
                    return -1;
                }
                iCount = DBHelper.QueryCount(Params);
            }
            catch (Exception ex)
            {
                LogService.MailSend(ex.Message + @"Param（Params：" + ServiceBase.JsonToString(Params) + @"）", ex, @"", @"", nameof(ComWebService), nameof(QueryCount), nameof(QueryCount), @"", @"", @"");
            }
            return iCount; //回傳字串
        }

        #endregion QueryCount

        #endregion 資料查詢

        #region 資料新增

        #region Add

        /// <summary>
        /// 資料新增Add
        /// </summary>
        /// <param name="Params">todo: describe Params parameter on Add</param>
        [System.Web.Services.Protocols.SoapHeader(@"header")]
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public int Add(Object Params)
        {
            var iRel = 0;
            try
            {
                if (!SignExtension.VerifyIdentity(HttpContext.Current, Header))
                {
                    return -1;
                }
                iRel = new DBHelper().Insert(Params);
            }
            catch (Exception ex)
            {
                LogService.MailSend(ex.Message + @"Param（Params：" + ServiceBase.JsonToString(Params) + @"）", ex, @"", @"", nameof(ComWebService), nameof(Add), nameof(Add), @"", @"", @"");
            }
            return iRel; //回傳字串
        }

        #endregion Add

        #endregion 資料新增

        #region 資料修改

        #region Update

        /// <summary>
        /// 資料修改Update
        /// </summary>
        /// <param name="Params">todo: describe Params parameter on Update</param>
        [System.Web.Services.Protocols.SoapHeader(@"header")]
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public int Update(Object Params)
        {
            var iRel = 0;
            try
            {
                if (!SignExtension.VerifyIdentity(HttpContext.Current, Header))
                {
                    return -1;
                }
                iRel = new DBHelper().Update(Params);
            }
            catch (Exception ex)
            {
                LogService.MailSend(ex.Message + @"Param（Params：" + ServiceBase.JsonToString(Params) + @"）", ex, @"", @"", nameof(ComWebService), nameof(Update), nameof(Update), @"", @"", @"");
            }
            return iRel; //回傳字串
        }

        #endregion Update

        #region UpdateTran

        /// <summary>
        /// 資料修改Update
        /// </summary>
        /// <param name="Params">todo: describe Params parameter on UpdateTran</param>
        [System.Web.Services.Protocols.SoapHeader(@"header")]
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public int UpdateTran(Object Params)
        {
            var iRel = 0;
            try
            {
                if (!SignExtension.VerifyIdentity(HttpContext.Current, Header))
                {
                    return -1;
                }
                iRel = new DBHelper().UpdateTran(Params);
            }
            catch (Exception ex)
            {
                LogService.MailSend(ex.Message + @"Param（Params：" + ServiceBase.JsonToString(Params) + @"）", ex, @"", @"", nameof(ComWebService), nameof(UpdateTran), nameof(UpdateTran), @"", @"", @"");
            }
            return iRel; //回傳字串
        }

        #endregion UpdateTran

        #region UpdatePrc

        /// <summary>
        /// 資料修改UpdatePrc
        /// </summary>
        /// <param name="Params">todo: describe Params parameter on UpdatePrc</param>
        [System.Web.Services.Protocols.SoapHeader(@"header")]
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public int UpdatePrc(Object Params)
        {
            var iRel = 0;
            try
            {
                if (!SignExtension.VerifyIdentity(HttpContext.Current, Header))
                {
                    return -1;
                }
                iRel = EntityBL.ExecuteSqlTran(Params);
            }
            catch (Exception ex)
            {
                LogService.MailSend(ex.Message + @"Param（Params：" + ServiceBase.JsonToString(Params) + @"）", ex, @"", @"", nameof(ComWebService), nameof(UpdatePrc), nameof(UpdatePrc), @"", @"", @"");
            }
            return iRel; //回傳字串
        }

        #endregion UpdatePrc

        #endregion 資料修改

        #region 資料刪除

        #region Delete

        /// <summary>
        /// 資料刪除
        /// </summary>
        /// <param name="Params">todo: describe Params parameter on Delete</param>
        [System.Web.Services.Protocols.SoapHeader(@"header")]
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public int Delete(Object Params)
        {
            var iRel = 0;
            try
            {
                if (!SignExtension.VerifyIdentity(HttpContext.Current, Header))
                {
                    return -1;
                }
                iRel = new DBHelper().Delete(Params);
                var sOrgid = HttpContext.Current.Request.Headers[@"orgid"];
                var sUserid = HttpContext.Current.Request.Headers[@"userid"];
                LogService.mo_Log.Debug(@"Params：" + ServiceBase.JsonToString(Params) + @" Deleter:" + sOrgid + @"-" + sUserid);
            }
            catch (Exception ex)
            {
                LogService.MailSend(ex.Message + @"Param（Params：" + ServiceBase.JsonToString(Params) + @"）", ex, @"", @"", nameof(ComWebService), nameof(Delete), nameof(Delete), @"", @"", @"");
            }
            return iRel; //回傳字串
        }

        #endregion Delete

        #endregion 資料刪除

        #region 發送郵件

        //<summary>
        //發送郵件
        //</summary>
        [System.Web.Services.Protocols.SoapHeader(@"header")]
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string SendMail(Object Params)
        {
            var sRes = @"0";
            string sError = null;
            try
            {
                do
                {
                    if (!SignExtension.VerifyIdentity(HttpContext.Current, Header))
                    {
                        return @"-1";
                    }
                    var sOrgid = HttpContext.Current.Request.Headers[@"orgid"];
                    var ms = new MailService(sOrgid);
                    var bSend = ms.SendMail(Params, out sError);

                    if (sError != null)
                    {
                        sRes = sError;
                        break;
                    }
                    if (bSend)
                    {
                        sRes = @"1";
                    }
                } while (false);

                return sRes;
            }
            catch (Exception ex)
            {
                LogService.MailSend(ex.Message + @"Param（Params：" + ServiceBase.JsonToString(Params) + @"）", ex, @"", @"", nameof(ComWebService), nameof(SendMail), @"SendMailPro", @"", @"", @"");
                return sRes;
            }
        }

        #endregion 發送郵件

        #region GetHttpClient

        /// <summary>
        /// 獲取特定Url html
        /// </summary>
        /// <param name="Url">todo: describe Url parameter on GetHttpClient</param>
        /// <param name="Params">todo: describe Params parameter on GetHttpClient</param>
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetHttpClient(string Url, Object Params)
        {
            var oJson = new Dictionary<string, object>();
            var sHtml = @"";
            try
            {
                if (!SignExtension.VerifyIdentity(HttpContext.Current, Header))
                {
                    return @"-1";
                }
                var client = new HttpWebClient(Url);
                if (Params is Dictionary<string, object>)
                {
                    var dicpm = Params as Dictionary<string, object>;
                    if (dicpm.Keys.Count > 0)
                    {
                        foreach (string key in dicpm.Keys)
                        {
                            client.PostingData.Add(key, dicpm[key].ToString());
                        }
                    }
                }
                sHtml = client.GetString();
            }
            catch (Exception ex)
            {
                LogService.MailSend(ex.Message + @"Param（Url：" + Url + @"Params：" + ServiceBase.JsonToString(Params) + @"）", ex, @"", @"", nameof(ComWebService), nameof(GetHttpClient), nameof(GetHttpClient), @"", @"", @"");
                return @"";
            }
            return sHtml;
        }

        #endregion GetHttpClient

        #region GetStringAsync

        /// <summary>
        /// 獲取特定Url html
        /// </summary>
        /// <param name="Url">todo: describe Url parameter on GetStringAsync</param>
        /// <param name="Params">todo: describe Params parameter on GetStringAsync</param>
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetStringAsync(string Url, Object Params)
        {
            var oJson = new Dictionary<string, object>();
            var sHtml = @"";
            try
            {
                if (!SignExtension.VerifyIdentity(HttpContext.Current, Header))
                {
                    return @"-1";
                }
                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    var paramList = new List<KeyValuePair<String, String>>();
                    if (Params is Dictionary<string, object>)
                    {
                        var dicpm = Params as Dictionary<string, object>;
                        if (dicpm.Keys.Count > 0)
                        {
                            foreach (string key in dicpm.Keys)
                            {
                                paramList.Add(new KeyValuePair<string, string>(key, dicpm[key].ToString()));
                            }
                        }
                    }
                    var content = new System.Net.Http.FormUrlEncodedContent(paramList);
                    var response = httpClient.PostAsync(Url, content).Result;
                    sHtml = response.Content.ReadAsStringAsync().Result;
                }
            }
            catch (Exception ex)
            {
                LogService.MailSend(ex.Message + @"Param（Url：" + Url + @"Params：" + ServiceBase.JsonToString(Params) + @"）", ex, @"", @"", nameof(ComWebService), nameof(GetStringAsync), nameof(GetStringAsync), @"", @"", @"");
                return @"";
            }
            return sHtml;
        }

        #endregion GetStringAsync

        #region 获取web客户端ip

        /*获取web客户端ip*/

        [System.Web.Services.Protocols.SoapHeader(@"header")]
        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetWebClientIp()
        {
            var sUserIP = @"";

            try
            {
                if (!SignExtension.VerifyIdentity(HttpContext.Current, Header))
                {
                    return @"-1";
                }

                if (HttpContext.Current == null
            || HttpContext.Current.Request == null
            || HttpContext.Current.Request.ServerVariables == null)
                    return @"";

                var CustomerIP = @"";

                //CDN加速后取到的IP simone 090805
                CustomerIP = HttpContext.Current.Request.Headers[@"Cdn-Src-Ip"];
                if (!string.IsNullOrEmpty(CustomerIP))
                {
                    return CustomerIP;
                }

                CustomerIP = HttpContext.Current.Request.ServerVariables[@"HTTP_X_FORWARDED_FOR"];

                if (!String.IsNullOrEmpty(CustomerIP))
                    return CustomerIP;

                if (HttpContext.Current.Request.ServerVariables[@"HTTP_VIA"] != null)
                {
                    CustomerIP = HttpContext.Current.Request.ServerVariables[@"HTTP_X_FORWARDED_FOR"];
                    if (CustomerIP == null)
                        CustomerIP = HttpContext.Current.Request.ServerVariables[@"REMOTE_ADDR"];
                }
                else
                {
                    CustomerIP = HttpContext.Current.Request.ServerVariables[@"REMOTE_ADDR"];
                }

                if (string.Compare(CustomerIP, @"unknown", true) == 0)
                    return System.Web.HttpContext.Current.Request.UserHostAddress;
                return CustomerIP;
            }
            catch (Exception ex)
            {
                LogService.MailSend(ex.Message, ex, @"", @"", nameof(ComWebService), nameof(GetWebClientIp), nameof(GetWebClientIp), @"", @"", @"");
            }
            return sUserIP;
        }

        #endregion 获取web客户端ip
    }
}