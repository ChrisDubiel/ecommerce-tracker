namespace EcommerceTracker.Domain.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using Newtonsoft.Json;

    public enum AlertType
    {
        [Description("Category")]
        Category,
        [Description("Necessity")]
        Necessity
    }

    public abstract class Alert
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Description { get; set; }
        public AlertType AlertType { get; set; }
        public decimal CostThreshold { get; set; }
        public int NumberOfMonths { get; set; }
    }

    [Table("CategoryAlerts")]
    public class CategoryAlert : Alert
    {
        [NotMapped]
        public List<int> CategoryIds { get; set; } = new List<int>();

        /// <summary> <see cref="CategoryIds"/> for database persistence. </summary>
        [Obsolete("Only for Persistence by EntityFramework")]
        public string CategoryIdsJsonForDb
        {
            get => CategoryIds == null || !CategoryIds.Any()
                ? null
                : JsonConvert.SerializeObject(CategoryIds);
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    CategoryIds.Clear();
                else
                    CategoryIds = JsonConvert.DeserializeObject<List<int>>(value);
            }
        }
    }

    [Table("NecessityAlerts")]
    public class NecessityAlert : Alert
    {
        [NotMapped]
        public List<int> NecessityValueIds { get; set; } = new List<int>();

        /// <summary> <see cref="NecessityValueIds"/> for database persistence. </summary>
        [Obsolete("Only for Persistence by EntityFramework")]
        public string NecessityValueIdsJsonForDb
        {
            get => NecessityValueIds == null || !NecessityValueIds.Any()
                ? null
                : JsonConvert.SerializeObject(NecessityValueIds);
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    NecessityValueIds.Clear();
                else
                    NecessityValueIds = JsonConvert.DeserializeObject<List<int>>(value);
            }
        }
    }
}
