using System.ComponentModel.DataAnnotations;
using System.Web;

namespace OnlineBookstore.Models
{
    public class Book
    {
        public int BookId { get; set; }

        [Required, StringLength(200)]
        public string Title { get; set; }

        [Required, StringLength(150)]
        public string Author { get; set; }

        [Required, Range(0.01, 10000)]
        public decimal Price { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string ImageUrl { get; set; }

        public string Description { get; set; }

        public System.DateTime CreatedAt { get; set; }

        public virtual Category Category { get; set; }
    }
}