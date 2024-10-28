using EasyContinuity_API.Helpers;
using EasyContinuity_API.Interfaces;
using EasyContinuity_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasyContinuity_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SnapshotController : ControllerBase
    {
        private readonly ISnapshotService _snapshotService;

        public SnapshotController(ISnapshotService snapshotService)
        {
            _snapshotService = snapshotService;
        }

        [HttpPost]
        public async Task<ActionResult<Snapshot>> Create(Snapshot snapshot)
        {
            return ResponseHelper.HandleErrorAndReturn(await _snapshotService.CreateSnapshot(snapshot));
        }

        // [HttpGet]
        // public async Task<ActionResult<List<Space>>> GetAll()
        // {
        //     return ResponseHelper.HandleErrorAndReturn(await _spaceService.GetAllSpaces());
        // }

        // [HttpPut("{id}")]
        // public async Task<ActionResult<Space>> Update(int id, Space space)
        // {
        //     return ResponseHelper.HandleErrorAndReturn(await _spaceService.UpdateSpace(id, space));
        // }
    }
}