
using System.ComponentModel.DataAnnotations;

namespace EasyContinuity_API.Models
{
    public class UserSpace
    {
        public int UserId { get; set; }

        public int SpaceId { get; set; }

        public DateTime JoinedOn { get; set; } = DateTime.UtcNow;
        
        [Required(ErrorMessage = "Role is required.")]
        public string Role { get; set; } = ""; // For now this would be creator if they have created the space or member is they just joined a space
    }
}