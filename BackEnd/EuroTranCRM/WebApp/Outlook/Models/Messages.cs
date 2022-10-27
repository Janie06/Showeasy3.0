using System;

namespace WebApp.Outlook.Models
{
    public class Messages
    {
        public string Subject { get; set; }
        public DateTimeOffset DateTimeReceived { get; set; }
        public string From { get; set; }

        public Messages(string subject, DateTimeOffset? dateTimeReceived,
            Microsoft.Office365.OutlookServices.Recipient from)
        {
            this.Subject = subject;
            this.DateTimeReceived = (DateTimeOffset)dateTimeReceived;
            this.From = from != null ? $"{from.EmailAddress.Name} ({from.EmailAddress.Address})" : "EMPTY";
        }
    }
}