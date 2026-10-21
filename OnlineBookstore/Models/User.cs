using System.Linq; // Add this
using System.ComponentModel.DataAnnotations;

namespace OnlineBookstore.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required, StringLength(50)]
        public string Username { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        public System.DateTime CreatedAt { get; set; }
    }
}