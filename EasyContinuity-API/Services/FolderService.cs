using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
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

        public async Task<Response<List<Folder>>> GetAllFoldersByParentId(int parentId)
        {
            var folders = await _ecDbContext.Folders.Where(f => f.ParentId == parentId).ToListAsync();

            return Response<List<Folder>>.Success(folders);
        }

        public async Task<Response<Folder>> UpdateFolder(int id, FolderUpdateDTO updatedFolderDTO)
        {
            var existingFolder = await _ecDbContext.Folders.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id);

            if (existingFolder == null)
            {
                return Response<Folder>.Fail(404, "Folder Not Found");
            }

            var folder = new Folder
            {
                Id = id,
                SpaceId = existingFolder.SpaceId,
                ParentId = updatedFolderDTO.ParentId ?? existingFolder.ParentId,
                Name = updatedFolderDTO.Name ?? existingFolder.Name,
                Description = updatedFolderDTO.Description ?? existingFolder.Description,
                IsDeleted = updatedFolderDTO.IsDeleted ?? existingFolder.IsDeleted,
                CreatedBy = existingFolder.CreatedBy,
                CreatedOn = existingFolder.CreatedOn,
                LastUpdatedBy = updatedFolderDTO.LastUpdatedBy ?? existingFolder.LastUpdatedBy,
                LastUpdatedOn = updatedFolderDTO.LastUpdatedOn ?? existingFolder.LastUpdatedOn,
                DeletedOn = updatedFolderDTO.DeletedOn ?? existingFolder.DeletedOn,
                DeletedBy = updatedFolderDTO.DeletedBy ?? existingFolder.DeletedBy
            };

            _ecDbContext.Folders.Update(folder);
            await _ecDbContext.SaveChangesAsync();

            return Response<Folder>.Success(folder);
        }
    }
}