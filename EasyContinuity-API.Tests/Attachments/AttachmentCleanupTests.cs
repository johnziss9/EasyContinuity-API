using EasyContinuity_API.Data;
using EasyContinuity_API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EasyContinuity_API.Tests.Attachments;

public class AttachmentCleanupTests
{
    private readonly IConfiguration _configuration;
    private readonly IImageCompressionService _compressionService;
    private readonly ICloudinaryStorageService _cloudinaryService;
    private readonly List<string> _uploadedPublicIds = new();

    public AttachmentCleanupTests()
    {
        // Get credentials from environment or .env file
        var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME");
        var apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY");
        var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET");

        if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            var currentDir = Directory.GetCurrentDirectory();
            var solutionDir = Directory.GetParent(currentDir)?.Parent?.Parent?.Parent?.FullName;
            var envPath = Path.Combine(solutionDir!, ".env");

            if (File.Exists(envPath))
            {
                var envFile = File.ReadAllLines(envPath);
                cloudName = envFile.FirstOrDefault(l => l.StartsWith("CLOUDINARY_CLOUD_NAME="))?.Split('=')[1];
                apiKey = envFile.FirstOrDefault(l => l.StartsWith("CLOUDINARY_API_KEY="))?.Split('=')[1];
                apiSecret = envFile.FirstOrDefault(l => l.StartsWith("CLOUDINARY_API_SECRET="))?.Split('=')[1];
            }
        }

        if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
        {
            throw new InvalidOperationException("Cloudinary credentials are missing.");
        }

        var configValues = new Dictionary<string, string?>
        {
            {"CLOUDINARY_CLOUD_NAME", cloudName},
            {"CLOUDINARY_API_KEY", apiKey},
            {"CLOUDINARY_API_SECRET", apiSecret}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();

        _compressionService = new ImageCompressionService();
        _cloudinaryService = new CloudinaryStorageService(_configuration, _compressionService);
    }

    private ECDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ECDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ECDbContext(options);
    }

    private IFormFile CreateTestImage(string filename = "test.jpg", string contentType = "image/jpeg")
    {
        using var image = new Image<Rgba32>(100, 100);
        var stream = new MemoryStream();

        if (contentType == "image/jpeg")
        {
            image.SaveAsJpeg(stream);
        }
        else if (contentType == "image/png")
        {
            image.SaveAsPng(stream);
        }

        stream.Position = 0;

        return new FormFile(stream, 0, stream.Length, "test", filename)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    [Fact]
    public async Task CleanupService_ShouldDeleteStoredAndDeletedAttachments()
    {
        // Arrange
        using var context = CreateContext("CleanupDeletedAttachmentsTest");
        var service = new AttachmentCleanupService(context, _cloudinaryService);

        // Upload test file to Cloudinary
        var testFile = CreateTestImage();
        var uploadResult = await _cloudinaryService.UploadAsync(testFile);
        Assert.True(uploadResult.IsSuccess);
        var cloudinaryPublicId = uploadResult.Data!;
        _uploadedPublicIds.Add(cloudinaryPublicId);

        // Create test attachments
        var attachments = new[]
        {
            new Attachment
            {
                Name = "Should Delete",
                Path = cloudinaryPublicId,
                IsDeleted = true,
                IsStored = true
            },
            new Attachment
            {
                Name = "Not Deleted",
                Path = "path2",
                IsDeleted = false,
                IsStored = true
            },
            new Attachment
            {
                Name = "Deleted But Not Stored",
                Path = "path3",
                IsDeleted = true,
                IsStored = false
            }
        };

        context.Attachments.AddRange(attachments);
        await context.SaveChangesAsync();

        // Act
        await service.CleanupDeletedAttachments();

        // Assert
        var updatedAttachment = await context.Attachments.FindAsync(attachments[0].Id);
        Assert.NotNull(updatedAttachment);
        Assert.False(updatedAttachment.IsStored);

        // Verify file was deleted from Cloudinary
        var existsResult = await _cloudinaryService.ExistsAsync(cloudinaryPublicId);
        Assert.False(existsResult.Data);

        // Other attachments should be unchanged
        var unchangedAttachment1 = await context.Attachments.FindAsync(attachments[1].Id);
        Assert.True(unchangedAttachment1!.IsStored);

        var unchangedAttachment2 = await context.Attachments.FindAsync(attachments[2].Id);
        Assert.False(unchangedAttachment2!.IsStored);
    }

    [Fact]
    public async Task CleanupJob_ShouldExecuteCleanupService()
    {
        // Arrange
        var dbName = "CleanupJobTest";
        var context = CreateContext(dbName);

        var services = new ServiceCollection();
        services.AddSingleton<ECDbContext>(context);  // Use context instance
        services.AddScoped<ICloudinaryStorageService>(_ => _cloudinaryService);
        services.AddScoped<AttachmentCleanupService>();

        var serviceProvider = services.BuildServiceProvider();
        var job = new AttachmentCleanupJob(serviceProvider);

        // Upload test file to Cloudinary
        var testFile = CreateTestImage();
        var uploadResult = await _cloudinaryService.UploadAsync(testFile);
        Assert.True(uploadResult.IsSuccess);
        var cloudinaryPublicId = uploadResult.Data!;
        _uploadedPublicIds.Add(cloudinaryPublicId);

        var attachment = new Attachment
        {
            Name = "Test Attachment",
            Path = cloudinaryPublicId,
            IsDeleted = true,
            IsStored = true
        };
        context.Attachments.Add(attachment);
        await context.SaveChangesAsync();
        var attachmentId = attachment.Id;

        // Act
        await job.Execute(null!);

        // Assert
        var updatedAttachment = await context.Attachments.FindAsync(attachmentId);
        Assert.NotNull(updatedAttachment);
        Assert.False(updatedAttachment.IsStored);
    }

    [Fact]
    public async Task CleanupService_WhenCloudinaryFails_ShouldNotUpdateIsStored()
    {
        // Arrange
        using var context = CreateContext("CleanupCloudinaryFailTest");
        var service = new AttachmentCleanupService(context, _cloudinaryService);

        var attachment = new Attachment
        {
            Name = "Test Attachment",
            Path = "invalid_cloudinary_id",  // Invalid ID will cause deletion to fail
            IsDeleted = true,
            IsStored = true
        };
        context.Attachments.Add(attachment);
        await context.SaveChangesAsync();

        // Act
        await service.CleanupDeletedAttachments();

        // Assert
        var updatedAttachment = await context.Attachments.FindAsync(attachment.Id);
        Assert.NotNull(updatedAttachment);
        Assert.True(updatedAttachment.IsStored);  // Should still be marked as stored
    }

    [Fact]
    public async Task CleanupService_WithMultipleRecords_ShouldProcessAll()
    {
        // Arrange
        using var context = CreateContext("CleanupMultipleRecordsTest");
        var service = new AttachmentCleanupService(context, _cloudinaryService);

        // Upload multiple test files
        var cloudinaryIds = new List<string>();
        for (int i = 0; i < 3; i++)
        {
            var testFile = CreateTestImage($"test{i}.jpg");
            var uploadResult = await _cloudinaryService.UploadAsync(testFile);
            Assert.True(uploadResult.IsSuccess);
            cloudinaryIds.Add(uploadResult.Data!);
            _uploadedPublicIds.Add(uploadResult.Data!);
        }

        var attachments = cloudinaryIds.Select(id => new Attachment
        {
            Name = $"Test Attachment {id}",
            Path = id,
            IsDeleted = true,
            IsStored = true
        }).ToList();

        context.Attachments.AddRange(attachments);
        await context.SaveChangesAsync();

        // Act
        await service.CleanupDeletedAttachments();

        // Assert
        foreach (var attachment in attachments)
        {
            var updatedAttachment = await context.Attachments.FindAsync(attachment.Id);
            Assert.NotNull(updatedAttachment);
            Assert.False(updatedAttachment.IsStored);

            var existsResult = await _cloudinaryService.ExistsAsync(attachment.Path);
            Assert.False(existsResult.Data);
        }
    }

    [Fact]
    public async Task CleanupService_WithInvalidCloudinaryId_ShouldHandleGracefully()
    {
        // Arrange
        using var context = CreateContext("CleanupInvalidIdTest");
        var service = new AttachmentCleanupService(context, _cloudinaryService);

        // Upload one valid file and create one invalid entry
        var testFile = CreateTestImage();
        var uploadResult = await _cloudinaryService.UploadAsync(testFile);
        Assert.True(uploadResult.IsSuccess);
        var validId = uploadResult.Data!;
        _uploadedPublicIds.Add(validId);

        var attachments = new[]
        {
            new Attachment
            {
                Name = "Valid ID",
                Path = validId,
                IsDeleted = true,
                IsStored = true
            },
            new Attachment
            {
                Name = "Invalid ID",
                Path = "definitely_invalid_id",
                IsDeleted = true,
                IsStored = true
            }
        };

        context.Attachments.AddRange(attachments);
        await context.SaveChangesAsync();

        // Act
        await service.CleanupDeletedAttachments();

        // Assert
        // Valid ID attachment should be processed
        var validAttachment = await context.Attachments.FindAsync(attachments[0].Id);
        Assert.NotNull(validAttachment);
        Assert.False(validAttachment.IsStored);

        // Invalid ID attachment should be handled gracefully
        var invalidAttachment = await context.Attachments.FindAsync(attachments[1].Id);
        Assert.NotNull(invalidAttachment);
        Assert.True(invalidAttachment.IsStored);  // Should still be marked as stored since deletion failed
    }

    protected virtual void Dispose()
    {
        foreach (var publicId in _uploadedPublicIds)
        {
            try
            {
                _cloudinaryService.DeleteAsync(publicId).Wait();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}