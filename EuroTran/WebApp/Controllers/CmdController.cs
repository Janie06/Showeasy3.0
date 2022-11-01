using EasyBL.WebApi.Filters;
using EasyBL.WebApi.Models;
using System.Net.Http;
using System.Web.Http;

namespace WebApp.Controllers
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