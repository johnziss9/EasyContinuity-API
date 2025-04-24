using EasyContinuity_API.Data;
using EasyContinuity_API.Helpers;
using EasyContinuity_API.Interfaces;
using EasyContinuity_API.Models;

namespace EasyContinuity_API.Services
{
    public class UserSpaceService : IUserSpaceService
    {
        private readonly ECDbContext _ecDbContext;

        public UserSpaceService(ECDbContext ecDbContext)
        {
            _ecDbContext = ecDbContext;
        }

        public async Task<Response<UserSpace>> CreateUserSpace(UserSpace userSpace)
        {
            _ecDbContext.UserSpaces.Add(userSpace);
            await _ecDbContext.SaveChangesAsync();

            return Response<UserSpace>.Success(userSpace);
        }
    }
}