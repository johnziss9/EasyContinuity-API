using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
using EasyContinuity_API.Services;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Tests.Attachments;

public class AttachmentServiceTests
{
    private ECDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ECDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ECDbContext(options);
    }

    [Fact]
    public async Task AddAttachment_ShouldAddAttachmentAndReturnSuccess()
    {
        // Arrange
        using var context = CreateContext("AddAttachmentServiceTest");
        var service = new AttachmentService(context);
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
        var service = new AttachmentService(context);
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
    public async Task AddAttachment_WithoutRequiredMetadata_ShouldReturnFailure()
    {
        // Arrange
        using var context = CreateContext("AddAttachmentNoMetadataTest");
        var service = new AttachmentService(context);
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
        var service = new AttachmentService(context);
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
            var service = new AttachmentService(context);

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
            var service = new AttachmentService(context);

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
            var service = new AttachmentService(context);

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
            var service = new AttachmentService(context);

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
            var service = new AttachmentService(context);

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
        var service = new AttachmentService(context);

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
                Path = "/original/path",
                Size = 1024,
                MimeType = "application/pdf",
                IsDeleted = false,
                AddedOn = dateAdded,
                AddedBy = 3
            };
            context.Attachments.Add(attachment);
            await context.SaveChangesAsync();
            attachmentId = attachment.Id;
        }

        using (var context = CreateContext(dbName))
        {
            var service = new AttachmentService(context);
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
        var service = new AttachmentService(context);
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
            var service = new AttachmentService(context);
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
}