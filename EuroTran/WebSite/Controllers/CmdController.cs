using EasyBL.WebApi.Models;
using EasyBL.WEBSITE.WebApi.Filters;
using System.Net.Http;
using System.Web.Http;

namespace WebSite.Controllers
{
    public class CmdController : ApiController
    {
        [HttpPost]
        [ApiSecurityFilter]
        public HttpResponseMessage GetData([FromBody]dynamic data)
        {
            return (new CmdService()).GetData(data, true, Request);
        }
    }
}