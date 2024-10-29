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

        // [HttpGet("space/{spaceId}")]
        // public async Task<ActionResult<List<Folder>>> GetAllBySpace(int spaceId)
        // {
        //     return ResponseHelper.HandleErrorAndReturn(await _folderService.GetAllFoldersBySpaceId(spaceId));
        // }

        // [HttpPut("{id}")]
        // public async Task<ActionResult<Folder>> Update(int id, Folder folder)
        // {
        //     return ResponseHelper.HandleErrorAndReturn(await _folderService.UpdateFolder(id, folder));
        // }
    }
}