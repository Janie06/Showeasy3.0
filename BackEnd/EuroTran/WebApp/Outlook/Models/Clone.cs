namespace WebApp.Outlook.Models
{
    public class Clone
    {
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string MailNickName { get; set; }
        public string TeamVisibilityType { get; set; }
        public string PartsToClone { get; set; } // "apps,members,settings,tabs,channels"
    }
}