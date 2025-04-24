
namespace EasyContinuity_API.Models
{
    public class UserSpace
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int SpaceId { get; set; }
        
        public SpaceRole Role { get; set; }

        public int AddedBy { get; set; }

        public DateTime AddedOn { get; set; }

        public int LastUpdatedBy { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public InvitationStatus InvitationStatus { get; set; }

        public string InvitationToken { get; set; } = String.Empty;

        public DateTime? InvitationExpiresOn { get; set; }

        public DateTime? LastAccessedOn { get; set; }
    }
}