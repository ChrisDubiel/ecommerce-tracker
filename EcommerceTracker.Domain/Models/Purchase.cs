using System;

namespace EcommerceTracker.Domain.Models
{
    using System.ComponentModel;

    public enum NecessityValue
    {
        [Description("Very unecessary")]
        VeryUnecessary,
        [Description("Somewhat unecessary")]
        SomewhatUnecessary,
        [Description("Somewhat necessary")]
        SomewhatNecessary,
        [Description("Very necessary")]
        VeryNecessary
    }

    public class Purchase
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public int? CategoryId { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public int SiteId { get; set; }
        public DateTime OrderDate { get; set; }
        public string OrderReferenceNumber { get; set; }
        public bool UseCategoryNecessityValue { get; set; }
        public NecessityValue NecessityValue { get; set; }

        public virtual Category Category { get; set; }
        public virtual Site Site { get; set; }

        public decimal GetTotal()
        {
            return Quantity * UnitPrice;
        }

        public NecessityValue GetNecessityValue()
        {
            if (Category == null || UseCategoryNecessityValue == false)
                return NecessityValue;
            return Category.GetNecessityValue();
        }
    }
}
