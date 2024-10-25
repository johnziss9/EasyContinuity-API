using EasyContinuity_API.Data;
using EasyContinuity_API.Helpers;
using EasyContinuity_API.Interfaces;
using EasyContinuity_API.Models;

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

        // public async Task<Response<List<Space>>> GetAllSpaces()
        // {
        //     var spaces = await _ecDbContext.Spaces.ToListAsync();

        //     return Response<List<Space>>.Success(spaces);
        // }

        // public async Task<Response<Space>> UpdateSpace(int id, Space updatedSpace)
        // {
        //     var space = await _ecDbContext.Spaces.Where(s => s.Id == id).FirstOrDefaultAsync();

        //     if (space == null)
        //     {
        //         return Response<Space>.Fail(404, "Space Not Found");
        //     }

        //     if (updatedSpace.Name != null && updatedSpace.Name != space.Name)
        //     {
        //         space.Name = updatedSpace.Name;
        //     }

        //     if (updatedSpace.Description != null && updatedSpace.Description != space.Description)
        //     {
        //         space.Description = updatedSpace.Description;
        //     }

        //     if (updatedSpace.IsDeleted != space.IsDeleted)
        //     {
        //         space.IsDeleted = updatedSpace.IsDeleted;
        //     }

        //     if (updatedSpace.LastUpdatedBy != null && updatedSpace.LastUpdatedBy != space.LastUpdatedBy)
        //     {
        //         space.LastUpdatedBy = updatedSpace.LastUpdatedBy;
        //     }

        //     if (updatedSpace.LastUpdatedOn != null && updatedSpace.LastUpdatedOn != space.LastUpdatedOn)
        //     {
        //         space.LastUpdatedOn = updatedSpace.LastUpdatedOn;
        //     }

        //     if (updatedSpace.DeletedOn != null && updatedSpace.DeletedOn != space.DeletedOn)
        //     {
        //         space.DeletedOn = updatedSpace.DeletedOn;
        //     }

        //     await _ecDbContext.SaveChangesAsync();

        //     return Response<Space>.Success(space);
        // }
    }
}