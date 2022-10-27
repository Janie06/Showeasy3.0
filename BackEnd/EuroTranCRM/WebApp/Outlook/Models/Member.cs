using Newtonsoft.Json;
using System;

namespace WebApp.Outlook.Models
{
    public class Member
    {
        public String GroupId { get; set; }
        public String Upn { get; set; }
        public bool Owner { get; set; }

        [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string Id { get; set; }
    }
}