namespace EasyContinuity_API.DTOs
{
    public class SpaceUpdateDTO
    {        
        public string Name { get; set; } = "";
        
        public string? Description { get; set; }

        public string Type { get; set; } = "";
        
        public bool? IsDeleted { get; set; }
        
        public int? LastUpdatedBy { get; set; }
        
        public DateTime? LastUpdatedOn { get; set; }
        
        public DateTime? DeletedOn { get; set; }
        
        public int? DeletedBy { get; set; }
    }
}