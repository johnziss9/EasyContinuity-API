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

        public async Task<Response<Space>> UpdateSpace(int id, SpaceUpdateDto updatedSpaceDTO)
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