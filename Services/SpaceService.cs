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
    }
}