using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EcommerceTracker.ViewModels
{
    using System.ComponentModel;
    using Domain.Models;

    public class AlertViewModel
    {
        public int Id { get; set; }
        [DisplayName("Type")]
        public AlertType AlertType { get; set; }
        [DisplayName("Categories or Necessities")]
        public List<string> CategoryOrNecessityNames { get; set; }
        [DisplayName("Current Amount")]
        public decimal CurrentAmount { get; set; }
        [DisplayName("Threshold Amount")]
        public decimal ThresholdAmount { get; set; }
        [DisplayName("Progress")]
        public decimal Progress { get; set; }
    }
}