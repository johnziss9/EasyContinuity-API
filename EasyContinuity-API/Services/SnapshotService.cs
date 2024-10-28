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

        public async Task<Response<Snapshot>> UpdateSnapshot(int id, Snapshot updatedSnapshot)
        {
            var snapshot = await _ecDbContext.Snapshots.Where(s => s.Id == id).FirstOrDefaultAsync();

            if (snapshot == null)
            {
                return Response<Snapshot>.Fail(404, "Snapshot Not Found");
            }

            if (updatedSnapshot.Name != null && updatedSnapshot.Name != snapshot.Name)
            {
                snapshot.Name = updatedSnapshot.Name;
            }

            if (updatedSnapshot.FolderId != null && updatedSnapshot.FolderId != snapshot.FolderId)
            {
                snapshot.FolderId = updatedSnapshot.FolderId;
            }

            if (updatedSnapshot.IsDeleted != snapshot.IsDeleted)
            {
                snapshot.IsDeleted = updatedSnapshot.IsDeleted;
            }

            if (updatedSnapshot.LastUpdatedBy != null && updatedSnapshot.LastUpdatedBy != snapshot.LastUpdatedBy)
            {
                snapshot.LastUpdatedBy = updatedSnapshot.LastUpdatedBy;
            }

            if (updatedSnapshot.LastUpdatedOn != null && updatedSnapshot.LastUpdatedOn != snapshot.LastUpdatedOn)
            {
                snapshot.LastUpdatedOn = updatedSnapshot.LastUpdatedOn;
            }

            if (updatedSnapshot.DeletedOn != null && updatedSnapshot.DeletedOn != snapshot.DeletedOn)
            {
                snapshot.DeletedOn = updatedSnapshot.DeletedOn;
            }

            if (updatedSnapshot.DeletedBy != null && updatedSnapshot.DeletedBy != snapshot.DeletedBy)
            {
                snapshot.DeletedBy = updatedSnapshot.DeletedBy;
            }

            if (updatedSnapshot.Episode != null && updatedSnapshot.Episode != snapshot.Episode)
            {
                snapshot.Episode = updatedSnapshot.Episode;
            }

            if (updatedSnapshot.Scene != null && updatedSnapshot.Scene != snapshot.Scene)
            {
                snapshot.Scene = updatedSnapshot.Scene;
            }

            if (updatedSnapshot.StoryDay != null && updatedSnapshot.StoryDay != snapshot.StoryDay)
            {
                snapshot.StoryDay = updatedSnapshot.StoryDay;
            }

            if (updatedSnapshot.Character != null && updatedSnapshot.Character != snapshot.Character)
            {
                snapshot.Character = updatedSnapshot.Character;
            }

            if (updatedSnapshot.Notes != null && updatedSnapshot.Notes != snapshot.Notes)
            {
                snapshot.Notes = updatedSnapshot.Notes;
            }

            if (updatedSnapshot.Skin != null && updatedSnapshot.Skin != snapshot.Skin)
            {
                snapshot.Skin = updatedSnapshot.Skin;
            }

            if (updatedSnapshot.Brows != null && updatedSnapshot.Brows != snapshot.Brows)
            {
                snapshot.Brows = updatedSnapshot.Brows;
            }

            if (updatedSnapshot.Eyes != null && updatedSnapshot.Eyes != snapshot.Eyes)
            {
                snapshot.Eyes = updatedSnapshot.Eyes;
            }

            if (updatedSnapshot.Lips != null && updatedSnapshot.Lips != snapshot.Lips)
            {
                snapshot.Lips = updatedSnapshot.Lips;
            }

            if (updatedSnapshot.Effects != null && updatedSnapshot.Effects != snapshot.Effects)
            {
                snapshot.Effects = updatedSnapshot.Effects;
            }

            if (updatedSnapshot.MakeupNotes != null && updatedSnapshot.MakeupNotes != snapshot.MakeupNotes)
            {
                snapshot.MakeupNotes = updatedSnapshot.MakeupNotes;
            }

            if (updatedSnapshot.Prep != null && updatedSnapshot.Prep != snapshot.Prep)
            {
                snapshot.Prep = updatedSnapshot.Prep;
            }

            if (updatedSnapshot.Method != null && updatedSnapshot.Method != snapshot.Method)
            {
                snapshot.Method = updatedSnapshot.Method;
            }

            if (updatedSnapshot.StylingTools != null && updatedSnapshot.StylingTools != snapshot.StylingTools)
            {
                snapshot.StylingTools = updatedSnapshot.StylingTools;
            }

            if (updatedSnapshot.Products != null && updatedSnapshot.Products != snapshot.Products)
            {
                snapshot.Products = updatedSnapshot.Products;
            }

            if (updatedSnapshot.HairNotes != null && updatedSnapshot.HairNotes != snapshot.HairNotes)
            {
                snapshot.HairNotes = updatedSnapshot.HairNotes;
            }

            await _ecDbContext.SaveChangesAsync();

            return Response<Snapshot>.Success(snapshot);
        }
    }
}