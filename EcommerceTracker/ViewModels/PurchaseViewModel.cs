namespace EcommerceTracker.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    public class PurchaseViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [DisplayName("Category")]
        public string CategoryName { get; set; }
        [DisplayName("Parent category")]
        public string ParentCategoryName { get; set; }
        [DisplayName("Necessary?")]
        public string NecessityDescription { get; set; }
        [DisplayName("Price")]
        [DataType(DataType.Currency)]
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        [DisplayName("Site")]
        public string SiteName { get; set; }
        [DisplayName("Order Date")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}", ApplyFormatInEditMode = true)]
        public DateTime OrderDate { get; set; }
    }
}