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

        public async Task<Response<Character>> UpdateCharacter(int id, CharacterUpdateDTO updatedCharacterDTO)
        {
            var existingCharacter = await _ecDbContext.Characters.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

            if (existingCharacter == null)
            {
                return Response<Character>.Fail(404, "Space Not Found");
            }

            var character = new Character
            {
                Id = id,
                SpaceId = existingCharacter.SpaceId,
                Name = updatedCharacterDTO.Name ?? existingCharacter.Name,
                IsDeleted = updatedCharacterDTO.IsDeleted ?? existingCharacter.IsDeleted,
                CreatedBy = existingCharacter.CreatedBy,
                CreatedOn = existingCharacter.CreatedOn,
                LastUpdatedBy = updatedCharacterDTO.LastUpdatedBy ?? existingCharacter.LastUpdatedBy,
                LastUpdatedOn = updatedCharacterDTO.LastUpdatedOn ?? existingCharacter.LastUpdatedOn,
                DeletedOn = updatedCharacterDTO.DeletedOn ?? existingCharacter.DeletedOn,
                DeletedBy = updatedCharacterDTO.DeletedBy ?? existingCharacter.DeletedBy
            };
            
            _ecDbContext.Characters.Update(character);
            await _ecDbContext.SaveChangesAsync();

            return Response<Character>.Success(character);
        }
    }
}