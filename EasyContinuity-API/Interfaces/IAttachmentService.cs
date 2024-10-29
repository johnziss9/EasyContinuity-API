using EasyContinuity_API.Helpers;
using EasyContinuity_API.Models;

namespace EasyContinuity_API.Interfaces
{
    public interface IAttachmentService
    {
        Task<Response<Attachment>> CreateAttachment(Attachment attachment);

        // Task<Response<List<Folder>>> GetAllFoldersBySpaceId(int spaceId);

        // Task<Response<Folder>> UpdateFolder(int id, Folder updatedFolder);
    }
}