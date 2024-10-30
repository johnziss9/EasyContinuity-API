using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
using EasyContinuity_API.Services;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Tests.Snapshots;

public class SnapshotServiceTests
{
    private ECDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ECDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ECDbContext(options);
    }

    [Fact]
    public async Task CreateSnapshot_ShouldAddSnapshotAndReturnSuccess()
    {
        // Arrange
        using var context = CreateContext("CreateSnapshotServiceTest");
        var service = new SnapshotService(context);
        var snapshot = new Models.Snapshot 
        { 
            Name = "Test Snapshot",
            Character = 2,
            Episode = "1",
            Scene = 4
        };

        // Act
        var result = await service.CreateSnapshot(snapshot);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        
        var returnedSnapshot = result.Data!;
        Assert.NotEqual(0, returnedSnapshot.Id);
        Assert.Equal(snapshot.Name, returnedSnapshot.Name);
        
        var savedSnapshot = await context.Snapshots.FindAsync(returnedSnapshot.Id);
        Assert.NotNull(savedSnapshot);
        Assert.Equal(snapshot.Name, savedSnapshot!.Name);
    }

    [Fact]
    public async Task GetAllSnapshotsBySpaceId_ShouldReturnAllSnapshotsForSpace()
    {
        // Arrange
        var dbName = "GetAllSnapshotsSpaceTest";
        var spaceId = 1;

        using (var context = CreateContext(dbName))
        {
            context.Snapshots.AddRange(
                new Models.Snapshot { Name = "Snapshot 1", SpaceId = spaceId, Character = 2 },
                new Models.Snapshot { Name = "Snapshot 2", SpaceId = spaceId, Character = 3 },
                new Models.Snapshot { Name = "Other Space", SpaceId = 2, Character = 6 }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SnapshotService(context);

            // Act
            var result = await service.GetAllSnapshotsBySpaceId(spaceId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, s => s.Name == "Snapshot 1");
            Assert.Contains(result.Data, s => s.Name == "Snapshot 2");
            Assert.DoesNotContain(result.Data, s => s.Name == "Other Space");
        }
    }

    [Fact]
    public async Task GetAllSnapshotsByFolderId_ShouldReturnAllSnapshotsForFolder()
    {
        // Arrange
        var dbName = "GetAllSnapshotsFolderTest";
        var folderId = 1;

        using (var context = CreateContext(dbName))
        {
            context.Snapshots.AddRange(
                new Models.Snapshot { Name = "Snapshot 1", FolderId = folderId, Character = 1 },
                new Models.Snapshot { Name = "Snapshot 2", FolderId = folderId, Character = 4 },
                new Models.Snapshot { Name = "Other Folder", FolderId = 2, Character = 4 }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SnapshotService(context);

            // Act
            var result = await service.GetAllSnapshotsByFolderId(folderId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, s => s.Name == "Snapshot 1");
            Assert.Contains(result.Data, s => s.Name == "Snapshot 2");
            Assert.DoesNotContain(result.Data, s => s.Name == "Other Folder");
        }
    }

    [Fact]
    public async Task GetAllRootSnapshotsBySpaceId_ShouldReturnOnlyRootSnapshots()
    {
        // Arrange
        var dbName = "GetRootSnapshotsTest";
        var spaceId = 1;

        using (var context = CreateContext(dbName))
        {
            context.Snapshots.AddRange(
                new Models.Snapshot { Name = "Root 1", SpaceId = spaceId, FolderId = null },
                new Models.Snapshot { Name = "Root 2", SpaceId = spaceId, FolderId = null },
                new Models.Snapshot { Name = "In Folder", SpaceId = spaceId, FolderId = 1 }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SnapshotService(context);

            // Act
            var result = await service.GetAllRootSnapshotsBySpaceId(spaceId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, s => s.Name == "Root 1");
            Assert.Contains(result.Data, s => s.Name == "Root 2");
            Assert.DoesNotContain(result.Data, s => s.Name == "In Folder");
        }
    }

    [Fact]
    public async Task GetSingleSnapshotById_WithValidId_ShouldReturnSnapshot()
    {
        // Arrange
        var dbName = "GetSingleSnapshotValidTest";
        int snapshotId;

        using (var context = CreateContext(dbName))
        {
            var snapshot = new Models.Snapshot 
            { 
                Name = "Test Snapshot",
                Character = 4,
                Episode = "Test Episode"
            };
            context.Snapshots.Add(snapshot);
            await context.SaveChangesAsync();
            snapshotId = snapshot.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SnapshotService(context);

            // Act
            var result = await service.GetSingleSnapshotById(snapshotId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("Test Snapshot", result.Data.Name);
            Assert.Equal(4, result.Data.Character);
            Assert.Equal("Test Episode", result.Data.Episode);
        }
    }

    [Fact]
    public async Task GetSingleSnapshotById_WithInvalidId_ShouldReturnFailResponse()
    {
        // Arrange
        using var context = CreateContext("GetSingleSnapshotInvalidTest");
        var service = new SnapshotService(context);

        // Act
        var result = await service.GetSingleSnapshotById(999);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Snapshot Not Found", result.Message);
    }

    [Fact]
    public async Task UpdateSnapshot_WithValidId_ShouldUpdateAndReturnSnapshot()
    {
        // Arrange
        var dbName = "UpdateSnapshotValidTest";
        int snapshotId;

        using (var context = CreateContext(dbName))
        {
            var snapshot = new Models.Snapshot 
            { 
                Name = "Original Name",
                Character = 3,
                Episode = "Original Episode",
                Scene = 20,
                IsDeleted = false
            };
            context.Snapshots.Add(snapshot);
            await context.SaveChangesAsync();
            snapshotId = snapshot.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SnapshotService(context);
            var updatedSnapshot = new SnapshotUpdateDTO
            {
                Name = "Updated Name",
                Character = 5,
                Episode = "Updated Episode",
                Scene = 12,
                IsDeleted = true
            };

            // Act
            var result = await service.UpdateSnapshot(snapshotId, updatedSnapshot);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            
            Assert.Equal(updatedSnapshot.Name, result.Data.Name);
            Assert.Equal(updatedSnapshot.Character, result.Data.Character);
            Assert.Equal(updatedSnapshot.Episode, result.Data.Episode);
            Assert.Equal(updatedSnapshot.Scene, result.Data.Scene);
            Assert.True(result.Data.IsDeleted);

            var savedSnapshot = await context.Snapshots.FindAsync(snapshotId);
            Assert.NotNull(savedSnapshot);
            Assert.Equal(updatedSnapshot.Name, savedSnapshot!.Name);
            Assert.Equal(updatedSnapshot.Character, savedSnapshot.Character);
            Assert.Equal(updatedSnapshot.Episode, savedSnapshot.Episode);
            Assert.Equal(updatedSnapshot.Scene, savedSnapshot.Scene);
            Assert.True(savedSnapshot.IsDeleted);
        }
    }

    [Fact]
    public async Task UpdateSnapshot_WithInvalidId_ShouldReturnFailResponse()
    {
        // Arrange
        using var context = CreateContext("UpdateSnapshotInvalidTest");
        var service = new SnapshotService(context);
        var updatedSnapshot = new SnapshotUpdateDTO 
        { 
            Name = "Updated Name",
            Character = 4 
        };

        // Act
        var result = await service.UpdateSnapshot(999, updatedSnapshot);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Snapshot Not Found", result.Message);
    }

    [Fact]
    public async Task UpdateSnapshot_WithNoChanges_ShouldNotModifyDatabase()
    {
        // Arrange
        var dbName = "UpdateSnapshotNoChangesTest";
        int snapshotId;
        DateTime originalUpdateTime;

        using (var context = CreateContext(dbName))
        {
            var snapshot = new Models.Snapshot 
            { 
                Name = "Original Name",
                Character = 4,
                LastUpdatedOn = DateTime.UtcNow
            };
            context.Snapshots.Add(snapshot);
            await context.SaveChangesAsync();
            snapshotId = snapshot.Id;
            originalUpdateTime = snapshot.LastUpdatedOn.Value;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SnapshotService(context);
            var updatedSnapshot = new SnapshotUpdateDTO
            {
                Name = "Original Name",
                Character = 4
            };

            // Act
            var result = await service.UpdateSnapshot(snapshotId, updatedSnapshot);

            // Assert
            Assert.True(result.IsSuccess);
            var savedSnapshot = await context.Snapshots.FindAsync(snapshotId);
            Assert.NotNull(savedSnapshot);
            Assert.Equal(originalUpdateTime, savedSnapshot!.LastUpdatedOn!.Value);
        }
    }
}