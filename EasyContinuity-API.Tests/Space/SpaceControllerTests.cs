using EasyContinuity_API.Controllers;
using EasyContinuity_API.Data;
using EasyContinuity_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Tests.Space;

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
                new Models.Space { Name = "Space 1", Description = "Description 1" },
                new Models.Space { Name = "Space 2", Description = "Description 2" }
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
            var updatedSpace = new Models.Space 
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
        var space = new Models.Space 
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