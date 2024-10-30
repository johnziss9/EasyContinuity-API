using EasyContinuity_API.DTOs;
using EasyContinuity_API.Helpers;
using EasyContinuity_API.Models;

namespace EasyContinuity_API.Interfaces
{
    public interface ICharacterService
    {
        Task<Response<Character>> CreateCharacter(Character character);

        Task<Response<List<Character>>> GetAllCharactersBySpaceId(int spaceId);

        // Task<Response<Space>> UpdateSpace(int id, SpaceUpdateDto updatedSpaceDTO);
    }
}