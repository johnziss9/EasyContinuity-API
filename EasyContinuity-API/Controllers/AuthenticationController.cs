using EasyContinuity_API.DTOs;
using EasyContinuity_API.Helpers;
using EasyContinuity_API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EasyContinuity_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid) // Check if the fields are valid like [Required] or [EmailAddress]
            {
                var errors = ModelState
                    .SelectMany(x => x.Value?.Errors ?? new())
                    .Select(x => x.ErrorMessage)
                    .ToList();
                
                return ResponseHelper.HandleErrorAndReturn(Response<UserDto>.ValidationError(errors));
            }

            var result = await _authenticationService.Register(registerDto);

            return ResponseHelper.HandleErrorAndReturn(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid) // Check if the fields are valid like [Required] or [EmailAddress]
            {
                var errors = ModelState
                    .SelectMany(x => x.Value?.Errors ?? new())
                    .Select(x => x.ErrorMessage)
                    .ToList();
                
                return ResponseHelper.HandleErrorAndReturn(Response<UserDto>.ValidationError(errors));
            }

            var result = await _authenticationService.Login(loginDto);

            return ResponseHelper.HandleErrorAndReturn(result);
        }
    }
}