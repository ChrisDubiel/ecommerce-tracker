using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EcommerceTracker.ViewModels
{
    using System.ComponentModel;
    using System.Web.Mvc;
    using Domain.Models;

    public class EditAlertViewModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        [DisplayName("Alert Type")]
        public AlertType AlertType { get; set; }
        [DisplayName("Choose Categories")]
        public List<CheckModel> CategoryCheckModels { get; set; }
        [DisplayName("Choose Necessities")]
        public List<CheckModel> NecessityValueCheckModels { get; set; }
        [DisplayName("Amount")]
        public decimal Amount { get; set; }

        public IEnumerable<SelectListItem> AlertTypes { get; set; }
    }
}