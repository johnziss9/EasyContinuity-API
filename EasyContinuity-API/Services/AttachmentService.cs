using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
using EasyContinuity_API.Helpers;
using EasyContinuity_API.Interfaces;
using EasyContinuity_API.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Services
{
    public class AttachmentService : IAttachmentService
    {
        private readonly ECDbContext _ecDbContext;

        public AttachmentService(ECDbContext ecDbContext)
        {
            _ecDbContext = ecDbContext;
        }

        public async Task<Response<Attachment>> CreateAttachment(Attachment attachment)
        {
            _ecDbContext.Attachments.Add(attachment);
            await _ecDbContext.SaveChangesAsync();

            return Response<Attachment>.Success(attachment);
        }

        public async Task<Response<List<Attachment>>> GetAllAttachmentsBySpaceId(int spaceId)
        {
            var attachments = await _ecDbContext.Attachments.Where(s => s.SpaceId == spaceId).ToListAsync();

            return Response<List<Attachment>>.Success(attachments);
        }

        public async Task<Response<List<Attachment>>> GetAllAttachmentsByFolderId(int folderId)
        {
            var attachments = await _ecDbContext.Attachments.Where(s => s.FolderId == folderId).ToListAsync();

            return Response<List<Attachment>>.Success(attachments);
        }

        public async Task<Response<List<Attachment>>> GetAllAttachmentsBySnapshotId(int snapshotId)
        {
            var attachments = await _ecDbContext.Attachments.Where(s => s.SnapshotId == snapshotId).ToListAsync();

            return Response<List<Attachment>>.Success(attachments);
        }

        public async Task<Response<List<Attachment>>> GetAllRootAttachmentsBySpaceId(int spaceId)
        {
            var attachments = await _ecDbContext.Attachments.Where(s => s.SpaceId == spaceId && s.FolderId == null).ToListAsync();

            return Response<List<Attachment>>.Success(attachments);
        }

        public async Task<Response<Attachment>> GetSingleAttachmentById(int attachmentId)
        {
            var attachment = await _ecDbContext.Attachments.Where(a => a.Id == attachmentId).FirstOrDefaultAsync();

            if (attachment == null)
            {
                return Response<Attachment>.Fail(404, "Attachment Not Found");
            }

            return Response<Attachment>.Success(attachment);
        }

        public async Task<Response<Attachment>> UpdateAttachment(int id, AttachmentUpdateDTO updatedAttachmentDTO)
        {
            var existingAttachment = await _ecDbContext.Attachments.AsNoTracking().FirstOrDefaultAsync(ea => ea.Id == id);

            if (existingAttachment == null)
            {
                return Response<Attachment>.Fail(404, "Attachment Not Found");
            }

            var attachment = new Attachment
            {
                Id = id,
                SpaceId = existingAttachment.SpaceId,
                SnapshotId = updatedAttachmentDTO.SnapshotId ?? existingAttachment.SnapshotId,
                FolderId = updatedAttachmentDTO.FolderId ?? existingAttachment.FolderId,
                Name = updatedAttachmentDTO.Name ?? existingAttachment.Name,
                Path = updatedAttachmentDTO.Path ?? existingAttachment.Path,
                Size = updatedAttachmentDTO.Size ?? existingAttachment.Size,
                MimeType = updatedAttachmentDTO.MimeType ?? existingAttachment.MimeType,
                IsDeleted = updatedAttachmentDTO.IsDeleted ?? existingAttachment.IsDeleted,
                AddedBy = existingAttachment.AddedBy,
                AddedOn = existingAttachment.AddedOn, 
                LastUpdatedBy = updatedAttachmentDTO.LastUpdatedBy ?? existingAttachment.LastUpdatedBy,
                LastUpdatedOn = updatedAttachmentDTO.LastUpdatedOn ?? existingAttachment.LastUpdatedOn,
                DeletedOn = updatedAttachmentDTO.DeletedOn ?? existingAttachment.DeletedOn,
                DeletedBy = updatedAttachmentDTO.DeletedBy ?? existingAttachment.DeletedBy
            };

            _ecDbContext.Attachments.Update(attachment);
            await _ecDbContext.SaveChangesAsync();

            return Response<Attachment>.Success(attachment);
        }
    }
}