using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
using EasyContinuity_API.Services;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Tests.Spaces;

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
                new Models.Space { Name = "Space 1", Description = "Description 1", IsDeleted = false },
                new Models.Space { Name = "Space 2", Description = "Description 2", IsDeleted = false },
                new Models.Space { Name = "Space 3", Description = "Description 3", IsDeleted = true }
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
    public async Task GetSingleSpaceById_WithValidId_ShouldReturnSpace()
    {
        // Arrange
        var dbName = "GetSingleSpaceValidTest";
        int spaceId;
        var dateAdded = DateTime.UtcNow;

        using (var context = CreateContext(dbName))
        {
            var space = new Models.Space 
            { 
                Name = "Test Space",
                CreatedOn = dateAdded,
                CreatedBy = 2
            };
            context.Spaces.Add(space);
            await context.SaveChangesAsync();
            spaceId = space.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SpaceService(context);

            // Act
            var result = await service.GetSingleSpaceById(spaceId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("Test Space", result.Data.Name);
            Assert.Equal(dateAdded, result.Data.CreatedOn);
            Assert.Equal(2, result.Data.CreatedBy);
        }
    }

    [Fact]
    public async Task GetSingleSpaceById_WithInvalidId_ShouldReturnFailResponse()
    {
        // Arrange
        using var context = CreateContext("GetSingleSpaceInvalidTest");
        var service = new SpaceService(context);

        // Act
        var result = await service.GetSingleSpaceById(999);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Space Not Found", result.Message);
    }

    [Fact]
    public async Task GetSingleSpaceById_WithDeletedTrue_ShouldReturnNotFound()
    {
        // Arrange
        var dbName = "GetSingleSpaceDeletedTrueTest";
        int spaceId;
        var dateAdded = DateTime.UtcNow;

        using (var context = CreateContext(dbName))
        {
            var space = new Models.Space 
            { 
                Name = "Test Space",
                CreatedOn = dateAdded,
                IsDeleted = true,
                CreatedBy = 2
            };
            context.Spaces.Add(space);
            await context.SaveChangesAsync();
            spaceId = space.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new SpaceService(context);

            // Act
            var result = await service.GetSingleSpaceById(spaceId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
            Assert.Equal("Space Not Found", result.Message);
        }
    }

    [Fact]
    public async Task SearchContentsBySpace_WithValidQuery_ShouldReturnResults()
    {
        // Arrange
        var dbName = "SearchContentsServiceTest";
        using var context = CreateContext(dbName);
        var service = new SpaceService(context);

        var space = new Models.Space { Id = 1, Name = "Test Space" };
        var folder = new Models.Folder 
        { 
            Name = "Test Folder",
            SpaceId = 1,
            IsDeleted = false
        };
        var snapshot = new Models.Snapshot 
        { 
            Name = "Test Snapshot",
            SpaceId = 1,
            IsDeleted = false
        };
        
        context.Spaces.Add(space);
        context.Folders.Add(folder);
        context.Snapshots.Add(snapshot);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchContentsBySpace(1, "Test");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public async Task SearchContentsBySpace_ShouldBeCaseInsensitive()
    {
        // Arrange
        var dbName = "SearchContentsCaseTest";
        using var context = CreateContext(dbName);
        var service = new SpaceService(context);

        var space = new Models.Space { Id = 1, Name = "Test Space" };
        var folder = new Models.Folder 
        { 
            Name = "TEST FOLDER",
            SpaceId = 1,
            IsDeleted = false
        };
        var snapshot = new Models.Snapshot 
        { 
            Name = "test snapshot",
            SpaceId = 1,
            IsDeleted = false
        };
        
        context.Spaces.Add(space);
        context.Folders.Add(folder);
        context.Snapshots.Add(snapshot);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchContentsBySpace(1, "test");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(2, result.Data.Count);
    }

    [Fact]
    public async Task SearchContentsBySpace_WithNonExistentSpace_ShouldReturnEmptyList()
    {
        // Arrange
        using var context = CreateContext("SearchContentsNonExistentSpaceTest");
        var service = new SpaceService(context);

        // Act
        var result = await service.SearchContentsBySpace(999, "test");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task SearchContentsBySpace_WithDifferentSpace_ShouldNotReturnItems()
    {
        // Arrange
        var dbName = "SearchContentsDifferentSpaceTest";
        using var context = CreateContext(dbName);
        var service = new SpaceService(context);

        var space1 = new Models.Space { Id = 1, Name = "Space 1" };
        var space2 = new Models.Space { Id = 2, Name = "Space 2" };
        
        var folderInSpace2 = new Models.Folder 
        { 
            Name = "Test Folder",
            SpaceId = space2.Id,
            IsDeleted = false
        };
        
        context.Spaces.AddRange(space1, space2);
        context.Folders.Add(folderInSpace2);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchContentsBySpace(space1.Id, "Test");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task SearchContentsBySpace_WithNoMatches_ShouldReturnEmptyList()
    {
        // Arrange
        var dbName = "SearchContentsNoMatchesServiceTest";
        using var context = CreateContext(dbName);
        var service = new SpaceService(context);

        var space = new Models.Space { Id = 1, Name = "Test Space" };
        var folder = new Models.Folder 
        { 
            Name = "Sample Folder",
            SpaceId = 1,
            IsDeleted = false
        };
        var snapshot = new Models.Snapshot 
        { 
            Name = "Example Snapshot",
            SpaceId = 1,
            IsDeleted = false
        };
        
        context.Spaces.Add(space);
        context.Folders.Add(folder);
        context.Snapshots.Add(snapshot);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchContentsBySpace(1, "NonExistent");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
    }

    [Fact]
    public async Task SearchContentsBySpace_ShouldNotReturnDeletedItems()
    {
        // Arrange
        var dbName = "SearchContentsDeletedItemsServiceTest";
        using var context = CreateContext(dbName);
        var service = new SpaceService(context);

        var space = new Models.Space { Id = 1, Name = "Test Space" };
        var folder = new Models.Folder 
        { 
            Name = "Test Folder",
            SpaceId = 1,
            IsDeleted = true
        };
        var snapshot = new Models.Snapshot 
        { 
            Name = "Test Snapshot",
            SpaceId = 1,
            IsDeleted = true
        };
        var activeFolder = new Models.Folder 
        { 
            Name = "Different Name",
            SpaceId = 1,
            IsDeleted = false
        };
        
        context.Spaces.Add(space);
        context.Folders.AddRange(folder, activeFolder);
        context.Snapshots.Add(snapshot);
        await context.SaveChangesAsync();

        // Act
        var result = await service.SearchContentsBySpace(1, "Test");

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Empty(result.Data);
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
            var updatedSpace = new SpaceUpdateDTO
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
        var updatedSpace = new SpaceUpdateDTO 
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
            var updatedSpace = new SpaceUpdateDTO
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