using Newtonsoft.Json;

namespace WebApp.Outlook.Models
{
    public class Team
    {
        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TeamGuestSettings guestSettings { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TeamMemberSettings memberSettings { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TeamMessagingSettings messagingSettings { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public TeamFunSettings funSettings { get; set; }
    }
}