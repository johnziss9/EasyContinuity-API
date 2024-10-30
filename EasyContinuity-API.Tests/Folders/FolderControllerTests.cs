using EasyContinuity_API.Controllers;
using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
using EasyContinuity_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Tests.Folders;

public class FolderControllerTests
{
    private ECDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ECDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ECDbContext(options);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedFolder()
    {
        // Arrange

        // Creates a new in-memory database with a unique name "CreateControllerTest"
        using var context = CreateContext("CreateControllerTest");
        var service = new FolderService(context);
        var controller = new FolderController(service);
        var folder = new Models.Folder 
        { 
            Name = "Test Folder Name",
            Description = "Test Folder Description" 
        };

        // Act
        var result = await controller.Create(folder);

        // Assert

        // Verify we got an OkObjectResult (HTTP 200 OK)
        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        // Verify the returned object is a Folder
        var returnValue = Assert.IsType<Models.Folder>(actionResult.Value);
        Assert.Equal(folder.Name, returnValue.Name);
        
        // Verify in database
        var savedFolder = await context.Folders.FindAsync(returnValue.Id);
        Assert.NotNull(savedFolder);
        Assert.Equal(folder.Name, savedFolder.Name);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllFoldersBySpaceId()
    {
        // Arrange
        var dbName = "GetAllControllerTest";
        var spaceId = 1;

        using (var context = CreateContext(dbName))
        {
            context.Folders.AddRange(
                new Models.Folder { Name = "Folder 1", Description = "Description 1", SpaceId = spaceId },
                new Models.Folder { Name = "Folder 2", Description = "Description 2", SpaceId = spaceId },
                new Models.Folder { Name = "Folder 3", Description = "Description 3", SpaceId = 5 }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new FolderService(context);
            var controller = new FolderController(service);

            // Act
            var result = await controller.GetAllBySpace(spaceId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<Models.Folder>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Contains(returnValue, s => s.Name == "Folder 1");
            Assert.Contains(returnValue, s => s.Name == "Folder 2");
            Assert.DoesNotContain(returnValue, s => s.Name == "Folder 3");
        }
    }

    [Fact]
    public async Task Update_WithValidId_ShouldReturnUpdatedFolder()
    {
        // Arrange
        var dbName = "UpdateControllerValidTest";
        int folderId;

        using (var context = CreateContext(dbName))
        {
            var folder = new Models.Folder 
            { 
                Name = "Original Name",
                Description = "Original Description" 
            };
            context.Folders.Add(folder);
            await context.SaveChangesAsync();
            folderId = folder.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new FolderService(context);
            var controller = new FolderController(service);
            var updatedFolder = new FolderUpdateDTO 
            { 
                Name = "Updated Name",
                Description = "Updated Description" 
            };

            // Act
            var result = await controller.Update(folderId, updatedFolder);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Models.Folder>(actionResult.Value);
            Assert.Equal(updatedFolder.Name, returnValue.Name);
            
            // Verify in database
            var savedFolder = await context.Folders.FindAsync(folderId);
            Assert.NotNull(savedFolder);
            Assert.Equal(updatedFolder.Name, updatedFolder!.Name);
        }
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        using var context = CreateContext("UpdateControllerInvalidTest");
        var service = new FolderService(context);
        var controller = new FolderController(service);
        var folder = new FolderUpdateDTO 
        { 
            Name = "Test Folder",
            Description = "Test Description" 
        };

        // Act
        var result = await controller.Update(999, folder);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}