namespace EcommerceTracker.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using Domain.Models;

    public class EditPurchaseViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        [Display(Name = "Category")]
        public int? CategoryId { get; set; }
        [Display(Name = "Price")]
        public double UnitPrice { get; set; }
        public int Quantity { get; set; }
        [Display(Name = "Website")]
        public int SiteId { get; set; }
        [Display(Name = "Order date")]
        public DateTime OrderDate { get; set; }
        [Display(Name = "Order reference number")]
        public string OrderReferenceNumber { get; set; }
        [Display(Name = "Use category necessity value?")]
        public bool UseCategoryNecessityValue { get; set; }
        [Display(Name = "Necessity value")]
        public NecessityValue NecessityValue { get; set; }

        public IEnumerable<SelectListItem> Categories { get; set; }
        public IEnumerable<SelectListItem> Sites { get; set; }
        public IEnumerable<SelectListItem> NecessityValues { get; set; }
    }
}