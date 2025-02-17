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
        public async Task<ActionResult<IEnumerable<Attachment>>> Add([FromForm] IFormCollection form, [FromForm] int spaceId, [FromForm] int? snapshotId = null, [FromForm] int? folderId = null)
        {
            var files = form.Files;
            
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files uploaded");
            }

            // Check if adding these files would exceed the snapshot limit
            if (snapshotId.HasValue)
            {
                var existingCount = await _attachmentService.GetAllAttachmentsBySnapshotId(snapshotId.Value);
                if (existingCount?.Data?.Count + files.Count > 6)
                {
                    return BadRequest("Maximum of 6 images per snapshot allowed");
                }
            }

            // Determine the appropriate folder based on the context
            string? uploadFolder = DetermineUploadFolder(snapshotId);

            var attachments = new List<Attachment>();

            foreach (var file in files)
            {
                var uploadResult = await _cloudinaryService.UploadAsync(file, uploadFolder);
                if (!uploadResult.IsSuccess || uploadResult.Data == null)
                {
                    return StatusCode(uploadResult.StatusCode, uploadResult.Message ?? "Upload failed");
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
                    // DeletedBy = null,
                    IsStored = uploadResult.IsSuccess // Basing this on the upload of the attachment result
                };

                var createResult = await _attachmentService.AddAttachment(attachment);
                if (!createResult.IsSuccess || createResult.Data == null)
                {
                    return StatusCode(createResult.StatusCode, createResult.Message);
                }

                attachments.Add(createResult.Data);
            }

            return Ok(attachments);
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

        private string? DetermineUploadFolder(int? snapshotId)
        {
            return snapshotId switch
            {
                not null => "Snapshots",
                _ => null
            };
        }
    }
}