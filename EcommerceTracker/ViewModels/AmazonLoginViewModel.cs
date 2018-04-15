using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EcommerceTracker.ViewModels
{
    using System.ComponentModel.DataAnnotations;

    public class AmazonLoginViewModel
    {
        [Display(Name = "Email Address")]
        public string EmailAddress { get; set; }
        public string Password { get; set; }
    }
}