using EasyContinuity_API.DTOs;
using EasyContinuity_API.Helpers;

namespace EasyContinuity_API.Interfaces
{
    public interface IAuthenticationService
    {
        Task<Response<UserDto>> Register(RegisterDto registerDto);
        
        Task<Response<UserDto>> Login(LoginDto loginDto);
    }
}