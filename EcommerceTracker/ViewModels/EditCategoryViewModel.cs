namespace EcommerceTracker.ViewModels
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using Domain.Models;

    public class EditCategoryViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        [Required]
        public string Name { get; set; }
        [Display(Name = "Parent category")]
        public int? ParentCategoryId { get; set; }
        [Display(Name = "Use parent category necessity value?")]
        public bool UseParentCategoryNecessityValue { get; set; }
        [Display(Name = "Necessity value")]
        [Required]
        public NecessityValue NecessityValue { get; set; }

        public IEnumerable<SelectListItem> ParentCategories { get; set; }
        public IEnumerable<SelectListItem> NecessityValues { get; set; }
    }
}