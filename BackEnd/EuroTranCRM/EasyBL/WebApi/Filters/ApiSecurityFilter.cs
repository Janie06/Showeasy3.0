using EasyBL.WebApi.Common;
using EasyBL.WebApi.Helper;
using EasyBL.WebApi.Message;
using Entity.Sugar;
using Newtonsoft.Json;
using SqlSugar.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http.Filters;

namespace EasyBL.WebApi.Filters
{
    public sealed class ApiSecurityFilterAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(System.Web.Http.Controllers.HttpActionContext actionContext)
        {
            InvalidResponseMessage rm = null;
            var request = actionContext.Request;
            var method = request.Method.Method;
            var orgid = String.Empty;
            var userid = String.Empty;
            var timestamp = string.Empty;
            var nonce = string.Empty;
            var token = String.Empty;
            var signature = string.Empty;

            if (request.Headers.Contains(nameof(orgid)))
            {
                orgid = HttpUtility.UrlDecode(request.Headers.GetValues(nameof(orgid)).FirstOrDefault());
            }
            if (request.Headers.Contains(nameof(userid)))
            {
                userid = HttpUtility.UrlDecode(request.Headers.GetValues(nameof(userid)).FirstOrDefault());
            }
            if (request.Headers.Contains(nameof(timestamp)))
            {
                timestamp = HttpUtility.UrlDecode(request.Headers.GetValues(nameof(timestamp)).FirstOrDefault());
            }
            if (request.Headers.Contains(nameof(nonce)))
            {
                nonce = HttpUtility.UrlDecode(request.Headers.GetValues(nameof(nonce)).FirstOrDefault());
            }

            if (request.Headers.Contains(nameof(token)))
            {
                //token = HttpUtility.UrlDecode(request.Headers.GetValues("token").FirstOrDefault());
                token = request.Headers.GetValues(nameof(token)).FirstOrDefault();
            }

            if (request.Headers.Contains(nameof(signature)))
            {
                //signature = HttpUtility.UrlDecode(request.Headers.GetValues("signature").FirstOrDefault());
                signature = request.Headers.GetValues(nameof(signature)).FirstOrDefault();
            }
            //GetLogin（登陸）方法不需要进行签名验证
            if (actionContext.ActionDescriptor.ActionName == "GetLogin")
            {
                base.OnActionExecuting(actionContext);
                return;
            }
            //GetToken（獲取Token）方法不需要进行签名验证
            else if (actionContext.ActionDescriptor.ActionName == "GetToken")
            {
                if (string.IsNullOrEmpty(orgid) || string.IsNullOrEmpty(userid) || string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(nonce))
                {
                    rm = new InvalidResponseMessage
                    {
                        STATUSCODE = (int)StatusCodeEnum.ParameterError,
                        MSG = StatusCodeEnum.ParameterError.GetEnumText(),
                        DATA = null
                    };
                    actionContext.Response = HttpResponseExtension.ToJson(JsonConvert.SerializeObject(rm));
                    base.OnActionExecuting(actionContext);
                    return;
                }
                else
                {
                    base.OnActionExecuting(actionContext);
                    return;
                }
            }

            //判断请求头是否包含以下参数
            if (string.IsNullOrEmpty(orgid) || string.IsNullOrEmpty(userid) || string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(nonce) || (string.IsNullOrEmpty(token) && string.IsNullOrEmpty(signature)))
            {
                rm = new InvalidResponseMessage
                {
                    STATUSCODE = (int)StatusCodeEnum.ParameterError,
                    MSG = StatusCodeEnum.ParameterError.GetEnumText(),
                    DATA = null
                };
                actionContext.Response = HttpResponseExtension.ToJson(JsonConvert.SerializeObject(rm));
                base.OnActionExecuting(actionContext);
                return;
            }

            //判断timespan是否有效
            var ts2 = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds;
            double ts1;
            var timespanvalidate = double.TryParse(timestamp, out ts1);
            var ts = ts2 - ts1;
            var falg = ts > int.Parse(WebSettingsConfig.UrlExpireTime) * 1000;
            if (falg || (!timespanvalidate))
            {
                rm = new InvalidResponseMessage
                {
                    STATUSCODE = (int)StatusCodeEnum.URLExpireError,
                    MSG = StatusCodeEnum.URLExpireError.GetEnumText(),
                    DATA = null
                };
                actionContext.Response = HttpResponseExtension.ToJson(JsonConvert.SerializeObject(rm));
                base.OnActionExecuting(actionContext);
                return;
            }

            //判断token是否有效
            var oTicket = new OTB_SYS_TicketAuth();
            oTicket = (OTB_SYS_TicketAuth)HttpRuntimeCache.Get(orgid + userid);
            if (oTicket == null)
            {
                var db = SugarBase.DB;
                oTicket = db.Queryable<OTB_SYS_TicketAuth>().Single(it => it.OrgID == orgid && it.UserID == userid);
            }
            var signtoken = string.Empty;
            if (oTicket == null || oTicket.ExpireTime < DateTime.Now)
            {
                rm = new InvalidResponseMessage
                {
                    STATUSCODE = (int)StatusCodeEnum.TokenVerifyFailed,
                    MSG = StatusCodeEnum.TokenVerifyFailed.GetEnumText(),
                    DATA = null
                };
                actionContext.Response = HttpResponseExtension.ToJson(JsonConvert.SerializeObject(rm));
                base.OnActionExecuting(actionContext);
                return;
            }
            else
            {
                signtoken = oTicket.Token;
            }

            //根据请求类型拼接参数
            var form = HttpContext.Current.Request.QueryString;
            var data = string.Empty;
            switch (method)
            {
                case "POST":
                    var stream = HttpContext.Current.Request.InputStream;
                    var responseJson = string.Empty;
                    var streamReader = new StreamReader(stream);
                    data = streamReader.ReadToEnd();
                    break;

                case "GET":
                    //第一步：取出所有get参数
                    IDictionary<string, string> parameters = new Dictionary<string, string>();
                    for (int f = 0; f < form.Count; f++)
                    {
                        var key = form.Keys[f];
                        parameters.Add(key, form[key]);
                    }

                    // 第二步：把字典按Key的字母顺序排序
                    IDictionary<string, string> sortedParams = new SortedDictionary<string, string>(parameters);
                    var dem = sortedParams.GetEnumerator();

                    // 第三步：把所有参数名和参数值串在一起
                    var query = new StringBuilder();
                    while (dem.MoveNext())
                    {
                        var key = dem.Current.Key;
                        var value = dem.Current.Value;
                        if (!string.IsNullOrEmpty(key))
                        {
                            query.Append(key).Append(value);
                        }
                    }
                    data = query.ToString();
                    break;

                default:
                    rm = new InvalidResponseMessage
                    {
                        STATUSCODE = (int)StatusCodeEnum.HttpMehtodError,
                        MSG = StatusCodeEnum.HttpMehtodError.GetEnumText(),
                        DATA = null
                    };
                    actionContext.Response = HttpResponseExtension.ToJson(JsonConvert.SerializeObject(rm));
                    base.OnActionExecuting(actionContext);
                    return;
            }
            var result = false;
            result = signature == string.Empty ? signtoken == token : SignExtension.TokenVerify(timestamp, nonce, signtoken, signature);
            if (!result)
            {
                rm = new InvalidResponseMessage
                {
                    STATUSCODE = (int)StatusCodeEnum.HttpRequestError,
                    MSG = StatusCodeEnum.HttpRequestError.GetEnumText(),
                    DATA = null
                };
                actionContext.Response = HttpResponseExtension.ToJson(JsonConvert.SerializeObject(rm));
                base.OnActionExecuting(actionContext);
                return;
            }
            else
            {
                base.OnActionExecuting(actionContext);
            }
        }

        public override void OnActionExecuted(HttpActionExecutedContext actContext)
        {
            var content = actContext.Response.Content;
            var bytes = content?.ReadAsByteArrayAsync().Result;
            var zlibbedContent = bytes == null ? new byte[0] :
            CompressionHelper.DeflateByte(bytes);
            actContext.Response.Content = new ByteArrayContent(zlibbedContent);
            actContext.Response.Content.Headers.Remove("Content-Type");
            actContext.Response.Content.Headers.Add("Content-encoding", "deflate");
            actContext.Response.Content.Headers.Add("Content-Type", "application/json");
            base.OnActionExecuted(actContext);
        }
    }
}