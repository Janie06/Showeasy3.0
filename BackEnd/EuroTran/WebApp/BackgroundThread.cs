using Microsoft.AspNet.SignalR;
using System;
using System.Threading.Tasks;
using WebApp.Hubs;

namespace WebApp
{
    public class BackgroundThread
    {
        public static bool Enabled { get; set; }

        public static async Task SendOnHubAsync()
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<MsgHub>();
            while (Enabled)
            {
                //var db = DBUnit.Instance;
                //OVW_SYS_Announcement oEip = new OVW_SYS_Announcement();
                //var saAnn = db.Queryable<OVW_SYS_Announcement>().Where(it => it.EndDateTime == DateTime.Now).ToList();
                //if (msg.ConnectionIds.Count > 0)
                //{
                //    await context.Clients.Clients(msg.ConnectionIds).message(msg); // 特定的客户端，只對當前在綫人員推送
                //}
                await Task.Delay(TimeSpan.FromMinutes(1));//目前以一分鐘為准
            }
        }
    }
}