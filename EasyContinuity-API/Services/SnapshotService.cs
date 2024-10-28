using EasyContinuity_API.Data;
using EasyContinuity_API.Helpers;
using EasyContinuity_API.Interfaces;
using EasyContinuity_API.Models;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Services
{
    public class SnapshotService : ISnapshotService
    {
        private readonly ECDbContext _ecDbContext;

        public SnapshotService(ECDbContext ecDbContext)
        {
            _ecDbContext = ecDbContext;
        }

        public async Task<Response<Snapshot>> CreateSnapshot(Snapshot snapshot)
        {
            _ecDbContext.Snapshots.Add(snapshot);
            await _ecDbContext.SaveChangesAsync();

            return Response<Snapshot>.Success(snapshot);
        }

        public async Task<Response<List<Snapshot>>> GetAllSnapshotsBySpaceId(int spaceId)
        {
            var snapshots = await _ecDbContext.Snapshots.Where(s => s.SpaceId == spaceId).ToListAsync();

            return Response<List<Snapshot>>.Success(snapshots);
        }

        public async Task<Response<List<Snapshot>>> GetAllSnapshotsByFolderId(int folderId)
        {
            var snapshots = await _ecDbContext.Snapshots.Where(s => s.FolderId == folderId).ToListAsync();

            return Response<List<Snapshot>>.Success(snapshots);
        }

        public async Task<Response<List<Snapshot>>> GetAllRootSnapshotsBySpaceId(int spaceId)
        {
            var snapshots = await _ecDbContext.Snapshots.Where(s => s.SpaceId == spaceId && s.FolderId == null).ToListAsync();

            return Response<List<Snapshot>>.Success(snapshots);
        }

        public async Task<Response<Snapshot>> GetSingleSnapshotById(int snapshotId)
        {
            var snapshot = await _ecDbContext.Snapshots.Where(s => s.Id == snapshotId).FirstOrDefaultAsync();

            if (snapshot == null)
            {
                return Response<Snapshot>.Fail(404, "Snapshot Not Found");
            }

            return Response<Snapshot>.Success(snapshot);
        }

        // public async Task<Response<Space>> UpdateSpace(int id, Space updatedSpace)
        // {
        //     var space = await _ecDbContext.Spaces.Where(s => s.Id == id).FirstOrDefaultAsync();

        //     if (space == null)
        //     {
        //         return Response<Space>.Fail(404, "Space Not Found");
        //     }

        //     if (updatedSpace.Name != null && updatedSpace.Name != space.Name)
        //     {
        //         space.Name = updatedSpace.Name;
        //     }

        //     if (updatedSpace.Description != null && updatedSpace.Description != space.Description)
        //     {
        //         space.Description = updatedSpace.Description;
        //     }

        //     if (updatedSpace.IsDeleted != space.IsDeleted)
        //     {
        //         space.IsDeleted = updatedSpace.IsDeleted;
        //     }

        //     if (updatedSpace.LastUpdatedBy != null && updatedSpace.LastUpdatedBy != space.LastUpdatedBy)
        //     {
        //         space.LastUpdatedBy = updatedSpace.LastUpdatedBy;
        //     }

        //     if (updatedSpace.LastUpdatedOn != null && updatedSpace.LastUpdatedOn != space.LastUpdatedOn)
        //     {
        //         space.LastUpdatedOn = updatedSpace.LastUpdatedOn;
        //     }

        //     if (updatedSpace.DeletedOn != null && updatedSpace.DeletedOn != space.DeletedOn)
        //     {
        //         space.DeletedOn = updatedSpace.DeletedOn;
        //     }

        //     await _ecDbContext.SaveChangesAsync();

        //     return Response<Space>.Success(space);
        // }

    }
}