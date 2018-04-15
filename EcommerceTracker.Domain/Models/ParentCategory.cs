namespace EcommerceTracker.Domain.Models
{
    public class ParentCategory
    {
        public int Id { get; set; }
        public int ParentCategoryId { get; set; }
        public int ChildCategoryId { get; set; }
    }
}
