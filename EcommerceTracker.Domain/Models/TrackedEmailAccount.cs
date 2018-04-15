namespace EcommerceTracker.Domain.Models
{
    using System;

    public enum EmailAccountType { Imap, Gmail }

    public class TrackedEmailAccount
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string EmailAddress { get; set; }
        public EmailAccountType EmailAccountType { get; set; }
        public DateTime? LastScanned { get; set; }
    }
}