using EasyContinuity_API.Helpers;
using EasyContinuity_API.Models;

namespace EasyContinuity_API.Interfaces
{
    public interface ISnapshotService
    {
        Task<Response<Snapshot>> CreateSnapshot(Snapshot snapshot);

        Task<Response<List<Snapshot>>> GetAllSnapshotsBySpaceId(int spaceId);

        Task<Response<List<Snapshot>>> GetAllSnapshotsByFolderId(int folderId);

        Task<Response<List<Snapshot>>> GetAllRootSnapshotsBySpaceId(int spaceId);

        Task<Response<Snapshot>> GetSingleById(int snapshotId);

        // Task<Response<Space>> UpdateSpace(int id, Space updatedSpace);
    }
}