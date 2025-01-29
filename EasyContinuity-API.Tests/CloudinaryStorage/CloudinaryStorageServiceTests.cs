using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public class CloudinaryStorageServiceTests : IDisposable
{
    private readonly CloudinaryStorageService _service;
    private readonly IConfiguration _configuration;
    private readonly IImageCompressionService _compressionService;
    private readonly List<string> _uploadedPublicIds = new();

    public CloudinaryStorageServiceTests()
    {
        // Get the solution directory path
        var currentDir = Directory.GetCurrentDirectory();
        var solutionDir = Directory.GetParent(currentDir)?.Parent?.Parent?.Parent?.FullName;
        var envPath = Path.Combine(solutionDir!, ".env");

        if (!File.Exists(envPath))
        {
            throw new FileNotFoundException($".env file not found at {envPath}");
        }

        var envFile = File.ReadAllLines(envPath);
        var cloudName = envFile.FirstOrDefault(l => l.StartsWith("CLOUDINARY_CLOUD_NAME="))?.Split('=')[1];
        var apiKey = envFile.FirstOrDefault(l => l.StartsWith("CLOUDINARY_API_KEY="))?.Split('=')[1];
        var apiSecret = envFile.FirstOrDefault(l => l.StartsWith("CLOUDINARY_API_SECRET="))?.Split('=')[1];

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
        _service = new CloudinaryStorageService(_configuration, _compressionService);
    }

    private static bool ShouldSkipCloudinaryTests()
    {
        return Environment.GetEnvironmentVariable("CI") != null;
    }

    private IFormFile CreateTestFile(string filename = "test.jpg", string contentType = "image/jpeg")
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
    public async Task UploadAsync_WithValidJpeg_ShouldSucceed()
    {
        if (ShouldSkipCloudinaryTests())
        {
            Skip.If(true, "Skipping Cloudinary test in CI environment");
        }

        // Arrange
        var file = CreateTestFile("test.jpg", "image/jpeg");

        // Act
        var result = await _service.UploadAsync(file);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        _uploadedPublicIds.Add(result.Data!);
    }

    [Fact]
    public async Task UploadAsync_WithValidPng_ShouldSucceed()
    {
        if (ShouldSkipCloudinaryTests())
        {
            Skip.If(true, "Skipping Cloudinary test in CI environment");
        }

        // Arrange
        var file = CreateTestFile("test.png", "image/png");

        // Act
        var result = await _service.UploadAsync(file);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        _uploadedPublicIds.Add(result.Data!);
    }

    [Fact]
    public async Task UploadAsync_WithInvalidFileType_ShouldFail()
    {
        if (ShouldSkipCloudinaryTests())
        {
            Skip.If(true, "Skipping Cloudinary test in CI environment");
        }

        // Arrange
        var file = CreateTestFile("test.txt", "text/plain");

        // Act
        var result = await _service.UploadAsync(file);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task DeleteAsync_WithValidPublicId_ShouldSucceed()
    {
        if (ShouldSkipCloudinaryTests())
        {
            Skip.If(true, "Skipping Cloudinary test in CI environment");
        }

        // Arrange
        var file = CreateTestFile();
        var uploadResult = await _service.UploadAsync(file);
        Assert.True(uploadResult.IsSuccess);
        Assert.NotNull(uploadResult.Data);

        // Act
        var result = await _service.DeleteAsync(uploadResult.Data!);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task DeleteAsync_WithInvalidPublicId_ShouldFail()
    {
        if (ShouldSkipCloudinaryTests())
        {
            Skip.If(true, "Skipping Cloudinary test in CI environment");
        }

        // Arrange
        var invalidId = "definitely_invalid_id_" + Guid.NewGuid();

        // Act
        var result = await _service.DeleteAsync(invalidId);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task ExistsAsync_WithValidPublicId_ShouldReturnTrue()
    {
        if (ShouldSkipCloudinaryTests())
        {
            Skip.If(true, "Skipping Cloudinary test in CI environment");
        }

        // Arrange
        var file = CreateTestFile();
        var uploadResult = await _service.UploadAsync(file);
        Assert.True(uploadResult.IsSuccess);
        Assert.NotNull(uploadResult.Data);
        _uploadedPublicIds.Add(uploadResult.Data!);

        // Act
        var result = await _service.ExistsAsync(uploadResult.Data!);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task ExistsAsync_WithInvalidPublicId_ShouldReturnFalse()
    {
        if (ShouldSkipCloudinaryTests())
        {
            Skip.If(true, "Skipping Cloudinary test in CI environment");
        }

        // Arrange
        var invalidId = "definitely_invalid_id_" + Guid.NewGuid();

        // Act
        var result = await _service.ExistsAsync(invalidId);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Data);
    }

    [Fact]
    public void GetFileUrl_ShouldReturnUrl()
    {
        if (ShouldSkipCloudinaryTests())
        {
            Skip.If(true, "Skipping Cloudinary test in CI environment");
        }
        
        // Arrange
        var publicId = "test_id";

        // Act
        var result = _service.GetFileUrl(publicId);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(publicId, result);
    }

    public void Dispose()
    {
        foreach (var publicId in _uploadedPublicIds)
        {
            try
            {
                _service.DeleteAsync(publicId).Wait();
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}