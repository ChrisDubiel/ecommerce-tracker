namespace EcommerceTracker.Domain.Models
{
    public class Category
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Name { get; set; }
        public NecessityValue NecessityValue { get; set; }
        public int? ParentCategoryId { get; set; }
        public bool UseParentCategoryNecessityValue { get; set; }

        public virtual Category ParentCategory { get; set; }

        public NecessityValue GetNecessityValue()
        {
            if (ParentCategory == null || UseParentCategoryNecessityValue == false)
                return NecessityValue;
            return ParentCategory.NecessityValue;
        }
    }
}