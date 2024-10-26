using EasyContinuity_API.Data;
using EasyContinuity_API.Services;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Tests.Space;

public class SpaceServiceTests
{
    private ECDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ECDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ECDbContext(options);
    }

    [Fact]
    public async Task CreateSpace_ShouldAddSpaceAndReturnSuccess()
    {
        // Arrange
        using var context = CreateContext("CreateSpaceTest");
        var service = new SpaceService(context);
        var space = new Models.Space 
        { 
            Name = "Test Space",
            Description = "Test Description" 
        };

        // Act
        var result = await service.CreateSpace(space);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        
        var returnedSpace = result.Data!;
        Assert.NotEqual(0, returnedSpace.Id);
        Assert.Equal(space.Name, returnedSpace.Name);
        
        var savedSpace = await context.Spaces.FindAsync(returnedSpace.Id);
        Assert.NotNull(savedSpace);
        Assert.Equal(space.Name, savedSpace!.Name);
    }

    [Fact]
    public async Task GetAllSpaces_ShouldReturnAllSpaces()
    {
        // Arrange
        var dbName = "GetAllSpacesTest";
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

            // Act
            var result = await service.GetAllSpaces();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, s => s.Name == "Space 1");
            Assert.Contains(result.Data, s => s.Name == "Space 2");
        }
    }

    [Fact]
    public async Task UpdateSpace_WithValidId_ShouldUpdateAndReturnSpace()
    {
        // Arrange
        var dbName = "UpdateSpaceValidTest";
        int spaceId;

        using (var context = CreateContext(dbName))
        {
            var space = new Models.Space 
            { 
                Name = "Original Name",
                Description = "Original Description",
                IsDeleted = false
            };
            context.Spaces.Add(space);
            await context.SaveChangesAsync();
            spaceId = space.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SpaceService(context);
            var updatedSpace = new Models.Space
            {
                Name = "Updated Name",
                Description = "Updated Description",
                IsDeleted = true
            };

            // Act
            var result = await service.UpdateSpace(spaceId, updatedSpace);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            
            Assert.Equal(updatedSpace.Name, result.Data!.Name);
            Assert.Equal(updatedSpace.Description, result.Data.Description);
            Assert.True(result.Data.IsDeleted);

            var savedSpace = await context.Spaces.FindAsync(spaceId);
            Assert.NotNull(savedSpace); 

            Assert.Equal(updatedSpace.Name, savedSpace!.Name);
            Assert.Equal(updatedSpace.Description, savedSpace.Description);
            Assert.True(savedSpace.IsDeleted);
        }
    }

    [Fact]
    public async Task UpdateSpace_WithInvalidId_ShouldReturnFailResponse()
    {
        // Arrange
        using var context = CreateContext("UpdateSpaceInvalidTest");
        var service = new SpaceService(context);
        var updatedSpace = new Models.Space 
        { 
            Name = "Updated Name",
            Description = "Updated Description" 
        };

        // Act
        var result = await service.UpdateSpace(999, updatedSpace);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Space Not Found", result.Message);
    }

    [Fact]
    public async Task UpdateSpace_WithNoChanges_ShouldNotModifyDatabase()
    {
        // Arrange
        var dbName = "UpdateSpaceNoChangesTest";
        int spaceId;
        DateTime originalUpdateTime;

        using (var context = CreateContext(dbName))
        {
            var space = new Models.Space 
            { 
                Name = "Original Name",
                Description = "Original Description",
                LastUpdatedOn = DateTime.UtcNow
            };
            context.Spaces.Add(space);
            await context.SaveChangesAsync();
            spaceId = space.Id;
            originalUpdateTime = space.LastUpdatedOn.Value;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SpaceService(context);
            var updatedSpace = new Models.Space
            {
                Name = "Original Name",
                Description = "Original Description"
            };

            // Act
            var result = await service.UpdateSpace(spaceId, updatedSpace);

            // Assert
            Assert.True(result.IsSuccess);
            var savedSpace = await context.Spaces.FindAsync(spaceId);
            Assert.NotNull(savedSpace);
            Assert.NotNull(savedSpace!.LastUpdatedOn);
            Assert.Equal(originalUpdateTime, savedSpace.LastUpdatedOn!.Value);
        }
    }
}