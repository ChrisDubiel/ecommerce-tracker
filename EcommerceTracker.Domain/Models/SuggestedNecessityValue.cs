namespace EcommerceTracker.Domain.Models
{
    public class SuggestedNecessityValue
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public NecessityValue NecessityValue { get; set; }
    }
}
