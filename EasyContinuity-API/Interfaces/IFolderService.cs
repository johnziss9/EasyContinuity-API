using EasyContinuity_API.Helpers;
using EasyContinuity_API.Models;

namespace EasyContinuity_API.Interfaces
{
    public interface IFolderService
    {
        Task<Response<Folder>> CreateFolder(Folder folder);

        Task<Response<List<Folder>>> GetAllFoldersBySpaceId(int spaceId);

        Task<Response<Folder>> UpdateFolder(int id, Folder updatedFolder);
    }
}