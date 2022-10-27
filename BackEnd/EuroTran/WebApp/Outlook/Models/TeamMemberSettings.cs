using System;

namespace WebApp.Outlook.Models
{
    public class TeamMemberSettings
    {
        public Boolean AllowCreateUpdateChannels { get; set; }
        public Boolean AllowDeleteChannels { get; set; }
        public Boolean AllowAddRemoveApps { get; set; }
        public Boolean AllowCreateUpdateRemoveTabs { get; set; }
        public Boolean AllowCreateUpdateRemoveConnectors { get; set; }
    }
}