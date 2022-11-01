using System;

namespace WebApp.Outlook.Models
{
    public class TeamMessagingSettings
    {
        public Boolean AllowUserEditMessages { get; set; }
        public Boolean AllowUserDeleteMessages { get; set; }
        public Boolean AllowOwnerDeleteMessages { get; set; }
        public Boolean AllowTeamMentions { get; set; }
        public Boolean AllowChannelMentions { get; set; }
    }
}