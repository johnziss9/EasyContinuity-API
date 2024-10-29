using EasyContinuity_API.Helpers;
using EasyContinuity_API.Interfaces;
using EasyContinuity_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasyContinuity_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AttachmentController : ControllerBase
    {
        private readonly IAttachmentService _attachmentService;

        public AttachmentController(IAttachmentService attachmentService)
        {
            _attachmentService = attachmentService;
        }

        [HttpPost]
        public async Task<ActionResult<Attachment>> Create(Attachment attachment)
        {
            return ResponseHelper.HandleErrorAndReturn(await _attachmentService.CreateAttachment(attachment));
        }

        [HttpGet("space/{spaceId}")]
        public async Task<ActionResult<List<Attachment>>> GetAllBySpace(int spaceId)
        {
            return ResponseHelper.HandleErrorAndReturn(await _attachmentService.GetAllAttachmentsBySpaceId(spaceId));
        }

        [HttpGet("folder/{folderId}")]
        public async Task<ActionResult<List<Attachment>>> GetAllByFolder(int folderId)
        {
            return ResponseHelper.HandleErrorAndReturn(await _attachmentService.GetAllAttachmentsByFolderId(folderId));
        }

        [HttpGet("snapshot/{snapshotId}")]
        public async Task<ActionResult<List<Attachment>>> GetAllBySnapshot(int snapshotId)
        {
            return ResponseHelper.HandleErrorAndReturn(await _attachmentService.GetAllAttachmentsBySnapshotId(snapshotId));
        }

        [HttpGet("space/{spaceId}/root")]
        public async Task<ActionResult<List<Attachment>>> GetAllRootBySpace(int spaceId)
        {
            return ResponseHelper.HandleErrorAndReturn(await _attachmentService.GetAllRootAttachmentsBySpaceId(spaceId));
        }

        [HttpGet("{attachmentId}")]
        public async Task<ActionResult<Attachment>> GetSingle(int attachmentId)
        {
            return ResponseHelper.HandleErrorAndReturn(await _attachmentService.GetSingleAttachmentById(attachmentId));
        }

        // [HttpPut("{id}")]
        // public async Task<ActionResult<Folder>> Update(int id, Folder folder)
        // {
        //     return ResponseHelper.HandleErrorAndReturn(await _folderService.UpdateFolder(id, folder));
        // }
    }
}