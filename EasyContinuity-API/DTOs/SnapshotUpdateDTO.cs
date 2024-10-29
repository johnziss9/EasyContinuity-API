namespace EasyContinuity_API.DTOs
{
    public class SnapshotUpdateDTO
    {
        public int? FolderId { get; set; }

        public string? Name { get; set; }
        
        public bool? IsDeleted { get; set; }
        
        public int? LastUpdatedBy { get; set; }
        
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