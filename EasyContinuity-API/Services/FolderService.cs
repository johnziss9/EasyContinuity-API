using EasyContinuity_API.Data;
using EasyContinuity_API.Helpers;
using EasyContinuity_API.Interfaces;
using EasyContinuity_API.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Services
{
    public class FolderService : IFolderService
    {
        private readonly ECDbContext _ecDbContext;

        public FolderService(ECDbContext ecDbContext)
        {
            _ecDbContext = ecDbContext;
        }

        public async Task<Response<Folder>> CreateFolder(Folder folder)
        {
            _ecDbContext.Folders.Add(folder);
            await _ecDbContext.SaveChangesAsync();

            return Response<Folder>.Success(folder);
        }

        public async Task<Response<List<Folder>>> GetAllFoldersBySpaceId(int spaceId)
        {
            var folders = await _ecDbContext.Folders.Where(s => s.SpaceId == spaceId).ToListAsync();

            return Response<List<Folder>>.Success(folders);
        }

        public async Task<Response<Folder>> UpdateFolder(int id, Folder updatedFolder)
        {
            var folder = await _ecDbContext.Folders.Where(f => f.Id == id).FirstOrDefaultAsync();

            if (folder == null)
            {
                return Response<Folder>.Fail(404, "Folder Not Found");
            }

            if (updatedFolder.SpaceId != 0 && updatedFolder.SpaceId != folder.SpaceId)
            {
                folder.SpaceId = updatedFolder.SpaceId;
            }

            if (updatedFolder.ParentId != null && updatedFolder.ParentId != folder.ParentId)
            {
                folder.ParentId = updatedFolder.ParentId;
            }

            if (updatedFolder.Name != null && updatedFolder.Name != folder.Name)
            {
                folder.Name = updatedFolder.Name;
            }

            if (updatedFolder.Description != null && updatedFolder.Description != folder.Description)
            {
                folder.Description = updatedFolder.Description;
            }

            if (updatedFolder.IsDeleted != folder.IsDeleted)
            {
                folder.IsDeleted = updatedFolder.IsDeleted;
            }

            if (updatedFolder.LastUpdatedBy != null && updatedFolder.LastUpdatedBy != folder.LastUpdatedBy)
            {
                folder.LastUpdatedBy = updatedFolder.LastUpdatedBy;
            }

            if (updatedFolder.LastUpdatedOn != null && updatedFolder.LastUpdatedOn != folder.LastUpdatedOn)
            {
                folder.LastUpdatedOn = updatedFolder.LastUpdatedOn;
            }

            if (updatedFolder.DeletedOn != null && updatedFolder.DeletedOn != folder.DeletedOn)
            {
                folder.DeletedOn = updatedFolder.DeletedOn;
            }

            await _ecDbContext.SaveChangesAsync();

            return Response<Folder>.Success(folder);
        }
    }
}