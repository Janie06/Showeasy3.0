using System.Web;

namespace EasyBL.Handler
{
    /// <summary>
    /// Config 的摘要说明
    /// </summary>
    public class ConfigHandler : Handler
    {
        public ConfigHandler(HttpContext context) : base(context)
        {
        }

        public override void Process()
        {
            WriteJson(Config.Items);
        }
    }
}