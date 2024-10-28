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

        [HttpGet("space/{spaceId}")]
        public async Task<ActionResult<List<Snapshot>>> GetAllBySpace(int spaceId)
        {
            return ResponseHelper.HandleErrorAndReturn(await _snapshotService.GetAllSnapshotsBySpaceId(spaceId));
        }

        [HttpGet("folder/{folderId}")]
        public async Task<ActionResult<List<Snapshot>>> GetAllByFolder(int folderId)
        {
            return ResponseHelper.HandleErrorAndReturn(await _snapshotService.GetAllSnapshotsByFolderId(folderId));
        }

        [HttpGet("space/{spaceId}/root")]
        public async Task<ActionResult<List<Snapshot>>> GetAllRootBySpace(int spaceId)
        {
            return ResponseHelper.HandleErrorAndReturn(await _snapshotService.GetAllRootSnapshotsBySpaceId(spaceId));
        }

        [HttpGet("{snapshotId}")]
        public async Task<ActionResult<Snapshot>> GetSingle(int snapshotId)
        {
            return ResponseHelper.HandleErrorAndReturn(await _snapshotService.GetSingleSnapshotById(snapshotId));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Snapshot>> Update(int id, Snapshot snapshot)
        {
            return ResponseHelper.HandleErrorAndReturn(await _snapshotService.UpdateSnapshot(id, snapshot));
        }
    }
}