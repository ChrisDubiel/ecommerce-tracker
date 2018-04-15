namespace EcommerceTracker.Domain.Models
{
    using System;

    public class TrackedPurchaseFile
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FileName { get; set; }
        public string FolderName { get; set; }
        public string Website { get; set; }
        public DateTime ImportDate { get; set; }
    }
}
