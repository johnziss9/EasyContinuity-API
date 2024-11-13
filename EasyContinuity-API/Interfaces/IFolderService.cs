using EasyContinuity_API.DTOs;
using EasyContinuity_API.Helpers;
using EasyContinuity_API.Models;

namespace EasyContinuity_API.Interfaces
{
    public interface IFolderService
    {
        Task<Response<Folder>> CreateFolder(Folder folder);

        Task<Response<List<Folder>>> GetAllFoldersBySpaceId(int spaceId);

        Task<Response<List<Folder>>> GetAllFoldersByParentId(int parentId);

        Task<Response<Folder>> UpdateFolder(int id, FolderUpdateDTO updatedFolderDTO);
    }
}