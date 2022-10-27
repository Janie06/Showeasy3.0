using System.Web.Http;

namespace WebSite
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{data}",
                defaults: new { data = RouteParameter.Optional }
            );
        }
    }
}