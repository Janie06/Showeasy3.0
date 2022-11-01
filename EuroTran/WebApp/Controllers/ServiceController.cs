using EasyBL;
using EasyBL.WebApi.Filters;
using EasyBL.WEBAPP;
using System;
using System.Net.Http;
using System.Web.Http;

namespace WebApp.Controllers
{
    public class ServiceController : ApiController
    {
        /// <summary>
        /// 获取token
        /// </summary>
        /// <param name="orgId"></param>
        /// <param name="userId"></param>
        /// <param name="pasWd"></param>
        /// <returns></returns>
        [HttpGet]
        [ApiSecurityFilter]
        public HttpResponseMessage GetToken(string orgId, string userId, string pasWd)
        {
            return new BaseAuthorizeService().GetToken(orgId, userId, pasWd);
        }

        /// <summary>
        /// 後臺登陸
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        [HttpPost]
        [ApiSecurityFilter]
        public HttpResponseMessage GetLogin([FromBody]dynamic data)
        {
            try
            {
                return new AuthorizeService().GetLogin(data, Request);
            }
            catch (Exception ex)
            {
                var sMsg = Util.GetLastExceptionMsg(ex);
                LogService.MailSend(sMsg + @"Param：", ex, @"TE", @"apadmin", nameof(ServiceController), @"登陸", @"GetLogin（系統登陸）", @"", @"", @"");
                return null;
            }
        }
    }
}