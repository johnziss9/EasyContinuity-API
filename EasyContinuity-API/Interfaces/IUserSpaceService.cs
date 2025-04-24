using EasyContinuity_API.Helpers;
using EasyContinuity_API.Models;

namespace EasyContinuity_API.Interfaces
{
    public interface IUserSpaceService
    {
        Task<Response<UserSpace>> CreateUserSpace(UserSpace userSpace);
    }
}