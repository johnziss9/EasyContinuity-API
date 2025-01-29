using EasyContinuity_API.DTOs;
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
        private readonly ICloudinaryStorageService _cloudinaryService;


        public AttachmentController(IAttachmentService attachmentService, ICloudinaryStorageService cloudinaryService)
        {
            _attachmentService = attachmentService;
            _cloudinaryService = cloudinaryService;
        }

        [HttpPost]
        public async Task<ActionResult<Attachment>> Add([FromForm] IFormFile file, [FromForm] int spaceId, [FromForm] int? snapshotId = null, [FromForm] int? folderId = null)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            var uploadResult = await _cloudinaryService.UploadAsync(file);
            if (!uploadResult.IsSuccess || uploadResult.Data == null)
            {
                return StatusCode(uploadResult.StatusCode, uploadResult.Message);
            }

            var attachment = new Attachment
            {
                SpaceId = spaceId,
                SnapshotId = snapshotId,
                FolderId = folderId,
                Name = file.FileName,
                Path = uploadResult.Data,
                Size = file.Length,
                MimeType = file.ContentType,
                IsDeleted = false,
                // AddedBy = null,
                AddedOn = DateTime.UtcNow,
                // LastUpdatedBy = null,
                LastUpdatedOn = null,
                DeletedOn = null,
                // DeletedBy = null
            };

            return ResponseHelper.HandleErrorAndReturn(await _attachmentService.AddAttachment(attachment));
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

        [HttpPut("{id}")]
        public async Task<ActionResult<Attachment>> Update(int id, AttachmentUpdateDTO updatedAttachmentDTO)
        {
            return ResponseHelper.HandleErrorAndReturn(await _attachmentService.UpdateAttachment(id, updatedAttachmentDTO));
        }
    }
}