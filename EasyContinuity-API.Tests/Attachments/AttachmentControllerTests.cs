using EasyContinuity_API.Controllers;
using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
using EasyContinuity_API.Models;
using EasyContinuity_API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace EasyContinuity_API.Tests.Attachments;

public class AttachmentControllerTests
{
    private readonly List<string> _uploadedPublicIds = new();
    private readonly IConfiguration _configuration;
    private readonly IImageCompressionService _compressionService;
    private readonly ICloudinaryStorageService _cloudinaryService;

    public AttachmentControllerTests()
    {
        // Reuse your configuration setup from CloudinaryStorageServiceTests
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
    public async Task Add_WithValidFiles_ShouldReturnCreatedAttachments()
    {
        // Arrange
        using var context = CreateContext("AddFilesControllerTest");
        var service = new AttachmentService(context, _cloudinaryService);
        var controller = new AttachmentController(service, _cloudinaryService);

        var files = new FormFileCollection
    {
        CreateTestImage("test1.jpg"),
        CreateTestImage("test2.jpg")
    };

        var form = new FormCollection(
            new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(),
            files
        );

        try
        {
            // Act
            var result = await controller.Add(form, 1);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var attachments = Assert.IsType<List<Attachment>>(actionResult.Value);

            Assert.Equal(2, attachments.Count);
            foreach (var attachment in attachments)
            {
                _uploadedPublicIds.Add(attachment.Path);
                Assert.NotNull(attachment.Path);
                Assert.True(attachment.IsStored);
            }
        }
        finally
        {
            // Clean up the files immediately after test
            foreach (var publicId in _uploadedPublicIds)
            {
                var deleteResult = await _cloudinaryService.DeleteAsync(publicId);
            }
        }
    }

    [Fact]
    public async Task Add_WithTooManyFilesForSnapshot_ShouldReturnBadRequest()
    {
        // Arrange
        using var context = CreateContext("AddTooManyFilesTest");
        var service = new AttachmentService(context, _cloudinaryService);
        var controller = new AttachmentController(service, _cloudinaryService);

        // Add 5 existing attachments
        for (int i = 0; i < 5; i++)
        {
            await context.Attachments.AddAsync(new Attachment
            {
                SpaceId = 1,
                SnapshotId = 1,
                Name = $"Existing {i}",
                Path = $"path_{i}",
                IsDeleted = false,
                IsStored = true
            });
        }
        await context.SaveChangesAsync();

        var form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());
        var files = new FormFileCollection
        {
            CreateTestImage("test1.jpg"),
            CreateTestImage("test2.jpg")
        };
        var formWithFiles = new FormCollection(form.ToDictionary(), files);

        // Act
        var result = await controller.Add(formWithFiles, 1, 1);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Add_WhenUploadFails_ShouldReturnError()
    {
        // Arrange
        using var context = CreateContext("AddFailedUploadTest");
        var service = new AttachmentService(context, _cloudinaryService);
        var controller = new AttachmentController(service, _cloudinaryService);

        var form = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>());

        // Create a text file which should fail image validation
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("This is not an image"));
        var invalidFile = new FormFile(
            baseStream: stream,
            baseStreamOffset: 0,
            length: stream.Length,
            name: "file",
            fileName: "test.txt"
        )
        {
            Headers = new HeaderDictionary(),
            ContentType = "text/plain"
        };

        var files = new FormFileCollection { invalidFile };
        var formWithFiles = new FormCollection(form.ToDictionary(), files);

        // Act
        var result = await controller.Add(formWithFiles, 1);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task Add_WithNoFiles_ShouldReturnBadRequest()
    {
        // Arrange
        using var context = CreateContext("AddNoFilesTest");
        var service = new AttachmentService(context, _cloudinaryService);
        var controller = new AttachmentController(service, _cloudinaryService);

        var form = new FormCollection(
            new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>(),
            new FormFileCollection()
        );

        // Act
        var result = await controller.Add(form, spaceId: 1);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetAllBySpace_ShouldReturnAllAttachmentsBySpaceId()
    {
        // Arrange
        var dbName = "GetAllBySpaceControllerTest";
        var spaceId = 1;

        using (var context = CreateContext(dbName))
        {
            context.Attachments.AddRange(
                new Models.Attachment { Name = "Attachment 1", SpaceId = spaceId },
                new Models.Attachment { Name = "Attachment 2", SpaceId = spaceId },
                new Models.Attachment { Name = "Other Space", SpaceId = 2 }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new AttachmentService(context, _cloudinaryService);
            var controller = new AttachmentController(service, _cloudinaryService);

            // Act
            var result = await controller.GetAllBySpace(spaceId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<Models.Attachment>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Contains(returnValue, s => s.Name == "Attachment 1");
            Assert.Contains(returnValue, s => s.Name == "Attachment 2");
            Assert.DoesNotContain(returnValue, s => s.Name == "Other Space");
        }
    }

    [Fact]
    public async Task GetAllByFolder_ShouldReturnAllAttachmentsByFolderId()
    {
        // Arrange
        var dbName = "GetAllByFolderControllerTest";
        var folderId = 1;

        using (var context = CreateContext(dbName))
        {
            context.Attachments.AddRange(
                new Models.Attachment { Name = "Attachment 1", FolderId = folderId },
                new Models.Attachment { Name = "Attachment 2", FolderId = folderId },
                new Models.Attachment { Name = "Other Folder", FolderId = 2 }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new AttachmentService(context, _cloudinaryService);
            var controller = new AttachmentController(service, _cloudinaryService);

            // Act
            var result = await controller.GetAllByFolder(folderId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<Models.Attachment>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Contains(returnValue, s => s.Name == "Attachment 1");
            Assert.Contains(returnValue, s => s.Name == "Attachment 2");
            Assert.DoesNotContain(returnValue, s => s.Name == "Other Folder");
        }
    }

    [Fact]
    public async Task GetAllBySnapshot_ShouldReturnAllAttachmentsBySnapshotId()
    {
        // Arrange
        var dbName = "GetAllBySnapshotControllerTest";
        var snapshotId = 1;

        using (var context = CreateContext(dbName))
        {
            context.Attachments.AddRange(
                new Models.Attachment { Name = "Attachment 1", SnapshotId = snapshotId },
                new Models.Attachment { Name = "Attachment 2", SnapshotId = snapshotId },
                new Models.Attachment { Name = "Other Snapshot", SnapshotId = 2 }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new AttachmentService(context, _cloudinaryService);
            var controller = new AttachmentController(service, _cloudinaryService);

            // Act
            var result = await controller.GetAllBySnapshot(snapshotId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<Models.Attachment>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Contains(returnValue, s => s.Name == "Attachment 1");
            Assert.Contains(returnValue, s => s.Name == "Attachment 2");
            Assert.DoesNotContain(returnValue, s => s.Name == "Other Snapshot");
        }
    }

    [Fact]
    public async Task GetAllRootBySpace_ShouldReturnAllRootAttachments()
    {
        // Arrange
        var dbName = "GetAllRootBySpaceControllerTest";
        var spaceId = 1;

        using (var context = CreateContext(dbName))
        {
            context.Attachments.AddRange(
                new Models.Attachment { Name = "Root Attachment 1", SpaceId = spaceId, FolderId = null },
                new Models.Attachment { Name = "Root Attachment 2", SpaceId = spaceId, FolderId = null },
                new Models.Attachment { Name = "Folder Attachment", SpaceId = spaceId, FolderId = 1 }
            );
            await context.SaveChangesAsync();
        }

        using (var context = CreateContext(dbName))
        {
            var service = new AttachmentService(context, _cloudinaryService);
            var controller = new AttachmentController(service, _cloudinaryService);

            // Act
            var result = await controller.GetAllRootBySpace(spaceId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<Models.Attachment>>(actionResult.Value);
            Assert.Equal(2, returnValue.Count);
            Assert.Contains(returnValue, s => s.Name == "Root Attachment 1");
            Assert.Contains(returnValue, s => s.Name == "Root Attachment 2");
            Assert.DoesNotContain(returnValue, s => s.Name == "Folder Attachment");
        }
    }

    [Fact]
    public async Task GetSingle_WithValidId_ShouldReturnAttachment()
    {
        // Arrange
        var dbName = "GetSingleControllerTest";
        int attachmentId;

        using (var context = CreateContext(dbName))
        {
            var attachment = new Attachment
            {
                Name = "Test Attachment",
                Path = "/path/to/file",
                Size = 1024,
                MimeType = "application/pdf"
            };
            context.Attachments.Add(attachment);
            await context.SaveChangesAsync();
            attachmentId = attachment.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new AttachmentService(context, _cloudinaryService);
            var controller = new AttachmentController(service, _cloudinaryService);

            // Act
            var result = await controller.GetSingle(attachmentId);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Models.Attachment>(actionResult.Value);
            Assert.Equal("Test Attachment", returnValue.Name);
            Assert.Equal("/path/to/file", returnValue.Path);
            Assert.Equal(1024, returnValue.Size);
            Assert.Equal("application/pdf", returnValue.MimeType);
        }
    }

    [Fact]
    public async Task GetSingle_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        using var context = CreateContext("GetSingleInvalidControllerTest");
        var service = new AttachmentService(context, _cloudinaryService);
        var controller = new AttachmentController(service, _cloudinaryService);

        // Act
        var result = await controller.GetSingle(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task Update_WithValidId_ShouldReturnUpdatedAttachment()
    {
        // Arrange
        var dbName = "UpdateAttachmentValidControllerTest";
        int attachmentId;

        using (var context = CreateContext(dbName))
        {
            var attachment = new Attachment
            {
                Name = "Original Name",
                Path = "/original/path",
                Size = 1024,
                MimeType = "application/pdf"
            };
            context.Attachments.Add(attachment);
            await context.SaveChangesAsync();
            attachmentId = attachment.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new AttachmentService(context, _cloudinaryService);
            var controller = new AttachmentController(service, _cloudinaryService);
            var updatedAttachment = new AttachmentUpdateDTO
            {
                Name = "Updated Name",
                Path = "/updated/path",
                Size = 2048,
                MimeType = "image/jpeg"
            };

            // Act
            var result = await controller.Update(attachmentId, updatedAttachment);

            // Assert
            var actionResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<Models.Attachment>(actionResult.Value);
            Assert.Equal(updatedAttachment.Name, returnValue.Name);

            // Verify in database
            var savedAttachment = await context.Attachments.FindAsync(attachmentId);
            Assert.NotNull(savedAttachment);
            Assert.Equal(updatedAttachment.Name, savedAttachment.Name);
            Assert.Equal(updatedAttachment.Path, savedAttachment.Path);
            Assert.Equal(updatedAttachment.Size, savedAttachment.Size);
            Assert.Equal(updatedAttachment.MimeType, savedAttachment.MimeType);
        }
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnNotFound()
    {
        // Arrange
        using var context = CreateContext("UpdateAttachmentInvalidControllerTest");
        var service = new AttachmentService(context, _cloudinaryService);
        var controller = new AttachmentController(service, _cloudinaryService);
        var attachment = new AttachmentUpdateDTO
        {
            Name = "Test Attachment",
            Path = "/path/to/file"
        };

        // Act
        var result = await controller.Update(999, attachment);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
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
