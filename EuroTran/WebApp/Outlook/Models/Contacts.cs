using System.Collections.Generic;

namespace WebApp.Outlook.Models
{
    public class Contacts
    {
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        public string MobilePhone { get; set; }

        public Contacts(string displayName, IList<Microsoft.Office365.OutlookServices.EmailAddress> emailAddresses, string mobilePhone)
        {
            this.DisplayName = displayName;
            this.EmailAddress = emailAddresses.Count <= 0 ? "" : emailAddresses[0].Address;
            this.MobilePhone = mobilePhone;
        }
    }
}