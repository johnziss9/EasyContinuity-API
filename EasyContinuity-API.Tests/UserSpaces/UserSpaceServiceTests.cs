using EasyContinuity_API.Data;
using EasyContinuity_API.Models;
using EasyContinuity_API.Services;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Tests.UserSpaces;

public class UserSpaceServiceTests
{
    private ECDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ECDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ECDbContext(options);
    }

    [Fact]
    public async Task CreateUserSpace_ShouldAddUserSpaceAndReturnSuccess()
    {
        // Arrange
        using var context = CreateContext("CreateUserSpaceTest");
        var service = new UserSpaceService(context);
        var userSpace = new UserSpace 
        { 
            UserId = 1,
            SpaceId = 2,
            Role = SpaceRole.Editor
        };

        // Act
        var result = await service.CreateUserSpace(userSpace);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        
        var returnedUserSpace = result.Data!;
        Assert.Equal(userSpace.UserId, returnedUserSpace.UserId);
        Assert.Equal(userSpace.SpaceId, returnedUserSpace.SpaceId);
        Assert.Equal(userSpace.Role, returnedUserSpace.Role);
        
        var savedUserSpace = await context.UserSpaces.FirstOrDefaultAsync(us => 
            us.UserId == returnedUserSpace.UserId && us.SpaceId == returnedUserSpace.SpaceId);
        Assert.NotNull(savedUserSpace);
        Assert.Equal(userSpace.UserId, savedUserSpace!.UserId);
        Assert.Equal(userSpace.SpaceId, savedUserSpace.SpaceId);
        Assert.Equal(userSpace.Role, savedUserSpace.Role);
    }

    [Fact]
    public async Task CreateUserSpace_WithExistingUserAndSpace_ShouldSucceed()
    {
        // Arrange
        var dbName = "CreateUserSpaceWithExistingEntitiesTest";
        
        // Create a new context for setup
        using (var context = CreateContext(dbName))
        {
            // Add User and Space to the database first
            var user = new User { Id = 1, Email = "user@test.com" };
            var space = new Space { Id = 2, Name = "Test Space" };
            context.Users.Add(user);
            context.Spaces.Add(space);
            await context.SaveChangesAsync();
        }
        
        // Create a new context for the test
        using (var context = CreateContext(dbName))
        {
            var service = new UserSpaceService(context);
            var userSpace = new UserSpace 
            { 
                UserId = 1,
                SpaceId = 2,
                Role = SpaceRole.Editor
            };

            // Act
            var result = await service.CreateUserSpace(userSpace);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            
            var savedUserSpace = await context.UserSpaces.FirstOrDefaultAsync(us => 
                us.UserId == 1 && us.SpaceId == 2);
            Assert.NotNull(savedUserSpace);
            Assert.Equal(SpaceRole.Editor, savedUserSpace!.Role);
        }
    }
}