using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
using EasyContinuity_API.Helpers;
using EasyContinuity_API.Interfaces;
using EasyContinuity_API.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Services
{
    public class CharacterService : ICharacterService
    {
        private readonly ECDbContext _ecDbContext;

        public CharacterService(ECDbContext ecDbContext)
        {
            _ecDbContext = ecDbContext;
        }

        public async Task<Response<Character>> CreateCharacter(Character character)
        {
            _ecDbContext.Characters.Add(character);
            await _ecDbContext.SaveChangesAsync();

            return Response<Character>.Success(character);
        }

        public async Task<Response<List<Character>>> GetAllCharactersBySpaceId(int spaceId)
        {
            var characters = await _ecDbContext.Characters.Where(c => c.SpaceId == spaceId).ToListAsync();

            return Response<List<Character>>.Success(characters);
        }

        // public async Task<Response<Space>> UpdateSpace(int id, SpaceUpdateDto updatedSpaceDTO)
        // {
        //     var existingSpace = await _ecDbContext.Spaces.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

        //     if (existingSpace == null)
        //     {
        //         return Response<Space>.Fail(404, "Space Not Found");
        //     }

        //     var space = new Space
        //     {
        //         Id = id,
        //         Name = updatedSpaceDTO.Name ?? existingSpace.Name,
        //         Description = updatedSpaceDTO.Description ?? existingSpace.Description,
        //         IsDeleted = updatedSpaceDTO.IsDeleted ?? existingSpace.IsDeleted,
        //         CreatedBy = existingSpace.CreatedBy,
        //         CreatedOn = existingSpace.CreatedOn,
        //         LastUpdatedBy = updatedSpaceDTO.LastUpdatedBy ?? existingSpace.LastUpdatedBy,
        //         LastUpdatedOn = updatedSpaceDTO.LastUpdatedOn ?? existingSpace.LastUpdatedOn,
        //         DeletedOn = updatedSpaceDTO.DeletedOn ?? existingSpace.DeletedOn,
        //         DeletedBy = updatedSpaceDTO.DeletedBy ?? existingSpace.DeletedBy
        //     };
            
        //     _ecDbContext.Spaces.Update(space);
        //     await _ecDbContext.SaveChangesAsync();

        //     return Response<Space>.Success(space);
        // }
    }
}