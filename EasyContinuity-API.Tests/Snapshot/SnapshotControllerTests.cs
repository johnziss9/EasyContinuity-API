using EasyContinuity_API.Controllers;
using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
using EasyContinuity_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Tests.Snapshot;

public class SnapshotControllerTests
{
    private ECDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ECDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ECDbContext(options);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedSnapshot()
    {
        // Arrange

        // Creates a new in-memory database with a unique name "CreateControllerTest"
        using var context = CreateContext("CreateControllerTest");
        var service = new SnapshotService(context);
        var controller = new SnapshotController(service);
        var snapshot = new Models.Snapshot 
        { 
            Name = "Test Snapshot",
            Character = 3,
            Episode = "5",
            Scene = 23
        };

        // Act
        var result = await controller.Create(snapshot);

        // Assert

        // Verify we got an OkObjectResult (HTTP 200 OK)
        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        // Verify the returned object is a Snapshot
        var returnValue = Assert.IsType<Models.Snapshot>(actionResult.Value);
        Assert.Equal(snapshot.Name, returnValue.Name);
        
        // Verify in database
        var savedSnapshot = await context.Snapshots.FindAsync(returnValue.Id);
        Assert.NotNull(savedSnapshot);
        Assert.Equal(snapshot.Name, savedSnapshot.Name);
    }

    [Fact]
    public async Task GetAllBySpace_ShouldReturnAllSnapshotsBySpaceId()
    {
        // Arrange
        var dbName = "GetAllBySpaceControllerTest";
        var spaceId = 1;

        using (var context = CreateContext(dbName))
        {
            context.Snapshots.AddRange(
                new Models.Snapshot { Name = "Snapshot 1", SpaceId = spaceId, Character = 4 },
                new Models.Snapshot { Name = "Snapshot 2", SpaceId = spaceId, Character = 1 },
                new Models.Snapshot { Name = "Snapshot 3", SpaceId = 4, Character = 1 }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SnapshotService(context);
            var controller = new SnapshotController(service);

            // Act
            var result = await controller.GetAllBySpace(spaceId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<Models.Snapshot>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Contains(returnValue, s => s.Name == "Snapshot 1");
            Assert.Contains(returnValue, s => s.Name == "Snapshot 2");
            Assert.DoesNotContain(returnValue, s => s.Name == "Snapshot 3");
        }
    }

    [Fact]
    public async Task GetAllByFolder_ShouldReturnAllSnapshotsByFolderId()
    {
        // Arrange
        var dbName = "GetAllByFolderControllerTest";
        var folderId = 1;

        using (var context = CreateContext(dbName))
        {
            context.Snapshots.AddRange(
                new Models.Snapshot { Name = "Snapshot 1", FolderId = folderId, Character = 3 },
                new Models.Snapshot { Name = "Snapshot 2", FolderId = folderId, Character = 2 },
                new Models.Snapshot { Name = "Snapshot 3", FolderId = 5, Character = 2 }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SnapshotService(context);
            var controller = new SnapshotController(service);

            // Act
            var result = await controller.GetAllByFolder(folderId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<Models.Snapshot>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Contains(returnValue, s => s.Name == "Snapshot 1");
            Assert.Contains(returnValue, s => s.Name == "Snapshot 2");
            Assert.DoesNotContain(returnValue, s => s.Name == "Snapshot 3");
        }
    }

    [Fact]
    public async Task GetAllRootBySpace_ShouldReturnAllRootSnapshots()
    {
        // Arrange
        var dbName = "GetAllRootBySpaceControllerTest";
        var spaceId = 1;

        using (var context = CreateContext(dbName))
        {
            context.Snapshots.AddRange(
                new Models.Snapshot { Name = "Root Snapshot 1", SpaceId = spaceId, FolderId = null },
                new Models.Snapshot { Name = "Root Snapshot 2", SpaceId = spaceId, FolderId = null },
                new Models.Snapshot { Name = "Folder Snapshot", SpaceId = spaceId, FolderId = 1 }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SnapshotService(context);
            var controller = new SnapshotController(service);

            // Act
            var result = await controller.GetAllRootBySpace(spaceId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<Models.Snapshot>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Contains(returnValue, s => s.Name == "Root Snapshot 1");
            Assert.Contains(returnValue, s => s.Name == "Root Snapshot 2");
            Assert.DoesNotContain(returnValue, s => s.Name == "Folder Snapshot");
        }
    }

    [Fact]
    public async Task GetSingle_WithValidId_ShouldReturnSnapshot()
    {
        // Arrange
        var dbName = "GetSingleControllerTest";
        int snapshotId;

        using (var context = CreateContext(dbName))
        {
            var snapshot = new Models.Snapshot 
            { 
                Name = "Test Snapshot",
                Character = 6 
            };
            context.Snapshots.Add(snapshot);
            await context.SaveChangesAsync();
            snapshotId = snapshot.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SnapshotService(context);
            var controller = new SnapshotController(service);

            // Act
            var result = await controller.GetSingle(snapshotId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Models.Snapshot>(actionResult.Value);
            Assert.Equal("Test Snapshot", returnValue.Name);
        }
    }

    [Fact]
    public async Task GetSingle_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        using var context = CreateContext("GetSingleInvalidControllerTest");
        var service = new SnapshotService(context);
        var controller = new SnapshotController(service);

        // Act
        var result = await controller.GetSingle(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_WithValidId_ShouldReturnUpdatedSnapshot()
    {
        // Arrange
        var dbName = "UpdateSnapshotValidControllerTest";
        int snapshotId;

        using (var context = CreateContext(dbName))
        {
            var snapshot = new Models.Snapshot 
            { 
                Name = "Original Name",
                Character = 4,
                Episode = "10"
            };
            context.Snapshots.Add(snapshot);
            await context.SaveChangesAsync();
            snapshotId = snapshot.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SnapshotService(context);
            var controller = new SnapshotController(service);
            var updatedSnapshot = new SnapshotUpdateDTO 
            { 
                Name = "Updated Name",
                Character = 2,
                Episode = "11"
            };

            // Act
            var result = await controller.Update(snapshotId, updatedSnapshot);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Models.Snapshot>(actionResult.Value);
            Assert.Equal(updatedSnapshot.Name, returnValue.Name);
            
            // Verify in database
            var savedSnapshot = await context.Snapshots.FindAsync(snapshotId);
            Assert.NotNull(savedSnapshot);
            Assert.Equal(updatedSnapshot.Name, savedSnapshot.Name);
            Assert.Equal(updatedSnapshot.Character, savedSnapshot.Character);
            Assert.Equal(updatedSnapshot.Episode, savedSnapshot.Episode);
        }
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        using var context = CreateContext("UpdateSnapshotInvalidControllerTest");
        var service = new SnapshotService(context);
        var controller = new SnapshotController(service);
        var snapshot = new SnapshotUpdateDTO 
        { 
            Name = "Test Snapshot",
            Character = 3 
        };

        // Act
        var result = await controller.Update(999, snapshot);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}