using Newtonsoft.Json;
using System;

namespace WebApp.Outlook.Models
{
    public class Group
    {
        public String DisplayName { get; set; }
        public String MailNickname { get; set; }
        public String Description { get; set; }
        public String[] GroupTypes { get; set; }
        public Boolean MailEnabled { get; set; }
        public Boolean SecurityEnabled { get; set; }
        public String Visibility { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Id { get; set; }
    }
}