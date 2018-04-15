namespace EcommerceTracker.ViewModels
{
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using Domain.Models;

    public class CategoryViewModel      
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [DisplayName("Necessary?")]
        [Required]
        public string NecessityDescription { get; set; }
        [DisplayName("Parent category")]
        public string ParentCategoryName { get; set; }
    }
}