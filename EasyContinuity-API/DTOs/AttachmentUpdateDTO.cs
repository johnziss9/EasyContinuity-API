namespace EasyContinuity_API.DTOs
{
    public class AttachmentUpdateDTO
    {
        public int? SnapshotId { get; set; }
        
        public int? FolderId { get; set; }
        
        public string? Name { get; set; }
        
        public string? Path { get; set; }
        
        public int? Size { get; set; }
        
        public string? MimeType { get; set; }
        
        public bool? IsDeleted { get; set; }
        
        public int? LastUpdatedBy { get; set; }
        
        public DateTime? LastUpdatedOn { get; set; }
        
        public DateTime? DeletedOn { get; set; }
        
        public int? DeletedBy { get; set; }
    }
}