namespace OnlineBookstore.Models
{
    public class OrderDetail
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        // Navigation
        public virtual Book Book { get; set; }
    }
}