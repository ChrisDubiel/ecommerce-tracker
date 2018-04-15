using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EcommerceTracker.Domain.Models
{
    public class EmailMessage
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Key]
        public string Identifier { get; set; }
        public int TrackedEmailAccountId { get; set; }
        public bool? Scanned { get; set; }
    }
}
