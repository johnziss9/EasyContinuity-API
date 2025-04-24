using EasyContinuity_API.DTOs;
using EasyContinuity_API.Helpers;
using EasyContinuity_API.Models;

namespace EasyContinuity_API.Interfaces
{
    public interface ISpaceService
    {
        Task<Response<Space>> CreateSpace(Space space);

        Task<Response<List<Space>>> GetAllSpacesByUserId(int userId);

        Task<Response<List<Space>>> GetAllSpaces();

        Task<Response<Space>> GetSingleSpaceById(int spaceId);

        Task<Response<List<object>>> SearchContentsBySpace(int spaceId, string query);

        Task<Response<Space>> UpdateSpace(int id, SpaceUpdateDTO updatedSpaceDTO);
    }
}