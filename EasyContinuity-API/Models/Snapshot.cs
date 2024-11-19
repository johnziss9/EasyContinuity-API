using System.ComponentModel.DataAnnotations;

namespace EasyContinuity_API.Models
{
    public class Snapshot
    {
        public int Id { get; set; }

        public int SpaceId { get; set; }

        public int? FolderId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = "";

        public bool IsDeleted { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public DateTime? DeletedOn { get; set; }

        public int? DeletedBy { get; set; }

        public string? Episode { get; set; }

        public int? Scene { get; set; }

        public int? StoryDay { get; set; }

        public int? Character { get; set; }

        public string? Notes { get; set; }

        public string? Skin { get; set; }

        public string? Brows { get; set; }

        public string? Eyes { get; set; }

        public string? Lips { get; set; }

        public string? Effects { get; set; }

        public string? MakeupNotes { get; set; }

        public string? Prep { get; set; }

        public string? Method { get; set; }

        public string? StylingTools { get; set; }

        public string? Products { get; set; }

        public string? HairNotes { get; set; }
    }
}