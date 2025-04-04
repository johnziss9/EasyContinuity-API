using System.ComponentModel.DataAnnotations;

namespace EasyContinuity_API.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "First Name is required.")]
        public string FirstName { get; set; } = "";

        [Required(ErrorMessage = "Last Name is required.")]
        public string LastName { get; set; } = "";

        public byte[] PasswordHash { get; set; } = Array.Empty<byte>();
        
        public byte[] PasswordSalt { get; set; } = Array.Empty<byte>();

        public bool IsDeleted { get; set; }

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedOn { get; set; }

        public DateTime? LastLoginOn { get; set; }
    }
}