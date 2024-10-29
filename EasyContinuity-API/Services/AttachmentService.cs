using EasyContinuity_API.Data;
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

        // public async Task<Response<Folder>> UpdateFolder(int id, Folder updatedFolder)
        // {
        //     var folder = await _ecDbContext.Folders.Where(f => f.Id == id).FirstOrDefaultAsync();

        //     if (folder == null)
        //     {
        //         return Response<Folder>.Fail(404, "Folder Not Found");
        //     }

        //     if (updatedFolder.SpaceId != 0 && updatedFolder.SpaceId != folder.SpaceId)
        //     {
        //         folder.SpaceId = updatedFolder.SpaceId;
        //     }

        //     if (updatedFolder.ParentId != null && updatedFolder.ParentId != folder.ParentId)
        //     {
        //         folder.ParentId = updatedFolder.ParentId;
        //     }

        //     if (updatedFolder.Name != null && updatedFolder.Name != folder.Name)
        //     {
        //         folder.Name = updatedFolder.Name;
        //     }

        //     if (updatedFolder.Description != null && updatedFolder.Description != folder.Description)
        //     {
        //         folder.Description = updatedFolder.Description;
        //     }

        //     if (updatedFolder.IsDeleted != folder.IsDeleted)
        //     {
        //         folder.IsDeleted = updatedFolder.IsDeleted;
        //     }

        //     if (updatedFolder.LastUpdatedBy != null && updatedFolder.LastUpdatedBy != folder.LastUpdatedBy)
        //     {
        //         folder.LastUpdatedBy = updatedFolder.LastUpdatedBy;
        //     }

        //     if (updatedFolder.LastUpdatedOn != null && updatedFolder.LastUpdatedOn != folder.LastUpdatedOn)
        //     {
        //         folder.LastUpdatedOn = updatedFolder.LastUpdatedOn;
        //     }

        //     if (updatedFolder.DeletedOn != null && updatedFolder.DeletedOn != folder.DeletedOn)
        //     {
        //         folder.DeletedOn = updatedFolder.DeletedOn;
        //     }

        //     if (updatedFolder.DeletedBy != null && updatedFolder.DeletedBy != folder.DeletedBy)
        //     {
        //         folder.DeletedBy = updatedFolder.DeletedBy;
        //     }

        //     await _ecDbContext.SaveChangesAsync();

        //     return Response<Folder>.Success(folder);
        // }
    }
}