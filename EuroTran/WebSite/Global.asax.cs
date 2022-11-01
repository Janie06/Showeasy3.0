using EasyBL;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;

namespace WebSite
{
    public class WebApiApplication : HttpApplication
    {
        public override void Init()
        {
            this.PostAuthenticateRequest += (sender, e) => HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            base.Init();
        }

        protected void Application_Start()
        {
            string LogWorkDir = System.Web.Hosting.HostingEnvironment.MapPath("~/");
            LogHelper.Init(LogWorkDir, System.IO.Path.Combine(LogWorkDir, "Log4Net.config"));

            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }
    }
}