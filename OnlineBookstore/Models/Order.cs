using System;
using System.Collections.Generic;

namespace OnlineBookstore.Models
{
    public class Order
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        // ✅ Delivery Address Fields (for Cash on Delivery)
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }

        // Navigation
        public virtual User User { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}