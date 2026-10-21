using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineBookstore.Models
{
    public class OrderHistoryItem
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        // From OrderDetails + Books JOIN
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
    }
}