using System.Web.Mvc;
using System.Web.Routing;

namespace WebApp
{
    public static class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{directory}/{resource}.asmx/{*pathInfo}");
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{para1}/{para2}/{para3}/{para4}",
                defaults: new
                {
                    controller = "Login",
                    action = "Index",
                    para1 = UrlParameter.Optional,
                    para2 = UrlParameter.Optional,
                    para3 = UrlParameter.Optional,
                    para4 = UrlParameter.Optional
                }
            );
        }
    }
}