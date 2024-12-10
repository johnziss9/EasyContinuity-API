using EasyContinuity_API.Controllers;
using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
using EasyContinuity_API.Models;
using EasyContinuity_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Tests.Spaces;

public class SpaceControllerTests
{
    private ECDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ECDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ECDbContext(options);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedSpace()
    {
        // Arrange

        // Creates a new in-memory database with a unique name "CreateControllerTest"
        using var context = CreateContext("CreateControllerTest");
        var service = new SpaceService(context);
        var controller = new SpaceController(service);
        var space = new Models.Space 
        { 
            Name = "Test Space Name",
            Description = "Test Space Description" 
        };

        // Act
        var result = await controller.Create(space);

        // Assert

        // Verify we got an OkObjectResult (HTTP 200 OK)
        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        // Verify the returned object is a Space
        var returnValue = Assert.IsType<Models.Space>(actionResult.Value);
        Assert.Equal(space.Name, returnValue.Name);
        
        // Verify in database
        var savedSpace = await context.Spaces.FindAsync(returnValue.Id);
        Assert.NotNull(savedSpace);
        Assert.Equal(space.Name, savedSpace.Name);
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllSpaces()
    {
        // Arrange
        var dbName = "GetAllControllerTest";
        using (var context = CreateContext(dbName))
        {
            context.Spaces.AddRange(
                new Space { Name = "Space 1", Description = "Description 1", IsDeleted = false },
                new Space { Name = "Space 2", Description = "Description 2", IsDeleted = false },
                new Space { Name = "Space 3", Description = "Description 3", IsDeleted = true }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SpaceService(context);
            var controller = new SpaceController(service);

            // Act
            var result = await controller.GetAll();

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<Models.Space>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Contains(returnValue, s => s.Name == "Space 1");
            Assert.Contains(returnValue, s => s.Name == "Space 2");
        }
    }

    [Fact]
    public async Task GetSingle_WithValidId_ShouldReturnSpace()
    {
        // Arrange
        var dbName = "GetSingleControllerTest";
        int spaceId;

        using (var context = CreateContext(dbName))
        {
            var space = new Space 
            { 
                Name = "Test Space"
            };
            context.Spaces.Add(space);
            await context.SaveChangesAsync();
            spaceId = space.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SpaceService(context);
            var controller = new SpaceController(service);

            // Act
            var result = await controller.GetSingle(spaceId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Space>(actionResult.Value);
            Assert.Equal("Test Space", returnValue.Name);
        }
    }

    [Fact]
    public async Task GetSingle_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        using var context = CreateContext("GetSingleInvalidControllerTest");
        var service = new SpaceService(context);
        var controller = new SpaceController(service);

        // Act
        var result = await controller.GetSingle(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetSingle_WithDeletedTrue_ShouldReturnNotFound()
    {
        // Arrange
        var dbName = "GetSingleDeletedTrueControllerTest";
        int spaceId;

        using (var context = CreateContext(dbName))
        {
            var space = new Space 
            { 
                Name = "Test Space",
                IsDeleted = true
            };
            context.Spaces.Add(space);
            await context.SaveChangesAsync();
            spaceId = space.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SpaceService(context);
            var controller = new SpaceController(service);

            // Act
            var result = await controller.GetSingle(spaceId);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }
    }

    [Fact]
    public async Task SearchContents_WithValidQuery_ShouldReturnResults()
    {
        // Arrange
        var dbName = "SearchContentsControllerTest";
        int spaceId;

        using (var context = CreateContext(dbName))
        {
            var space = new Space { Id = 1, Name = "Test Space" };
            var folder = new Folder 
            { 
                Name = "Test Folder",
                SpaceId = 1,
                IsDeleted = false
            };
            var snapshot = new Snapshot 
            { 
                Name = "Test Snapshot",
                SpaceId = 1,
                IsDeleted = false
            };
            
            context.Spaces.Add(space);
            context.Folders.Add(folder);
            context.Snapshots.Add(snapshot);
            await context.SaveChangesAsync();
            spaceId = space.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SpaceService(context);
            var controller = new SpaceController(service);

            // Act
            var result = await controller.SearchContents(spaceId, "Test");

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<object>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
        }
    }

    [Fact]
    public async Task SearchContents_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var dbName = "SearchContentsNoMatchesTest";
        int spaceId;

        using (var context = CreateContext(dbName))
        {
            var space = new Space { Id = 1, Name = "Test Space" };
            context.Spaces.Add(space);
            await context.SaveChangesAsync();
            spaceId = space.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SpaceService(context);
            var controller = new SpaceController(service);

            // Act
            var result = await controller.SearchContents(spaceId, "NonExistent");

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<object>>(actionResult.Value);
            Assert.Empty(returnValue);
        }
    }

    [Fact]
    public async Task SearchContents_ShouldNotReturnDeletedItems()
    {
        // Arrange
        var dbName = "SearchContentsDeletedItemsTest";
        int spaceId;

        using (var context = CreateContext(dbName))
        {
            var space = new Space { Id = 1, Name = "Test Space" };
            var folder = new Folder 
            { 
                Name = "Test Folder",
                SpaceId = 1,
                IsDeleted = true
            };
            var snapshot = new Snapshot 
            { 
                Name = "Test Snapshot",
                SpaceId = 1,
                IsDeleted = true
            };
            
            context.Spaces.Add(space);
            context.Folders.Add(folder);
            context.Snapshots.Add(snapshot);
            await context.SaveChangesAsync();
            spaceId = space.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SpaceService(context);
            var controller = new SpaceController(service);

            // Act
            var result = await controller.SearchContents(spaceId, "Test");

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<object>>(actionResult.Value);
            Assert.Empty(returnValue);
        }
    }

    [Fact]
    public async Task SearchContents_ShouldBeCaseInsensitive()
    {
        // Arrange
        var dbName = "SearchContentsCaseControllerTest";
        int spaceId;

        using (var context = CreateContext(dbName))
        {
            var space = new Space { Id = 1, Name = "Test Space" };
            var folder = new Folder 
            { 
                Name = "TEST FOLDER",
                SpaceId = 1,
                IsDeleted = false
            };
            var snapshot = new Snapshot 
            { 
                Name = "test snapshot",
                SpaceId = 1,
                IsDeleted = false
            };
            
            context.Spaces.Add(space);
            context.Folders.Add(folder);
            context.Snapshots.Add(snapshot);
            await context.SaveChangesAsync();
            spaceId = space.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SpaceService(context);
            var controller = new SpaceController(service);

            // Act
            var result = await controller.SearchContents(spaceId, "test");

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<object>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
        }
    }

    [Fact]
    public async Task SearchContents_WithNonExistentSpace_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = CreateContext("SearchContentsNonExistentSpaceControllerTest");
        var service = new SpaceService(context);
        var controller = new SpaceController(service);

        // Act
        var result = await controller.SearchContents(999, "test");

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnValue = Assert.IsType<List<object>>(actionResult.Value);
        Assert.Empty(returnValue);
    }

    [Fact]
    public async Task SearchContents_WithDifferentSpace_ShouldNotReturnItems()
    {
        // Arrange
        var dbName = "SearchContentsDifferentSpaceControllerTest";
        int space1Id;

        using (var context = CreateContext(dbName))
        {
            var space1 = new Space { Id = 1, Name = "Space 1" };
            var space2 = new Space { Id = 2, Name = "Space 2" };
            
            var folderInSpace2 = new Folder 
            { 
                Name = "Test Folder",
                SpaceId = space2.Id,
                IsDeleted = false
            };
            
            context.Spaces.AddRange(space1, space2);
            context.Folders.Add(folderInSpace2);
            await context.SaveChangesAsync();
            space1Id = space1.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SpaceService(context);
            var controller = new SpaceController(service);

            // Act
            var result = await controller.SearchContents(space1Id, "Test");

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<object>>(actionResult.Value);
            Assert.Empty(returnValue);
        }
    }

    [Fact]
    public async Task Update_WithValidId_ShouldReturnUpdatedSpace()
    {
        // Arrange
        var dbName = "UpdateControllerValidTest";
        int spaceId;

        using (var context = CreateContext(dbName))
        {
            var space = new Models.Space 
            { 
                Name = "Original Name",
                Description = "Original Description" 
            };
            context.Spaces.Add(space);
            await context.SaveChangesAsync();
            spaceId = space.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SpaceService(context);
            var controller = new SpaceController(service);
            var updatedSpace = new SpaceUpdateDTO 
            { 
                Name = "Updated Name",
                Description = "Updated Description" 
            };

            // Act
            var result = await controller.Update(spaceId, updatedSpace);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Models.Space>(actionResult.Value);
            Assert.Equal(updatedSpace.Name, returnValue.Name);
            
            // Verify in database
            var savedSpace = await context.Spaces.FindAsync(spaceId);
            Assert.NotNull(savedSpace);
            Assert.Equal(updatedSpace.Name, savedSpace!.Name);
        }
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        using var context = CreateContext("UpdateControllerInvalidTest");
        var service = new SpaceService(context);
        var controller = new SpaceController(service);
        var space = new SpaceUpdateDTO 
        { 
            Name = "Test Space",
            Description = "Test Description" 
        };

        // Act
        var result = await controller.Update(999, space);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}