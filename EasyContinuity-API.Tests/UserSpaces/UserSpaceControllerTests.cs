using EasyContinuity_API.Controllers;
using EasyContinuity_API.Data;
using EasyContinuity_API.Models;
using EasyContinuity_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Tests.UserSpaces;

public class UserSpaceControllerTests
{
    private ECDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ECDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ECDbContext(options);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedUserSpace()
    {
        // Arrange
        using var context = CreateContext("CreateUserSpaceControllerTest");
        var service = new UserSpaceService(context);
        var controller = new UserSpaceController(service);
        var userSpace = new UserSpace 
        { 
            UserId = 1,
            SpaceId = 2,
            Role = SpaceRole.Editor
        };

        // Act
        var result = await controller.Create(userSpace);

        // Assert
        // Verify we got an OkObjectResult (HTTP 200 OK)
        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        // Verify the returned object is a UserSpace
        var returnValue = Assert.IsType<UserSpace>(actionResult.Value);
        Assert.Equal(userSpace.UserId, returnValue.UserId);
        Assert.Equal(userSpace.SpaceId, returnValue.SpaceId);
        Assert.Equal(userSpace.Role, returnValue.Role);
        
        // Verify in database
        var savedUserSpace = await context.UserSpaces.FirstOrDefaultAsync(us => 
            us.UserId == returnValue.UserId && us.SpaceId == returnValue.SpaceId);
        Assert.NotNull(savedUserSpace);
        Assert.Equal(userSpace.UserId, savedUserSpace.UserId);
        Assert.Equal(userSpace.SpaceId, savedUserSpace.SpaceId);
        Assert.Equal(userSpace.Role, savedUserSpace.Role);
    }
}