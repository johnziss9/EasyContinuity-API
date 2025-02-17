using EasyContinuity_API.Helpers;

public interface ICloudinaryStorageService
{
   Task<Response<string>> UploadAsync(IFormFile file, string? folder = null);
   Task<Response<bool>> DeleteAsync(string publicId);
   Task<Response<bool>> ExistsAsync(string publicId);
   string GetFileUrl(string publicId);
}