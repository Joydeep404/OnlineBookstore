using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.ComponentModel.DataAnnotations;

namespace OnlineBookstore.Models
{
    public class BookCreateViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string Author { get; set; }

        [Required]
        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        [Required]
        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Required]
        public int CategoryId { get; set; }

        // ✅ Only for upload — not mapped to DB
        public HttpPostedFileBase ImageFile { get; set; }

        public string Description { get; set; }
    }
}