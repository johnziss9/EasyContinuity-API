using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

public class ImageCompressionServiceTests
{
    private readonly IImageCompressionService _service;

    public ImageCompressionServiceTests()
    {
        _service = new ImageCompressionService();
    }

    private IFormFile CreateTestImage(int width, int height, string contentType = "image/jpeg")
    {
        using var image = new Image<Rgba32>(width, height);
        var stream = new MemoryStream();

        if (contentType == "image/png")
            image.SaveAsPng(stream);
        else
            image.SaveAsJpeg(stream);

        stream.Position = 0;

        return new FormFile(stream, 0, stream.Length, "test",
            contentType == "image/png" ? "test.png" : "test.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    [Fact]
    public async Task CompressImageAsync_WithLargeJpeg_ShouldReduceSize()
    {
        // Arrange
        var file = CreateTestImage(2000, 2000);
        var originalSize = file.Length;

        // Act
        var result = await _service.CompressImageAsync(file);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Length < originalSize);
    }

    [Fact]
    public async Task CompressImageAsync_WithSmallImage_ShouldNotUpscale()
    {
        // Arrange
        var file = CreateTestImage(800, 800);
        var originalSize = file.Length;

        // Act
        var result = await _service.CompressImageAsync(file);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);
        // Size might be slightly different due to compression but shouldn't be dramatically larger
        Assert.True(result.Data.Length <= originalSize * 1.1);
    }

    [Fact]
    public async Task CompressImageAsync_WithPng_ShouldMaintainFormat()
    {
        // Arrange
        var file = CreateTestImage(1000, 1000, "image/png");

        // Act
        var result = await _service.CompressImageAsync(file);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Data);

        // Verify it's still a PNG by checking the header bytes
        Assert.Equal(0x89, result.Data[0]);
        Assert.Equal(0x50, result.Data[1]); // 'P'
        Assert.Equal(0x4E, result.Data[2]); // 'N'
        Assert.Equal(0x47, result.Data[3]); // 'G'
    }

    [Fact]
    public async Task CompressImageAsync_WithInvalidFormat_ShouldReturnError()
    {
        // Arrange
        var file = CreateTestImage(100, 100, "image/gif");

        // Act
        var result = await _service.CompressImageAsync(file);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(400, result.StatusCode);
    }

    [Theory]
    [InlineData("image/jpeg", true)]
    [InlineData("image/jpg", true)]
    [InlineData("image/png", true)]
    [InlineData("image/gif", false)]
    [InlineData("image/bmp", false)]
    [InlineData("application/pdf", false)]
    public void IsValidImageFormat_ShouldValidateCorrectly(string contentType, bool expected)
    {
        // Act
        var result = _service.IsValidImageFormat(contentType);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task CompressImageAsync_QualityParameter_ShouldAffectFileSize()
    {
        // Arrange
        using var image = new Image<Rgba32>(1000, 1000, Color.Red);
        var stream = new MemoryStream();
        await image.SaveAsJpegAsync(stream);
        stream.Position = 0;

        var file = new FormFile(stream, 0, stream.Length, "test", "test.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        // Act
        var highQualityResult = await _service.CompressImageAsync(file, quality: 100);
        var lowQualityResult = await _service.CompressImageAsync(file, quality: 30);

        // Assert
        Assert.True(highQualityResult.IsSuccess);
        Assert.True(lowQualityResult.IsSuccess);
        Assert.NotNull(highQualityResult.Data);
        Assert.NotNull(lowQualityResult.Data);
        Assert.True(highQualityResult.Data.Length > lowQualityResult.Data.Length,
            $"High quality size: {highQualityResult.Data.Length}, Low quality size: {lowQualityResult.Data.Length}");
    }
}