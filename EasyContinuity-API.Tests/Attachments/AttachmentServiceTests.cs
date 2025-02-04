using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
using EasyContinuity_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EasyContinuity_API.Tests.Attachments;

public class AttachmentServiceTests
{
    private readonly IConfiguration _configuration;
    private readonly IImageCompressionService _compressionService;
    private readonly ICloudinaryStorageService _cloudinaryService;
    private readonly List<string> _uploadedPublicIds = new();

    public AttachmentServiceTests()
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
    public async Task AddAttachment_ShouldAddAttachmentAndReturnSuccess()
    {
        // Arrange
        using var context = CreateContext("AddAttachmentServiceTest");
        var service = new AttachmentService(context, _cloudinaryService);
        var attachment = new Models.Attachment
        {
            SpaceId = 1,
            Name = "Test Attachment",
            Path = "/path/to/file",
            Size = 1024,
            MimeType = "application/pdf",
            AddedOn = DateTime.UtcNow,
            AddedBy = 2
        };

        // Act
        var result = await service.AddAttachment(attachment);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        var returnedAttachment = result.Data!;
        Assert.NotEqual(0, returnedAttachment.Id);
        Assert.Equal(attachment.Name, returnedAttachment.Name);

        var savedAttachment = await context.Attachments.FindAsync(returnedAttachment.Id);
        Assert.NotNull(savedAttachment);
        Assert.Equal(attachment.Name, savedAttachment!.Name);
        Assert.Equal(attachment.Path, savedAttachment.Path);
        Assert.Equal(attachment.Size, savedAttachment.Size);
        Assert.Equal(attachment.MimeType, savedAttachment.MimeType);
        Assert.Equal(attachment.AddedBy, savedAttachment.AddedBy);
        Assert.Equal(attachment.SpaceId, savedAttachment.SpaceId);
        Assert.NotEqual(default(DateTime), savedAttachment.AddedOn);
    }

    [Fact]
    public async Task AddAttachment_WithoutSpaceId_ShouldReturnFailure()
    {
        // Arrange
        using var context = CreateContext("AddAttachmentNoSpaceTest");
        var service = new AttachmentService(context, _cloudinaryService);
        var attachment = new Models.Attachment
        {
            Name = "Test Attachment",
            Path = "/path/to/file",
            Size = 1024,
            MimeType = "application/pdf",
            AddedOn = DateTime.UtcNow,
            AddedBy = 2,
            SpaceId = 0  // Invalid space ID
        };

        // Act
        var result = await service.AddAttachment(attachment);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task AddAttachment_WithInvalidName_ShouldReturnFailure()
    {
        // Arrange
        using var context = CreateContext("AddAttachmentInvalidNameTest");
        var service = new AttachmentService(context, _cloudinaryService);
        var attachment = new Models.Attachment
        {
            SpaceId = 1,
            Name = new string('a', 151), // Name exceeds 150 characters
            Path = "path_to_file",
            Size = 1024,
            MimeType = "image/jpeg",
            AddedOn = DateTime.UtcNow,
            AddedBy = 2
        };

        // Act
        var result = await service.AddAttachment(attachment);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Name cannot exceed 150 characters", result.Message);
    }

    [Theory]
    [InlineData("Test File [1]", true)]
    [InlineData("My Document (2023)", true)]
    [InlineData("Report [Final] (v2).jpg", true)]
    [InlineData("Test@File", false)]
    [InlineData("File/Name", false)]
    [InlineData("File\\Name", false)]
    public async Task AddAttachment_WithVariousNames_ShouldValidateCorrectly(string fileName, bool shouldBeValid)
    {
        // Arrange
        using var context = CreateContext($"AddAttachment_{fileName}_Test");
        var service = new AttachmentService(context, _cloudinaryService);
        var attachment = new Models.Attachment
        {
            SpaceId = 1,
            Name = fileName,
            Path = "path_to_file",
            Size = 1024,
            MimeType = "image/jpeg",
            AddedOn = DateTime.UtcNow,
            AddedBy = 2
        };

        // Act
        var result = await service.AddAttachment(attachment);

        // Assert
        Assert.Equal(shouldBeValid, result.IsSuccess);
        if (!shouldBeValid)
        {
            Assert.Equal(400, result.StatusCode);
            Assert.Contains("Name can only contain", result.Message);
        }
    }

    [Fact]
    public async Task AddAttachment_WithoutRequiredMetadata_ShouldReturnFailure()
    {
        // Arrange
        using var context = CreateContext("AddAttachmentNoMetadataTest");
        var service = new AttachmentService(context, _cloudinaryService);
        var attachment = new Models.Attachment
        {
            SpaceId = 1,
            // Missing Name
            Path = "/path/to/file",
            // Missing Size
            // Missing MimeType
            AddedOn = DateTime.UtcNow,
            AddedBy = 2
        };

        // Act
        var result = await service.AddAttachment(attachment);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task Addttachment_WithValidCloudinaryPath_ShouldSucceed()
    {
        // Arrange
        using var context = CreateContext("AddAttachmentCloudinaryTest");
        var service = new AttachmentService(context, _cloudinaryService);
        var attachment = new Models.Attachment
        {
            Name = "Test Attachment",
            Path = "cloudinary_public_id",  // Cloudinary public ID format
            Size = 1024,
            MimeType = "image/jpeg",
            SpaceId = 1,
            AddedOn = DateTime.UtcNow,
            AddedBy = 2
        };

        // Act
        var result = await service.AddAttachment(attachment);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.Equal("cloudinary_public_id", result.Data.Path);
    }

    [Fact]
    public async Task AddAttachment_WithFileSizeExceedingLimit_ShouldReturnFailure()
    {
        // Arrange
        using var context = CreateContext("AddAttachmentLargeFileTest");
        var service = new AttachmentService(context, _cloudinaryService);
        var attachment = new Models.Attachment
        {
            SpaceId = 1,
            Name = "Large File",
            Path = "path_to_file",
            Size = 16 * 1024 * 1024,
            MimeType = "image/jpeg",
            AddedOn = DateTime.UtcNow,
            AddedBy = 2
        };

        // Act
        var result = await service.AddAttachment(attachment);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("File size exceeds maximum limit", result.Message);
    }

    [Fact]
    public async Task AddAttachment_WithMaxSnapshotImages_ShouldReturnFailure()
    {
        // Arrange
        var dbName = "AddAttachmentMaxImagesTest";
        using var context = CreateContext(dbName);
        var service = new AttachmentService(context, _cloudinaryService);
        var snapshotId = 1;

        // Add 6 attachments first
        for (int i = 0; i < 6; i++)
        {
            await context.Attachments.AddAsync(new Models.Attachment
            {
                SpaceId = 1,
                SnapshotId = snapshotId,
                Name = $"Existing Image {i}",
                Path = $"path_{i}",
                Size = 1024,
                MimeType = "image/jpeg",
                IsDeleted = false
            });
        }
        await context.SaveChangesAsync();

        var newAttachment = new Models.Attachment
        {
            SpaceId = 1,
            SnapshotId = snapshotId,
            Name = "One Too Many",
            Path = "path_7",
            Size = 1024,
            MimeType = "image/jpeg"
        };

        // Act
        var result = await service.AddAttachment(newAttachment);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("Maximum of 6 images per snapshot", result.Message);
    }

    [Fact]
    public async Task AddAttachment_WithMissingRequiredFields_ShouldReturnFirstError()
    {
        // Arrange
        using var context = CreateContext("AddAttachmentMissingFieldsTest");
        var service = new AttachmentService(context, _cloudinaryService);
        var attachment = new Models.Attachment
        {
            // All invalid fields but this should be the first error caught (SpaceId)
            SpaceId = 0,
            Name = string.Empty,
            Path = string.Empty,
            Size = 0,
            MimeType = string.Empty
        };

        // Act
        var result = await service.AddAttachment(attachment);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
        Assert.Contains("SpaceId", result.Message);
    }

    [Fact]
    public async Task AddAttachment_WithDeletedSnapshotAttachments_ShouldAllowNewAttachment()
    {
        // Arrange
        var dbName = "AddAttachmentWithDeletedTest";
        using var context = CreateContext(dbName);
        var service = new AttachmentService(context, _cloudinaryService);
        var snapshotId = 1;

        // Add 6 attachments but mark them as deleted
        for (int i = 0; i < 6; i++)
        {
            await context.Attachments.AddAsync(new Models.Attachment
            {
                SpaceId = 1,
                SnapshotId = snapshotId,
                Name = $"Deleted Image {i}",
                Path = $"path_{i}",
                Size = 1024,
                MimeType = "image/jpeg",
                IsDeleted = true
            });
        }
        await context.SaveChangesAsync();

        var newAttachment = new Models.Attachment
        {
            SpaceId = 1,
            SnapshotId = snapshotId,
            Name = "New Image",
            Path = "new_path",
            Size = 1024,
            MimeType = "image/jpeg"
        };

        // Act
        var result = await service.AddAttachment(newAttachment);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
    }

    [Fact]
    public async Task GetAllAttachmentsBySpaceId_ShouldReturnAllAttachmentsForSpace()
    {
        // Arrange
        var dbName = "GetAllAttachmentsSpaceTest";
        var spaceId = 1;

        using (var context = CreateContext(dbName))
        {
            context.Attachments.AddRange(
                new Models.Attachment { Name = "Attachment 1", SpaceId = spaceId, Path = "/path1" },
                new Models.Attachment { Name = "Attachment 2", SpaceId = spaceId, Path = "/path2" },
                new Models.Attachment { Name = "Other Space", SpaceId = 2, Path = "/path3" }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new AttachmentService(context, _cloudinaryService);

            // Act
            var result = await service.GetAllAttachmentsBySpaceId(spaceId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, s => s.Name == "Attachment 1");
            Assert.Contains(result.Data, s => s.Name == "Attachment 2");
            Assert.DoesNotContain(result.Data, s => s.Name == "Other Space");
        }
    }

    [Fact]
    public async Task GetAllAttachmentsByFolderId_ShouldReturnAllAttachmentsForFolder()
    {
        // Arrange
        var dbName = "GetAllAttachmentsFolderTest";
        var folderId = 1;

        using (var context = CreateContext(dbName))
        {
            context.Attachments.AddRange(
                new Models.Attachment { Name = "Attachment 1", FolderId = folderId, Path = "/path1" },
                new Models.Attachment { Name = "Attachment 2", FolderId = folderId, Path = "/path2" },
                new Models.Attachment { Name = "Other Folder", FolderId = 2, Path = "/path3" }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new AttachmentService(context, _cloudinaryService);

            // Act
            var result = await service.GetAllAttachmentsByFolderId(folderId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, s => s.Name == "Attachment 1");
            Assert.Contains(result.Data, s => s.Name == "Attachment 2");
            Assert.DoesNotContain(result.Data, s => s.Name == "Other Folder");
        }
    }

    [Fact]
    public async Task GetAllAttachmentsBySnapshotId_ShouldReturnAllAttachmentsForSnapshot()
    {
        // Arrange
        var dbName = "GetAllAttachmentsSnapshotTest";
        var snapshotId = 1;

        using (var context = CreateContext(dbName))
        {
            context.Attachments.AddRange(
                new Models.Attachment { Name = "Attachment 1", SnapshotId = snapshotId, Path = "/path1" },
                new Models.Attachment { Name = "Attachment 2", SnapshotId = snapshotId, Path = "/path2" },
                new Models.Attachment { Name = "Other Snapshot", SnapshotId = 2, Path = "/path3" }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new AttachmentService(context, _cloudinaryService);

            // Act
            var result = await service.GetAllAttachmentsBySnapshotId(snapshotId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, s => s.Name == "Attachment 1");
            Assert.Contains(result.Data, s => s.Name == "Attachment 2");
            Assert.DoesNotContain(result.Data, s => s.Name == "Other Snapshot");
        }
    }

    [Fact]
    public async Task GetAllRootAttachmentsBySpaceId_ShouldReturnOnlyRootAttachments()
    {
        // Arrange
        var dbName = "GetRootAttachmentsTest";
        var spaceId = 1;

        using (var context = CreateContext(dbName))
        {
            context.Attachments.AddRange(
                new Models.Attachment { Name = "Root 1", SpaceId = spaceId, FolderId = null },
                new Models.Attachment { Name = "Root 2", SpaceId = spaceId, FolderId = null },
                new Models.Attachment { Name = "In Folder", SpaceId = spaceId, FolderId = 1 }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new AttachmentService(context, _cloudinaryService);

            // Act
            var result = await service.GetAllRootAttachmentsBySpaceId(spaceId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Count);
            Assert.Contains(result.Data, s => s.Name == "Root 1");
            Assert.Contains(result.Data, s => s.Name == "Root 2");
            Assert.DoesNotContain(result.Data, s => s.Name == "In Folder");
        }
    }

    [Fact]
    public async Task GetSingleAttachmentById_WithValidId_ShouldReturnAttachment()
    {
        // Arrange
        var dbName = "GetSingleAttachmentValidTest";
        int attachmentId;
        var dateAdded = DateTime.UtcNow;

        using (var context = CreateContext(dbName))
        {
            var attachment = new Models.Attachment
            {
                Name = "Test Attachment",
                Path = "/path/to/file",
                Size = 1024,
                MimeType = "application/pdf",
                AddedOn = dateAdded,
                AddedBy = 2
            };
            context.Attachments.Add(attachment);
            await context.SaveChangesAsync();
            attachmentId = attachment.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new AttachmentService(context, _cloudinaryService);

            // Act
            var result = await service.GetSingleAttachmentById(attachmentId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal("Test Attachment", result.Data.Name);
            Assert.Equal("/path/to/file", result.Data.Path);
            Assert.Equal(1024, result.Data.Size);
            Assert.Equal("application/pdf", result.Data.MimeType);
            Assert.Equal(dateAdded, result.Data.AddedOn);
            Assert.Equal(2, result.Data.AddedBy);
        }
    }

    [Fact]
    public async Task GetSingleAttachmentById_WithInvalidId_ShouldReturnFailResponse()
    {
        // Arrange
        using var context = CreateContext("GetSingleAttachmentInvalidTest");
        var service = new AttachmentService(context, _cloudinaryService);

        // Act
        var result = await service.GetSingleAttachmentById(999);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Attachment Not Found", result.Message);
    }

    [Fact]
    public async Task UpdateAttachment_WithValidId_ShouldUpdateAndReturnAttachment()
    {
        // Arrange
        var dbName = "UpdateAttachmentValidTest";
        int attachmentId;
        var dateAdded = DateTime.UtcNow.AddDays(-1);
        var dateUpdated = DateTime.UtcNow;

        using (var context = CreateContext(dbName))
        {
            var attachment = new Models.Attachment
            {
                Name = "Original Name",
                Path = "/test/path",
                Size = 1024,
                MimeType = "application/pdf",
                IsDeleted = false,
                AddedOn = dateAdded,
                AddedBy = 3,
                IsStored = true
            };
            context.Attachments.Add(attachment);
            await context.SaveChangesAsync();
            attachmentId = attachment.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new AttachmentService(context, _cloudinaryService);
            var updatedAttachment = new AttachmentUpdateDTO
            {
                Name = "Updated Name",
                Path = "/updated/path",
                Size = 2048,
                MimeType = "image/jpeg",
                IsDeleted = true,
                LastUpdatedOn = dateUpdated,
                LastUpdatedBy = 4
            };

            // Act
            var result = await service.UpdateAttachment(attachmentId, updatedAttachment);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);

            Assert.Equal(updatedAttachment.Name, result.Data.Name);
            Assert.Equal(updatedAttachment.Path, result.Data.Path);
            Assert.Equal(updatedAttachment.Size, result.Data.Size);
            Assert.Equal(updatedAttachment.MimeType, result.Data.MimeType);
            Assert.True(result.Data.IsDeleted);
            Assert.Equal(dateAdded, result.Data.AddedOn);
            Assert.Equal(3, result.Data.AddedBy);
            Assert.Equal(dateUpdated, result.Data.LastUpdatedOn);
            Assert.Equal(4, result.Data.LastUpdatedBy);

            var savedAttachment = await context.Attachments.FindAsync(attachmentId);
            Assert.NotNull(savedAttachment);
            Assert.Equal(updatedAttachment.Name, savedAttachment!.Name);
            Assert.Equal(updatedAttachment.Path, savedAttachment.Path);
            Assert.Equal(updatedAttachment.Size, savedAttachment.Size);
            Assert.Equal(updatedAttachment.MimeType, savedAttachment.MimeType);
            Assert.True(savedAttachment.IsDeleted);
            Assert.Equal(dateAdded, savedAttachment.AddedOn);
            Assert.Equal(3, savedAttachment.AddedBy);
            Assert.Equal(dateUpdated, savedAttachment.LastUpdatedOn);
            Assert.Equal(4, savedAttachment.LastUpdatedBy);
        }
    }

    [Fact]
    public async Task UpdateAttachment_WithInvalidId_ShouldReturnFailResponse()
    {
        // Arrange
        using var context = CreateContext("UpdateAttachmentInvalidTest");
        var service = new AttachmentService(context, _cloudinaryService);
        var updatedAttachment = new AttachmentUpdateDTO
        {
            Name = "Updated Name",
            Path = "/updated/path"
        };

        // Act
        var result = await service.UpdateAttachment(999, updatedAttachment);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
        Assert.Equal("Attachment Not Found", result.Message);
    }

    [Fact]
    public async Task UpdateAttachment_WithNoChanges_ShouldNotModifyDatabase()
    {
        // Arrange
        var dbName = "UpdateAttachmentNoChangesTest";
        int attachmentId;
        DateTime originalUpdateTime;

        using (var context = CreateContext(dbName))
        {
            var attachment = new Models.Attachment
            {
                Name = "Test Name",
                Path = "/test/path",
                Size = 2048,
                MimeType = "image/jpeg",
                LastUpdatedOn = DateTime.UtcNow
            };
            context.Attachments.Add(attachment);
            await context.SaveChangesAsync();
            attachmentId = attachment.Id;
            originalUpdateTime = attachment.LastUpdatedOn.Value;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new AttachmentService(context, _cloudinaryService);
            var updatedAttachment = new AttachmentUpdateDTO
            {
                Name = "Test Name",
                Path = "/test/path",
                Size = 2048,
                MimeType = "image/jpeg",
            };

            // Act
            var result = await service.UpdateAttachment(attachmentId, updatedAttachment);

            // Assert
            Assert.True(result.IsSuccess);
            var savedAttachment = await context.Attachments.FindAsync(attachmentId);
            Assert.NotNull(savedAttachment);
            Assert.Equal(originalUpdateTime, savedAttachment!.LastUpdatedOn!.Value);
        }
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