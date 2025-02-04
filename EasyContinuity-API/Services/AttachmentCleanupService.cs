using EasyContinuity_API.Data;
using Microsoft.EntityFrameworkCore;

public class AttachmentCleanupService
{
    private readonly ECDbContext _ecDbContext;
    private readonly ICloudinaryStorageService _cloudinaryService;

    public AttachmentCleanupService(ECDbContext ecDbContext, ICloudinaryStorageService cloudinaryService)
    {
        _ecDbContext = ecDbContext;
        _cloudinaryService = cloudinaryService;
    }

    public async Task CleanupDeletedAttachments()
    {
        var query = _ecDbContext.Attachments
            .IgnoreQueryFilters()
            .Where(a => a.IsDeleted && a.IsStored);

        var deletedAttachments = await query.ToListAsync();

        foreach (var attachment in deletedAttachments)
        {
            try
            {
                var deleteResult = await _cloudinaryService.DeleteAsync(attachment.Path);
                if (deleteResult.IsSuccess)
                {
                    attachment.IsStored = false;
                    await _ecDbContext.SaveChangesAsync();
                }
                else
                {
                    Console.WriteLine($"❌ Failed to delete from Cloudinary: Attachment {attachment.Id} - {deleteResult.Message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting from Cloudinary: Attachment {attachment.Id} - {ex.Message}");
            }
        }
    }
}