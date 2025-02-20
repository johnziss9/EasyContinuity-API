using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EasyContinuity_API.Models
{
    public class Attachment
    {
        public int Id { get; set; }

        public int SpaceId { get; set; }

        public int? SnapshotId { get; set; }

        public int? FolderId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Path is required.")]
        public string Path { get; set; } = "";

        [Required(ErrorMessage = "Size is required.")]
        public long Size { get; set; }

        [Required(ErrorMessage = "MIME Type is required.")]
        public string MimeType { get; set; } = "";

        [NotMapped]
        public string? Url { get; set; }

        public bool IsDeleted { get; set; }

        public int AddedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime AddedOn { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public DateTime? DeletedOn { get; set; }

        public int? DeletedBy { get; set; }

        public bool IsStored { get; set; }
    }
}