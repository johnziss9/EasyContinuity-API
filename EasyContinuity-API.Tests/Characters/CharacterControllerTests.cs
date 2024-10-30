using EasyContinuity_API.Controllers;
using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
using EasyContinuity_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Tests.Folders;

public class CharacterControllerTests
{
    private ECDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ECDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ECDbContext(options);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedCharacter()
    {
        // Arrange

        // Creates a new in-memory database with a unique name "CreateControllerTest"
        using var context = CreateContext("CreateControllerTest");
        var service = new CharacterService(context);
        var controller = new CharacterController(service);
        var character = new Models.Character 
        { 
            Name = "Test Character",
            SpaceId = 1,
            CreatedBy = 4
        };

        // Act
        var result = await controller.Create(character);

        // Assert

        // Verify we got an OkObjectResult (HTTP 200 OK)
        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        // Verify the returned object is a Character
        var returnValue = Assert.IsType<Models.Character>(actionResult.Value);
        Assert.Equal(character.Name, returnValue.Name);
        
        var savedCharacter = await context.Characters.FindAsync(returnValue.Id);
        Assert.NotNull(savedCharacter);
        Assert.Equal(character.Name, savedCharacter.Name);
    }

    [Fact]
    public async Task GetAllBySpace_ShouldReturnAllCharactersBySpaceId()
    {
        // Arrange
        var dbName = "GetAllBySpaceControllerTest";
        var spaceId = 1;

        using (var context = CreateContext(dbName))
        {
            context.Characters.AddRange(
                new Models.Character { Name = "Character 1", SpaceId = spaceId },
                new Models.Character { Name = "Character 2", SpaceId = spaceId },
                new Models.Character { Name = "Other Space", SpaceId = 2 }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new CharacterService(context);
            var controller = new CharacterController(service);

            // Act
            var result = await controller.GetAllBySpace(spaceId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<Models.Character>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Contains(returnValue, c => c.Name == "Character 1");
            Assert.Contains(returnValue, c => c.Name == "Character 2");
            Assert.DoesNotContain(returnValue, c => c.Name == "Other Space");
        }
    }

    [Fact]
    public async Task Update_WithValidId_ShouldReturnUpdatedCharacter()
    {
        // Arrange
        var dbName = "UpdateCharacterValidControllerTest";
        int characterId;

        using (var context = CreateContext(dbName))
        {
            var character = new Models.Character 
            { 
                Name = "Original Name",
                SpaceId = 1,
                CreatedBy = 2
            };
            context.Characters.Add(character);
            await context.SaveChangesAsync();
            characterId = character.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new CharacterService(context);
            var controller = new CharacterController(service);
            var updatedCharacter = new CharacterUpdateDTO 
            { 
                Name = "Updated Name",
                IsDeleted = true
            };

            // Act
            var result = await controller.Update(characterId, updatedCharacter);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Models.Character>(actionResult.Value);
            Assert.Equal(updatedCharacter.Name, returnValue.Name);
            
            var savedCharacter = await context.Characters.FindAsync(characterId);
            Assert.NotNull(savedCharacter);
            Assert.Equal(updatedCharacter.Name, savedCharacter.Name);
            Assert.True(savedCharacter.IsDeleted);
        }
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        using var context = CreateContext("UpdateCharacterInvalidControllerTest");
        var service = new CharacterService(context);
        var controller = new CharacterController(service);
        var character = new CharacterUpdateDTO 
        { 
            Name = "Test Character"
        };

        // Act
        var result = await controller.Update(999, character);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}