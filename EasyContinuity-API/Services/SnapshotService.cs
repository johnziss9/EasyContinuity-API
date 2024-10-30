using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
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

        public async Task<Response<Snapshot>> UpdateSnapshot(int id, SnapshotUpdateDTO updatedSnapshotDTO)
        {
            var existingSnapshot = await _ecDbContext.Snapshots.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);

            if (existingSnapshot == null)
            {
                return Response<Snapshot>.Fail(404, "Snapshot Not Found");
            }

            var snapshot = new Snapshot
            {
                Id = id,
                SpaceId = existingSnapshot.SpaceId,
                FolderId = updatedSnapshotDTO.FolderId ?? existingSnapshot.FolderId,
                Name = updatedSnapshotDTO.Name ?? existingSnapshot.Name,
                IsDeleted = updatedSnapshotDTO.IsDeleted ?? existingSnapshot.IsDeleted,
                CreatedBy = existingSnapshot.CreatedBy,
                CreatedOn = existingSnapshot.CreatedOn,
                LastUpdatedBy = updatedSnapshotDTO.LastUpdatedBy ?? existingSnapshot.LastUpdatedBy,
                LastUpdatedOn = updatedSnapshotDTO.LastUpdatedOn ?? existingSnapshot.LastUpdatedOn,
                DeletedOn = updatedSnapshotDTO.DeletedOn ?? existingSnapshot.DeletedOn,
                DeletedBy = updatedSnapshotDTO.DeletedBy ?? existingSnapshot.DeletedBy,
                Episode = updatedSnapshotDTO.Episode ?? existingSnapshot.Episode,
                Scene = updatedSnapshotDTO.Scene ?? existingSnapshot.Scene,
                StoryDay = updatedSnapshotDTO.StoryDay ?? existingSnapshot.StoryDay,
                Character = updatedSnapshotDTO.Character ?? existingSnapshot.Character,
                Notes = updatedSnapshotDTO.Notes ?? existingSnapshot.Notes,
                Skin = updatedSnapshotDTO.Skin ?? existingSnapshot.Skin,
                Brows = updatedSnapshotDTO.Brows ?? existingSnapshot.Brows,
                Eyes = updatedSnapshotDTO.Eyes ?? existingSnapshot.Eyes,
                Lips = updatedSnapshotDTO.Lips ?? existingSnapshot.Lips,
                Effects = updatedSnapshotDTO.Effects ?? existingSnapshot.Effects,
                MakeupNotes = updatedSnapshotDTO.MakeupNotes ?? existingSnapshot.MakeupNotes,
                Prep = updatedSnapshotDTO.Prep ?? existingSnapshot.Prep,
                Method = updatedSnapshotDTO.Method ?? existingSnapshot.Method,
                StylingTools = updatedSnapshotDTO.StylingTools ?? existingSnapshot.StylingTools,
                Products = updatedSnapshotDTO.Products ?? existingSnapshot.Products,
                HairNotes = updatedSnapshotDTO.HairNotes ?? existingSnapshot.HairNotes
            };

            _ecDbContext.Snapshots.Update(snapshot);
            await _ecDbContext.SaveChangesAsync();

            return Response<Snapshot>.Success(snapshot);
        }
    }
}