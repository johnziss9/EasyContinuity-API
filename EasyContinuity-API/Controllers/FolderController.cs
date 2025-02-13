using EasyContinuity_API.DTOs;
using EasyContinuity_API.Helpers;
using EasyContinuity_API.Interfaces;
using EasyContinuity_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace EasyContinuity_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FolderController : ControllerBase
    {
        private readonly IFolderService _folderService;

        public FolderController(IFolderService folderService)
        {
            _folderService = folderService;
        }

        [HttpPost]
        public async Task<ActionResult<Folder>> Create(Folder folder)
        {
            return ResponseHelper.HandleErrorAndReturn(await _folderService.CreateFolder(folder));
        }

        [HttpGet("space/{spaceId}")]
        public async Task<ActionResult<List<Folder>>> GetAllBySpace(int spaceId)
        {
            return ResponseHelper.HandleErrorAndReturn(await _folderService.GetAllFoldersBySpaceId(spaceId));
        }

        [HttpGet("parent/{parentId}")]
        public async Task<ActionResult<List<Folder>>> GetAllByParent(int parentId)
        {
            return ResponseHelper.HandleErrorAndReturn(await _folderService.GetAllFoldersByParentId(parentId));
        }

        [HttpGet("{folderId}")]
        public async Task<ActionResult<Folder>> GetSingle(int folderId)
        {
            return ResponseHelper.HandleErrorAndReturn(await _folderService.GetSingleFolderById(folderId));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Folder>> Update(int id, FolderUpdateDTO updatedFolderDTO)
        {
            return ResponseHelper.HandleErrorAndReturn(await _folderService.UpdateFolder(id, updatedFolderDTO));
        }
    }
}