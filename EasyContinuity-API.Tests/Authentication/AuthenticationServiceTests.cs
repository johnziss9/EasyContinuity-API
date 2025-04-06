using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
using EasyContinuity_API.Models;
using EasyContinuity_API.Services;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EasyContinuity_API.Tests.Auth;

public class AuthServiceTests
{
    private ECDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ECDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ECDbContext(options);
    }

    private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        using var hmac = new HMACSHA512();
        passwordSalt = hmac.Key;
        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
    }

    private JwtSettings GetJwtSettings()
    {
        return new JwtSettings
        {
            Key = "test-key-with-at-least-32-characters-for-testing",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 60
        };
    }

    [Fact]
    public async Task RegisterAsync_ShouldAddUserAndReturnSuccess()
    {
        // Arrange
        using var context = CreateContext("RegisterAsyncTest");
        var jwtSettings = GetJwtSettings();
        var service = new AuthenticationService(context, jwtSettings);

        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act
        var result = await service.Register(registerDto);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal(200, result.StatusCode);

        var returnedUser = result.Data!;
        Assert.NotEqual(0, returnedUser.Id);
        Assert.Equal(registerDto.Email, returnedUser.Email);
        Assert.Equal(registerDto.FirstName, returnedUser.FirstName);
        Assert.Equal(registerDto.LastName, returnedUser.LastName);
        Assert.NotEmpty(returnedUser.Token);

        // Check database
        var savedUser = await context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
        Assert.NotNull(savedUser);
        Assert.Equal(registerDto.FirstName, savedUser!.FirstName);
        Assert.NotEmpty(savedUser.PasswordHash);
        Assert.NotEmpty(savedUser.PasswordSalt);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingEmail_ShouldReturnFailResponse()
    {
        // Arrange
        var dbName = "RegisterAsyncExistingEmailTest";

        // Add user with email
        using (var context = CreateContext(dbName))
        {
            var user = new User
            {
                Email = "existing@example.com",
                FirstName = "Existing",
                LastName = "User",
                PasswordHash = new byte[64],
                PasswordSalt = new byte[128]
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Now try to register with same email
        using (var context = CreateContext(dbName))
        {
            var jwtSettings = GetJwtSettings();
            var service = new AuthenticationService(context, jwtSettings);

            var registerDto = new RegisterDto
            {
                Email = "existing@example.com",
                FirstName = "New",
                LastName = "User",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act
            var result = await service.Register(registerDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Email is already registered.", result.Message);
        }
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var dbName = "LoginAsyncValidTest";
        var password = "Password123!";

        // Create a user
        using (var context = CreateContext(dbName))
        {
            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Login with the user
        using (var context = CreateContext(dbName))
        {
            var jwtSettings = GetJwtSettings();
            var service = new AuthenticationService(context, jwtSettings);

            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = password
            };

            // Act
            var result = await service.Login(loginDto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(200, result.StatusCode);

            var returnedUser = result.Data!;
            Assert.Equal("test@example.com", returnedUser.Email);
            Assert.NotEmpty(returnedUser.Token);

            // Check that LastLoginOn was updated
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            Assert.NotNull(user);
            Assert.NotNull(user!.LastLoginOn);
        }
    }

    [Fact]
    public async Task LoginAsync_WithInvalidEmail_ShouldReturnFailResponse()
    {
        // Arrange
        using var context = CreateContext("LoginAsyncInvalidEmailTest");
        var jwtSettings = GetJwtSettings();
        var service = new AuthenticationService(context, jwtSettings);

        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await service.Login(loginDto);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Equal("Invalid email or password.", result.Message);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidPassword_ShouldReturnFailResponse()
    {
        // Arrange
        var dbName = "LoginAsyncInvalidPasswordTest";

        // Create a user
        using (var context = CreateContext(dbName))
        {
            CreatePasswordHash("CorrectPassword123!", out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Try to login with wrong password
        using (var context = CreateContext(dbName))
        {
            var jwtSettings = GetJwtSettings();
            var service = new AuthenticationService(context, jwtSettings);

            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword123!"
            };

            // Act
            var result = await service.Login(loginDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid email or password.", result.Message);
        }
    }

    [Fact]
    public async Task LoginAsync_WithDeletedUser_ShouldReturnFailResponse()
    {
        // Arrange
        var dbName = "LoginAsyncDeletedUserTest";

        // Create a deleted user
        using (var context = CreateContext(dbName))
        {
            CreatePasswordHash("Password123!", out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Email = "deleted@example.com",
                FirstName = "Deleted",
                LastName = "User",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                IsDeleted = true
            };

            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Try to login with deleted user
        using (var context = CreateContext(dbName))
        {
            var jwtSettings = GetJwtSettings();
            var service = new AuthenticationService(context, jwtSettings);

            var loginDto = new LoginDto
            {
                Email = "deleted@example.com",
                Password = "Password123!"
            };

            // Act
            var result = await service.Login(loginDto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
            Assert.Equal("Invalid email or password.", result.Message);
        }
    }

    [Fact]
    public void VerifyPasswordHash_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        using var context = CreateContext("VerifyPasswordHashTest");
        var jwtSettings = GetJwtSettings();
        var service = new AuthenticationService(context, jwtSettings);

        var password = "Password123!";
        CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

        // Use reflection to access private method
        var methodInfo = typeof(AuthenticationService).GetMethod("VerifyPasswordHash",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Ensure the method was found
        Assert.NotNull(methodInfo);

        // Act
        var invokeResult = methodInfo.Invoke(service, new object[] { password, passwordHash, passwordSalt });

        // Assert
        Assert.NotNull(invokeResult);
        var result = (bool)invokeResult;
        Assert.True(result);
    }

    [Fact]
    public void VerifyPasswordHash_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        using var context = CreateContext("VerifyPasswordHashIncorrectTest");
        var jwtSettings = GetJwtSettings();
        var service = new AuthenticationService(context, jwtSettings);

        var correctPassword = "Password123!";
        var wrongPassword = "WrongPassword123!";
        CreatePasswordHash(correctPassword, out byte[] passwordHash, out byte[] passwordSalt);

        // Use reflection to access private method
        var methodInfo = typeof(AuthenticationService).GetMethod("VerifyPasswordHash",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Ensure the method was found
        Assert.NotNull(methodInfo);

        // Act
        var invokeResult = methodInfo.Invoke(service, new object[] { wrongPassword, passwordHash, passwordSalt });

        // Assert
        Assert.NotNull(invokeResult);
        var result = (bool)invokeResult;
        Assert.False(result);
    }

    [Fact]
    public void GenerateJwtToken_ShouldReturnValidToken()
    {
        // Arrange
        using var context = CreateContext("GenerateJwtTokenTest");
        var jwtSettings = GetJwtSettings();
        var service = new AuthenticationService(context, jwtSettings);

        var user = new User
        {
            Id = 1,
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User"
        };

        // Use reflection to access private method
        var methodInfo = typeof(AuthenticationService).GetMethod("GenerateJwtToken",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // Ensure the method was found
        Assert.NotNull(methodInfo);

        // Act
        var result = methodInfo.Invoke(service, new object[] { user });

        // Assert
        Assert.NotNull(result);
        var token = (string)result;
        Assert.NotEmpty(token);
        Assert.StartsWith("ey", token); // JWT tokens start with "ey"
        Assert.Contains(".", token); // JWT tokens contain periods
    }
}