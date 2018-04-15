namespace EcommerceTracker.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Domain.Models;

    public class ReportViewModel
    {
        public ReportType ReportType { get; set; }
        public Chart Chart { get; set; }
        public string OldestOrderDate { get; set; }
        public string NewestOrderDate { get; set; }
        [DisplayName("Start Date")]
        public string StartDateFilter { get; set; }
        [DisplayName("End Date")]
        public string EndDateFilter { get; set; }
        public bool ShowCategoryFilter { get; set; }
        public bool ShowNecessityValueFilter { get; set; }
        [DisplayName("Categories")]
        public List<CheckModel> CategoryCheckModels { get; set; }
        [DisplayName("Necessities")]
        public List<CheckModel> NecessityValueCheckModels { get; set; }
    }
}