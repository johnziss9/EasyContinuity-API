using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
using EasyContinuity_API.Services;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Tests.Folders;

public class FolderServiceTests
{
    private ECDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ECDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ECDbContext(options);
    }

    [Fact]
    public async Task CreateFolder_ShouldAddFolderAndReturnSuccess()
    {
        // Arrange
        using var context = CreateContext("CreateFolderTest");
        var service = new FolderService(context);
        var folder = new Models.Folder 
        { 
            Name = "Test Folder",
            Description = "Test Description" 
        };

        // Act
        var result = await service.CreateFolder(folder);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        
        var returnedFolder = result.Data!;
        Assert.NotEqual(0, returnedFolder.Id);
        Assert.Equal(folder.Name, returnedFolder.Name);
        
        var savedFolder = await context.Folders.FindAsync(returnedFolder.Id);
        Assert.NotNull(savedFolder);
        Assert.Equal(folder.Name, savedFolder!.Name);
    }

    [Fact]
    public async Task GetAllFoldersBySpaceId_ShouldReturnAllFoldersForASpecificId()
    {
        // Arrange
        var dbName = "GetAllFoldersTest";
        var spaceId = 1;

        using (var context = CreateContext(dbName))
        {
            context.Folders.AddRange(
                new Models.Folder { Name = "Folder 1", Description = "Description 1", SpaceId = spaceId },
                new Models.Folder { Name = "Folder 2", Description = "Description 2", SpaceId = spaceId },
                new Models.Folder { Name = "Folder 3", Description = "Description 3", SpaceId = 3 }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new FolderService(context);

            // Act
            var result = await service.GetAllFoldersBySpaceId(spaceId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, s => s.Name == "Folder 1");
            Assert.Contains(result.Data, s => s.Name == "Folder 2");
            Assert.DoesNotContain(result.Data, s => s.Name == "Folder 3");
        }
    }

    [Fact]
    public async Task GetAllFoldersByParentId_ShouldReturnAllFoldersForASpecificId()
    {
        // Arrange
        var dbName = "GetAllFoldersTest";
        var parentId = 1;

        using (var context = CreateContext(dbName))
        {
            context.Folders.AddRange(
                new Models.Folder { Name = "Folder 1", Description = "Description 1", ParentId = parentId },
                new Models.Folder { Name = "Folder 2", Description = "Description 2", ParentId = parentId },
                new Models.Folder { Name = "Folder 3", Description = "Description 3", ParentId = 3 }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new FolderService(context);

            // Act
            var result = await service.GetAllFoldersByParentId(parentId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, s => s.Name == "Folder 1");
            Assert.Contains(result.Data, s => s.Name == "Folder 2");
            Assert.DoesNotContain(result.Data, s => s.Name == "Folder 3");
        }
    }

    [Fact]
    public async Task UpdateFolder_WithValidId_ShouldUpdateAndReturnFolder()
    {
        // Arrange
        var dbName = "UpdateFolderValidTest";
        int folderId;

        using (var context = CreateContext(dbName))
        {
            var folder = new Models.Folder 
            { 
                Name = "Original Name",
                Description = "Original Description",
                IsDeleted = false
            };
            context.Folders.Add(folder);
            await context.SaveChangesAsync();
            folderId = folder.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new FolderService(context);
            var updatedFolder = new FolderUpdateDTO
            {
                Name = "Updated Name",
                Description = "Updated Description",
                IsDeleted = true
            };

            // Act
            var result = await service.UpdateFolder(folderId, updatedFolder);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            
            Assert.Equal(updatedFolder.Name, result.Data!.Name);
            Assert.Equal(updatedFolder.Description, result.Data.Description);
            Assert.True(result.Data.IsDeleted);

            var savedFolder = await context.Folders.FindAsync(folderId);
            Assert.NotNull(savedFolder); 

            Assert.Equal(updatedFolder.Name, savedFolder!.Name);
            Assert.Equal(updatedFolder.Description, savedFolder.Description);
            Assert.True(savedFolder.IsDeleted);
        }
    }

    [Fact]
    public async Task UpdateFolder_WithInvalidId_ShouldReturnFailResponse()
    {
        // Arrange
        using var context = CreateContext("UpdateFolderInvalidTest");
        var service = new FolderService(context);
        var updatedFolder = new FolderUpdateDTO 
        { 
            Name = "Updated Name",
            Description = "Updated Description" 
        };

        // Act
        var result = await service.UpdateFolder(999, updatedFolder);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Folder Not Found", result.Message);
    }

    [Fact]
    public async Task UpdateFolder_WithNoChanges_ShouldNotModifyDatabase()
    {
        // Arrange
        var dbName = "UpdateFolderNoChangesTest";
        int folderId;
        DateTime originalUpdateTime;

        using (var context = CreateContext(dbName))
        {
            var folder = new Models.Folder 
            { 
                Name = "Original Name",
                Description = "Original Description",
                LastUpdatedOn = DateTime.UtcNow
            };
            context.Folders.Add(folder);
            await context.SaveChangesAsync();
            folderId = folder.Id;
            originalUpdateTime = folder.LastUpdatedOn.Value;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new FolderService(context);
            var updatedFolder = new FolderUpdateDTO
            {
                Name = "Original Name",
                Description = "Original Description"
            };

            // Act
            var result = await service.UpdateFolder(folderId, updatedFolder);

            // Assert
            Assert.True(result.IsSuccess);
            var savedFolder = await context.Folders.FindAsync(folderId);
            Assert.NotNull(savedFolder);
            Assert.NotNull(savedFolder!.LastUpdatedOn);
            Assert.Equal(originalUpdateTime, savedFolder.LastUpdatedOn!.Value);
        }
    }
}