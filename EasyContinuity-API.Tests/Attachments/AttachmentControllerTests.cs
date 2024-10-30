using EasyContinuity_API.Controllers;
using EasyContinuity_API.Data;
using EasyContinuity_API.DTOs;
using EasyContinuity_API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EasyContinuity_API.Tests.Attachments;

public class AttachmentControllerTests
{
    private ECDbContext CreateContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<ECDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new ECDbContext(options);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedAttachment()
    {
        // Arrange

        // Creates a new in-memory database with a unique name "CreateControllerTest"
        using var context = CreateContext("CreateControllerTest");
        var service = new AttachmentService(context);
        var controller = new AttachmentController(service);
        var attachment = new Models.Attachment 
        { 
            Name = "Test Attachment",
            Path = "/path/to/file",
            Size = 1024,
            MimeType = "application/pdf"
        };

        // Act
        var result = await controller.Create(attachment);

        // Assert

        // Verify we got an OkObjectResult (HTTP 200 OK)
        var actionResult = Assert.IsType<OkObjectResult>(result.Result);
        // Verify the returned object is a Attachment
        var returnValue = Assert.IsType<Models.Attachment>(actionResult.Value);
        Assert.Equal(attachment.Name, returnValue.Name);
        
        var savedAttachment = await context.Attachments.FindAsync(returnValue.Id);
        Assert.NotNull(savedAttachment);
        Assert.Equal(attachment.Name, savedAttachment.Name);
        Assert.Equal(attachment.Path, savedAttachment.Path);
        Assert.Equal(attachment.Size, savedAttachment.Size);
        Assert.Equal(attachment.MimeType, savedAttachment.MimeType);
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
            var service = new AttachmentService(context);
            var controller = new AttachmentController(service);

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
            var service = new AttachmentService(context);
            var controller = new AttachmentController(service);

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
            var service = new AttachmentService(context);
            var controller = new AttachmentController(service);

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
            var service = new AttachmentService(context);
            var controller = new AttachmentController(service);

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
            var attachment = new Models.Attachment 
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
            var service = new AttachmentService(context);
            var controller = new AttachmentController(service);

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
        var service = new AttachmentService(context);
        var controller = new AttachmentController(service);

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
            var attachment = new Models.Attachment 
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
            var service = new AttachmentService(context);
            var controller = new AttachmentController(service);
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
        var service = new AttachmentService(context);
        var controller = new AttachmentController(service);
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
}
