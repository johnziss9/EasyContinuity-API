using EasyContinuity_API.Helpers;
using EasyContinuity_API.Interfaces;
using EasyContinuity_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasyContinuity_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserSpaceController : ControllerBase
    {
        private readonly IUserSpaceService _userSpaceService;

        public UserSpaceController(IUserSpaceService userSpaceService)
        {
            _userSpaceService = userSpaceService;
        }

        [HttpPost]
        public async Task<ActionResult<UserSpace>> Create(UserSpace userSpace)
        {
            return ResponseHelper.HandleErrorAndReturn(await _userSpaceService.CreateUserSpace(userSpace));
        }
    }
}