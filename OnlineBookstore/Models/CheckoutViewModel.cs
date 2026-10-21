using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace OnlineBookstore.Models
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }

        public string City { get; set; }
        public string PostalCode { get; set; }
    }
}