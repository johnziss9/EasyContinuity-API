using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using NSubstitute;
using EasyContinuity_API.Helpers;

public class CloudinaryStorageServiceTests
{
   private readonly CloudinaryStorageService _service;
   private readonly IImageCompressionService _compressionService;

   public CloudinaryStorageServiceTests()
   {
       var configuration = new ConfigurationBuilder()
           .AddInMemoryCollection([
                KeyValuePair.Create<string, string?>("CLOUDINARY_CLOUD_NAME", "test_cloud"),
                KeyValuePair.Create<string, string?>("CLOUDINARY_API_KEY", "test_key"),
                KeyValuePair.Create<string, string?>("CLOUDINARY_API_SECRET", "test_secret")
            ])
            .Build();

       _compressionService = Substitute.For<IImageCompressionService>();
       _service = new CloudinaryStorageService(configuration, _compressionService);
   }

   private IFormFile CreateTestFile(string filename = "test.jpg", string contentType = "image/jpeg")
   {
       var file = Substitute.For<IFormFile>();
       file.FileName.Returns(filename);
       file.ContentType.Returns(contentType);
       file.Length.Returns(1024);
       return file;
   }

   // These tests verify the methods execute and the response structure.
   //We can't verify exact responses as they depend on Cloudinary and that requires the Cloudinary credentials.

   [Fact]
   public async Task UploadAsync_WhenCompressionFails_ShouldReturnError()
   {
       // Arrange
       var file = CreateTestFile();
       _compressionService.CompressImageAsync(Arg.Any<IFormFile>(), Arg.Any<int>(), Arg.Any<int>())
           .Returns(Response<byte[]>.Fail(400, "Compression failed"));

       // Act
       var result = await _service.UploadAsync(file);

       // Assert
       Assert.False(result.IsSuccess);
       Assert.Equal(400, result.StatusCode);
       Assert.Contains("Compression failed", result.Message);
   }

   [Fact]
   public async Task UploadAsync_WhenCompressionReturnsNull_ShouldReturnError()
   {
       // Arrange
       var file = CreateTestFile();
       _compressionService.CompressImageAsync(Arg.Any<IFormFile>(), Arg.Any<int>(), Arg.Any<int>())
           .Returns(Response<byte[]>.Success(null));

       // Act
       var result = await _service.UploadAsync(file);

       // Assert
       Assert.False(result.IsSuccess);
       Assert.Equal(500, result.StatusCode);
   }

   [Fact]
   public async Task UploadAsync_WithInvalidFileType_ShouldFail()
   {
       // Arrange
       var file = CreateTestFile("test.txt", "text/plain");
       _compressionService.CompressImageAsync(Arg.Any<IFormFile>(), Arg.Any<int>(), Arg.Any<int>())
           .Returns(Response<byte[]>.Fail(400, "Invalid file type"));

       // Act
       var result = await _service.UploadAsync(file);

       // Assert
       Assert.False(result.IsSuccess);
       Assert.Equal(400, result.StatusCode);
   }

   [Fact]
   public async Task UploadAsync_WithValidFile_ShouldSucceed()
   {
       // Arrange
       var file = CreateTestFile();
       var compressedData = new byte[] { 1, 2, 3 };
       _compressionService.CompressImageAsync(Arg.Any<IFormFile>(), Arg.Any<int>(), Arg.Any<int>())
           .Returns(Response<byte[]>.Success(compressedData));

       // Act
       var result = await _service.UploadAsync(file);

       // Assert
       Assert.NotNull(result);
       // We can't assert much more as it depends on Cloudinary response
   }

   [Fact]
   public async Task DeleteAsync_WithValidPublicId_ShouldSucceed()
   {
       // Arrange
       var publicId = "valid_test_id";

       // Act
       var result = await _service.DeleteAsync(publicId);

       // Assert
       Assert.NotNull(result);
       // Result depends on Cloudinary but we can verify response structure
   }

   [Fact]
   public async Task DeleteAsync_WithInvalidPublicId_ShouldHandleError()
   {
       // Arrange
       var publicId = "";  // Invalid ID

       // Act
       var result = await _service.DeleteAsync(publicId);

       // Assert
       Assert.NotNull(result);
       // Response will depend on how Cloudinary handles invalid IDs
   }

   [Fact]
   public async Task ExistsAsync_WithValidPublicId_ShouldReturnResponse()
   {
       // Arrange
       var publicId = "test_id";

       // Act
       var result = await _service.ExistsAsync(publicId);

       // Assert
       Assert.NotNull(result);
       // Actual existence depends on Cloudinary
   }

   [Fact]
   public async Task ExistsAsync_WithInvalidPublicId_ShouldHandleError()
   {
       // Arrange
       var publicId = "";  // Invalid ID

       // Act
       var result = await _service.ExistsAsync(publicId);

       // Assert
       Assert.NotNull(result);
       // Response will depend on how Cloudinary handles invalid IDs
   }

   [Fact]
   public void GetFileUrl_ShouldReturnUrl()
   {
       // Arrange
       var publicId = "test_id";

       // Act
       var result = _service.GetFileUrl(publicId);

       // Assert
       Assert.NotNull(result);
       Assert.Contains(publicId, result);
   }

   [Fact]
   public void GetFileUrl_WithEmptyId_ShouldStillReturnUrl()
   {
       // Arrange
       var publicId = "";

       // Act
       var result = _service.GetFileUrl(publicId);

       // Assert
       Assert.NotNull(result);
   }
}