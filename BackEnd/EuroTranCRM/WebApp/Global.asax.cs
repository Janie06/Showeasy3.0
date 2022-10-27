using EasyBL;
using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.SessionState;

namespace WebApp
{
    public class Global : HttpApplication
    {
        public override void Init()
        {
            this.PostAuthenticateRequest += (sender, e) => HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            base.Init();
        }

        private void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);


            string LogWorkDir = System.Web.Hosting.HostingEnvironment.MapPath("~/");
            LogHelper.Init(LogWorkDir, System.IO.Path.Combine(LogWorkDir, "Log4Net.config"));

            // 启动日志组件
            //log4net.Config.XmlConfigurator.Configure();
            // 启动索引管理器
            //IndexManager.Instance.Start();
            // 启动定时任务
            //TaskScheduler.Start();
        }

        private void Application_End(object sender, EventArgs e)
        {
            // Code that runs on application shutdown
        }

        private void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
        }
    }
}