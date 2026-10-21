using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OnlineBookstore.Models
{
    public class OrderItemDetail
    {
        public int OrderId { get; set; }
        public int Quantity { get; set; }
        public string Title { get; set; }
        public decimal UnitPrice { get; set; }
    }
}