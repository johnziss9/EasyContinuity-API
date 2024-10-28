using EasyContinuity_API.Helpers;
using EasyContinuity_API.Models;

namespace EasyContinuity_API.Interfaces
{
    public interface ISnapshotService
    {
        Task<Response<Snapshot>> CreateSnapshot(Snapshot snapshot);

        // Task<Response<List<Space>>> GetAllSpaces();

        // Task<Response<Space>> UpdateSpace(int id, Space updatedSpace);
    }
}