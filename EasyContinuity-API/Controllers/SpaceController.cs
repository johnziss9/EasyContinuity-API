using EasyContinuity_API.DTOs;
using EasyContinuity_API.Helpers;
using EasyContinuity_API.Interfaces;
using EasyContinuity_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasyContinuity_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SpaceController : ControllerBase
    {
        private readonly ISpaceService _spaceService;

        public SpaceController(ISpaceService spaceService)
        {
            _spaceService = spaceService;
        }

        [HttpPost]
        public async Task<ActionResult<Space>> Create(Space space)
        {
            return ResponseHelper.HandleErrorAndReturn(await _spaceService.CreateSpace(space));
        }

        [HttpGet]
        public async Task<ActionResult<List<Space>>> GetAll()
        {
            return ResponseHelper.HandleErrorAndReturn(await _spaceService.GetAllSpaces());
        }

        [HttpGet("{spaceId}")]
        public async Task<ActionResult<Space>> GetSingle(int spaceId)
        {
            return ResponseHelper.HandleErrorAndReturn(await _spaceService.GetSingleSpaceById(spaceId));
        }

        [HttpGet("{spaceId}/search")]
        public async Task<ActionResult<List<object>>> SearchContents(int spaceId, [FromQuery] string query)
        {
            return ResponseHelper.HandleErrorAndReturn(await _spaceService.SearchContentsBySpace(spaceId, query));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Space>> Update(int id, SpaceUpdateDTO updatedSpaceDTO)
        {
            return ResponseHelper.HandleErrorAndReturn(await _spaceService.UpdateSpace(id, updatedSpaceDTO));
        }
    }
}