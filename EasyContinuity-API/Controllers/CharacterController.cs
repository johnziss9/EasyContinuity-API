using EasyContinuity_API.Helpers;
using EasyContinuity_API.Interfaces;
using EasyContinuity_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasyContinuity_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CharacterController : ControllerBase
    {
        private readonly ICharacterService _characterService;

        public CharacterController(ICharacterService characterService)
        {
            _characterService = characterService;
        }

        [HttpPost]
        public async Task<ActionResult<Character>> Create(Character character)
        {
            return ResponseHelper.HandleErrorAndReturn(await _characterService.CreateCharacter(character));
        }

        [HttpGet("space/{spaceId}")]
        public async Task<ActionResult<List<Character>>> GetAllBySpace(int spaceId)
        {
            return ResponseHelper.HandleErrorAndReturn(await _characterService.GetAllCharactersBySpaceId(spaceId));
        }

        // [HttpPut("{id}")]
        // public async Task<ActionResult<Space>> Update(int id, SpaceUpdateDto updatedSpaceDTO)
        // {
        //     return ResponseHelper.HandleErrorAndReturn(await _spaceService.UpdateSpace(id, updatedSpaceDTO));
        // }
    }
}