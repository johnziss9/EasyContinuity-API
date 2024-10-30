using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
using EasyContinuity_API.Services;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Tests.Characters;

public class CharacterServiceTests
{
    private ECDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ECDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new ECDbContext(options);
    }

    [Fact]
    public async Task CreateCharacter_ShouldAddCharacterAndReturnSuccess()
    {
        // Arrange
        using var context = CreateContext("CreateCharacterServiceTest");
        var service = new CharacterService(context);
        var character = new Models.Character 
        { 
            Name = "Test Character",
            SpaceId = 1,
            CreatedBy = 2,
            CreatedOn = DateTime.UtcNow
        };

        // Act
        var result = await service.CreateCharacter(character);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        var returnedCharacter = result.Data!;
        Assert.NotEqual(0, returnedCharacter.Id);
        Assert.Equal(character.Name, returnedCharacter.Name);
        
        var savedCharacter = await context.Characters.FindAsync(result.Data.Id);
        Assert.NotNull(savedCharacter);
        Assert.Equal(character.Name, savedCharacter!.Name);
        Assert.Equal(character.SpaceId, savedCharacter.SpaceId);
        Assert.Equal(character.CreatedBy, savedCharacter.CreatedBy);
    }

    [Fact]
    public async Task GetAllCharactersBySpaceId_ShouldReturnAllCharactersForSpace()
    {
        // Arrange
        var dbName = "GetAllCharactersSpaceTest";
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

            // Act
            var result = await service.GetAllCharactersBySpaceId(spaceId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, c => c.Name == "Character 1");
            Assert.Contains(result.Data, c => c.Name == "Character 2");
            Assert.DoesNotContain(result.Data, c => c.Name == "Other Space");
        }
    }

    [Fact]
    public async Task UpdateCharacter_WithValidId_ShouldUpdateAndReturnCharacter()
    {
        // Arrange
        var dbName = "UpdateCharacterValidTest";
        int characterId;
        var dateCreated = DateTime.UtcNow.AddDays(-1);
        var dateUpdated = DateTime.UtcNow;

        using (var context = CreateContext(dbName))
        {
            var character = new Models.Character 
            { 
                Name = "Original Name",
                SpaceId = 1,
                CreatedOn = dateCreated,
                CreatedBy = 4
            };
            context.Characters.Add(character);
            await context.SaveChangesAsync();
            characterId = character.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new CharacterService(context);
            var updatedCharacter = new CharacterUpdateDTO
            {
                Name = "Updated Name",
                IsDeleted = true,
                LastUpdatedOn = dateUpdated,
                LastUpdatedBy = 3
            };

            // Act
            var result = await service.UpdateCharacter(characterId, updatedCharacter);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(updatedCharacter.Name, result.Data.Name);
            Assert.True(result.Data.IsDeleted);
            Assert.Equal(dateCreated, result.Data.CreatedOn);
            Assert.Equal(4, result.Data.CreatedBy);
            Assert.Equal(dateUpdated, result.Data.LastUpdatedOn);
            Assert.Equal(3, result.Data.LastUpdatedBy);

            var savedCharacter = await context.Characters.FindAsync(characterId);
            Assert.NotNull(savedCharacter);
            Assert.Equal(updatedCharacter.Name, savedCharacter!.Name);
            Assert.True(savedCharacter.IsDeleted);
        }
    }

    [Fact]
    public async Task UpdateCharacter_WithInvalidId_ShouldReturnFailResponse()
    {
        // Arrange
        using var context = CreateContext("UpdateCharacterInvalidTest");
        var service = new CharacterService(context);
        var updatedCharacter = new CharacterUpdateDTO 
        { 
            Name = "Updated Name"
        };

        // Act
        var result = await service.UpdateCharacter(999, updatedCharacter);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Character Not Found", result.Message);
    }

    [Fact]
    public async Task UpdateCharacter_WithNoChanges_ShouldNotModifyDatabase()
    {
        // Arrange
        var dbName = "UpdateCharacterNoChangesTest";
        int characterId;
        DateTime originalUpdateTime;

        using (var context = CreateContext(dbName))
        {
            var character = new Models.Character 
            { 
                Name = "Original Name",
                LastUpdatedOn = DateTime.UtcNow
            };
            context.Characters.Add(character);
            await context.SaveChangesAsync();
            characterId = character.Id;
            originalUpdateTime = character.LastUpdatedOn.Value;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new CharacterService(context);
            var updatedCharacter = new CharacterUpdateDTO
            {
                Name = "Original Name",
            };

            // Act
            var result = await service.UpdateCharacter(characterId, updatedCharacter);

            // Assert
            Assert.True(result.IsSuccess);
            var savedCharacter = await context.Characters.FindAsync(characterId);
            Assert.NotNull(savedCharacter);
            Assert.NotNull(savedCharacter!.LastUpdatedOn);
            Assert.Equal(originalUpdateTime, savedCharacter.LastUpdatedOn!.Value);
        }
    }
}
