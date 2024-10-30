namespace EasyContinuity_API.DTOs
{
    public class CharacterUpdateDTO
    {        
        public string? Name { get; set; }
                
        public bool? IsDeleted { get; set; }
        
        public int? LastUpdatedBy { get; set; }
        
        public DateTime? LastUpdatedOn { get; set; }
        
        public DateTime? DeletedOn { get; set; }
        
        public int? DeletedBy { get; set; }
    }
}