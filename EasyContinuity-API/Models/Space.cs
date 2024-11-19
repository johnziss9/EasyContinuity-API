using System.ComponentModel.DataAnnotations;

namespace EasyContinuity_API.Models
{
    public class Space
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = "";

        public string? Description { get; set; }

        [Required(ErrorMessage = "Type is required.")]
        public string Type { get; set; } = "";

        public bool IsDeleted { get; set; }

        public int CreatedBy { get; set; }

        public int? LastUpdatedBy { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public DateTime? DeletedOn { get; set; }

        public int? DeletedBy { get; set; }
    }
}