using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
using EasyContinuity_API.Helpers;
using EasyContinuity_API.Interfaces;
using EasyContinuity_API.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Services
{
    public class SpaceService : ISpaceService
    {
        private readonly ECDbContext _ecDbContext;

        public SpaceService(ECDbContext ecDbContext)
        {
            _ecDbContext = ecDbContext;
        }

        public async Task<Response<Space>> CreateSpace(Space space)
        {
            _ecDbContext.Spaces.Add(space);
            await _ecDbContext.SaveChangesAsync();

            return Response<Space>.Success(space);
        }

        public async Task<Response<List<Space>>> GetAllSpaces()
        {
            var spaces = await _ecDbContext.Spaces.ToListAsync();

            return Response<List<Space>>.Success(spaces);
        }

        public async Task<Response<Space>> GetSingleSpaceById(int spaceId)
        {
            var space = await _ecDbContext.Spaces.Where(s => s.Id == spaceId).FirstOrDefaultAsync();

            if (space == null)
            {
                return Response<Space>.Fail(404, "Space Not Found");
            }

            return Response<Space>.Success(space);
        }

        public async Task<Response<List<object>>> SearchContentsBySpace(int spaceId, string query)
        {
            query = query.ToLower().Trim();

            var folders = await _ecDbContext.Folders
                .Where(f => f.SpaceId == spaceId
                        && f.IsDeleted == false
                        && f.Name.ToLower().Contains(query))
                .ToListAsync();

            var snapshots = await _ecDbContext.Snapshots
                .Where(s => s.SpaceId == spaceId
                        && s.IsDeleted == false
                        && s.Name.ToLower().Contains(query))
                .ToListAsync();

            var results = new List<object>();
            results.AddRange(folders);
            results.AddRange(snapshots);

            return Response<List<object>>.Success(results);
        }

        public async Task<Response<Space>> UpdateSpace(int id, SpaceUpdateDTO updatedSpaceDTO)
        {
            var existingSpace = await _ecDbContext.Spaces.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

            if (existingSpace == null)
            {
                return Response<Space>.Fail(404, "Space Not Found");
            }

            var space = new Space
            {
                Id = id,
                Name = updatedSpaceDTO.Name ?? existingSpace.Name,
                Type = updatedSpaceDTO.Type ?? existingSpace.Type,
                Description = updatedSpaceDTO.Description ?? existingSpace.Description,
                IsDeleted = updatedSpaceDTO.IsDeleted ?? existingSpace.IsDeleted,
                CreatedBy = existingSpace.CreatedBy,
                CreatedOn = existingSpace.CreatedOn,
                LastUpdatedBy = updatedSpaceDTO.LastUpdatedBy ?? existingSpace.LastUpdatedBy,
                LastUpdatedOn = updatedSpaceDTO.LastUpdatedOn ?? existingSpace.LastUpdatedOn,
                DeletedOn = updatedSpaceDTO.DeletedOn ?? existingSpace.DeletedOn,
                DeletedBy = updatedSpaceDTO.DeletedBy ?? existingSpace.DeletedBy
            };

            _ecDbContext.Spaces.Update(space);
            await _ecDbContext.SaveChangesAsync();

            return Response<Space>.Success(space);
        }
    }
}