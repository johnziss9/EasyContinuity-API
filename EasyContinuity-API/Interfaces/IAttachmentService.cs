using EasyContinuity_API.DTOs;
using EasyContinuity_API.Helpers;
using EasyContinuity_API.Models;

namespace EasyContinuity_API.Interfaces
{
    public interface IAttachmentService
    {
        Task<Response<Attachment>> CreateAttachment(Attachment attachment);

        Task<Response<List<Attachment>>> GetAllAttachmentsBySpaceId(int spaceId);

        Task<Response<List<Attachment>>> GetAllAttachmentsByFolderId(int folderId);

        Task<Response<List<Attachment>>> GetAllAttachmentsBySnapshotId(int snapshotId);

        Task<Response<List<Attachment>>> GetAllRootAttachmentsBySpaceId(int spaceId);

        Task<Response<Attachment>> GetSingleAttachmentById(int attachmentId);

        Task<Response<Attachment>> UpdateAttachment(int id, AttachmentUpdateDTO updatedAttachmentDTO);
    }
}