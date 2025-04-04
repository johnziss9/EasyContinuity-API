using EasyContinuity_API.Controllers;
using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
using EasyContinuity_API.Models;
using EasyContinuity_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace EasyContinuity_API.Tests.Auth;

public class AuthControllerTests
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
    public async Task Register_ShouldReturnUser()
    {
        // Arrange
        using var context = CreateContext("RegisterControllerTest");
        var jwtSettings = GetJwtSettings();
        var authService = new AuthenticationService(context, jwtSettings);
        var controller = new AuthenticationController(authService);
        
        var registerDto = new RegisterDto
        {
            Email = "test@example.com",
            FirstName = "Test",
            LastName = "User",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        // Act
        var result = await controller.Register(registerDto);

        // Assert
        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        var userDto = Assert.IsType<UserDto>(actionResult.Value);
        
        Assert.Equal(registerDto.Email, userDto.Email);
        Assert.Equal(registerDto.FirstName, userDto.FirstName);
        Assert.Equal(registerDto.LastName, userDto.LastName);
        Assert.NotEmpty(userDto.Token);
    }

    [Fact]
    public async Task Register_WithExistingEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var dbName = "RegisterExistingEmailControllerTest";
        
        // Add user with email
        using (var context = CreateContext(dbName))
        {
            CreatePasswordHash("Password123!", out byte[] passwordHash, out byte[] passwordSalt);
            
            var user = new User
            {
                Email = "existing@example.com",
                FirstName = "Existing",
                LastName = "User",
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt
            };
            
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }
        
        // Now try to register with same email
        using (var context = CreateContext(dbName))
        {
            var jwtSettings = GetJwtSettings();
            var authService = new AuthenticationService(context, jwtSettings);
            var controller = new AuthenticationController(authService);
            
            var registerDto = new RegisterDto
            {
                Email = "existing@example.com",
                FirstName = "New",
                LastName = "User",
                Password = "Password123!",
                ConfirmPassword = "Password123!"
            };

            // Act
            var result = await controller.Register(registerDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }

    [Fact]
    public async Task Register_WithInvalidModel_ShouldReturnBadRequest()
    {
        // Arrange
        using var context = CreateContext("RegisterInvalidModelControllerTest");
        var jwtSettings = GetJwtSettings();
        var authService = new AuthenticationService(context, jwtSettings);
        var controller = new AuthenticationController(authService);
        
        var registerDto = new RegisterDto
        {
            // Missing required fields
            Email = "",
            FirstName = "",
            LastName = "",
            Password = "pass", // Too short
            ConfirmPassword = "different" // Doesn't match
        };
        
        // Manually add model errors to simulate validation failing
        controller.ModelState.AddModelError("Email", "Email is required.");
        controller.ModelState.AddModelError("FirstName", "First Name is required.");
        controller.ModelState.AddModelError("LastName", "Last Name is required.");
        controller.ModelState.AddModelError("Password", "Password must be at least 8 characters.");
        controller.ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");

        // Act
        var result = await controller.Register(registerDto);

        // Assert
        var actionResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(actionResult.Value);
        Assert.Equal(422, problemDetails.Status);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnUser()
    {
        // Arrange
        var dbName = "LoginValidControllerTest";
        
        // Create a user
        using (var context = CreateContext(dbName))
        {
            CreatePasswordHash("Password123!", out byte[] passwordHash, out byte[] passwordSalt);
            
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
            var authService = new AuthenticationService(context, jwtSettings);
            var controller = new AuthenticationController(authService);
            
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "Password123!"
            };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var userDto = Assert.IsType<UserDto>(actionResult.Value);
            
            Assert.Equal("test@example.com", userDto.Email);
            Assert.NotEmpty(userDto.Token);
            
            // Check that LastLoginOn was updated
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
            Assert.NotNull(user);
            Assert.NotNull(user!.LastLoginOn);
        }
    }

    [Fact]
    public async Task Login_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        using var context = CreateContext("LoginInvalidEmailControllerTest");
        var jwtSettings = GetJwtSettings();
        var authService = new AuthenticationService(context, jwtSettings);
        var controller = new AuthenticationController(authService);
        
        var loginDto = new LoginDto
        {
            Email = "nonexistent@example.com",
            Password = "Password123!"
        };

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var dbName = "LoginInvalidPasswordControllerTest";
        
        // Create a user
        using (var context = CreateContext(dbName))
        {
            CreatePasswordHash("Password123!", out byte[] passwordHash, out byte[] passwordSalt);
            
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
            var authService = new AuthenticationService(context, jwtSettings);
            var controller = new AuthenticationController(authService);
            
            var loginDto = new LoginDto
            {
                Email = "test@example.com",
                Password = "WrongPassword123!"
            };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }

    [Fact]
    public async Task Login_WithDeletedUser_ShouldReturnBadRequest()
    {
        // Arrange
        var dbName = "LoginDeletedUserControllerTest";
        
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
            var authService = new AuthenticationService(context, jwtSettings);
            var controller = new AuthenticationController(authService);
            
            var loginDto = new LoginDto
            {
                Email = "deleted@example.com",
                Password = "Password123!"
            };

            // Act
            var result = await controller.Login(loginDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }
    }

    [Fact]
    public async Task Login_WithInvalidModel_ShouldReturnBadRequest()
    {
        // Arrange
        using var context = CreateContext("LoginInvalidModelControllerTest");
        var jwtSettings = GetJwtSettings();
        var authService = new AuthenticationService(context, jwtSettings);
        var controller = new AuthenticationController(authService);
        
        var loginDto = new LoginDto
        {
            Email = "", // Missing required field
            Password = "" // Missing required field
        };
        
        // Manually add model errors to simulate validation failing
        controller.ModelState.AddModelError("Email", "Email is required.");
        controller.ModelState.AddModelError("Password", "Password is required.");

        // Act
        var result = await controller.Login(loginDto);

        // Assert
        var actionResult = Assert.IsType<UnprocessableEntityObjectResult>(result.Result);
        var problemDetails = Assert.IsType<ProblemDetails>(actionResult.Value);
        Assert.Equal(422, problemDetails.Status);
    }
}