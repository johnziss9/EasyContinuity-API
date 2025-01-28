using EasyContinuity_API.Helpers;

public interface IImageCompressionService
{
    Task<Response<byte[]>> CompressImageAsync(IFormFile file, int maxWidth = 1200, int quality = 80);
    
    bool IsValidImageFormat(string contentType);
}