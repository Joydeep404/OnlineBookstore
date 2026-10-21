using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;

namespace OnlineBookstore.Models
{
    public class OnlineBookstoreContext : DbContext
    {
        public OnlineBookstoreContext() : base("OnlineBookstoreContext") { }

        public DbSet<User> Users { get; set; }
        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        // ✅ Get books using stored procedure
        public List<Book> GetBooksByCategory(int? categoryId = null)
        {
            var param = categoryId.HasValue
                ? new SqlParameter("@CategoryId", categoryId.Value)
                : new SqlParameter("@CategoryId", System.DBNull.Value);

            return Database.SqlQuery<Book>("EXEC GetBooksByCategory @CategoryId", param).ToList();
        }

        // ✅ Place order using stored procedure and UDT
        public void PlaceOrder(int userId, decimal totalAmount, List<OrderDetail> orderDetails,
    string fullName, string phoneNumber, string address, string city, string postalCode)
        {
            var table = new DataTable();
            table.Columns.Add("BookId", typeof(int));
            table.Columns.Add("Quantity", typeof(int));
            table.Columns.Add("UnitPrice", typeof(decimal));

            foreach (var od in orderDetails)
            {
                table.Rows.Add(od.BookId, od.Quantity, od.UnitPrice);
            }

            var paramTable = new SqlParameter("@OrderDetailTable", SqlDbType.Structured)
            {
                TypeName = "OrderDetailTable",
                Value = table
            };

            Database.ExecuteSqlCommand(
                "EXEC PlaceOrder @UserId, @TotalAmount, @OrderDetailTable, @FullName, @PhoneNumber, @Address, @City, @PostalCode",
                new SqlParameter("@UserId", userId),
                new SqlParameter("@TotalAmount", totalAmount),
                paramTable,
                new SqlParameter("@FullName", fullName),
                new SqlParameter("@PhoneNumber", phoneNumber),
                new SqlParameter("@Address", address),
                new SqlParameter("@City", city),
                new SqlParameter("@PostalCode", postalCode)
            );
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Order>()
                .HasMany(o => o.OrderDetails)
                .WithRequired()
                .HasForeignKey(od => od.OrderId);

            base.OnModelCreating(modelBuilder);
            // 👇 ADD THIS NEW BLOCK FOR Order → User
            modelBuilder.Entity<Order>()
                .HasRequired(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId);

            base.OnModelCreating(modelBuilder);
        }
    }
}