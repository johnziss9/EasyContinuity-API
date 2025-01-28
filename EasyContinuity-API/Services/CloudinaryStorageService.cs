using System.Net;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EasyContinuity_API.Helpers;

public class CloudinaryStorageService : ICloudinaryStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly IImageCompressionService _compressionService;

    public CloudinaryStorageService(IConfiguration configuration, IImageCompressionService compressionService)
    {
        var account = new Account(
            configuration["CLOUDINARY_CLOUD_NAME"],
            configuration["CLOUDINARY_API_KEY"],
            configuration["CLOUDINARY_API_SECRET"]);

        _cloudinary = new Cloudinary(account);
        _compressionService = compressionService;
    }

    public async Task<Response<string>> UploadAsync(IFormFile file)
    {
        try
        {
            // Compress image before uploading
            var compressionResult = await _compressionService.CompressImageAsync(file);
            if (!compressionResult.IsSuccess)
            {
                return Response<string>.Fail(compressionResult.StatusCode, compressionResult.Message ?? "Compression failed");
            }

            if (compressionResult.Data == null)
            {
                return Response<string>.Fail(500, "Compression resulted in null data");
            }

            // Upload compressed image
            using var stream = new MemoryStream(compressionResult.Data);
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                UniqueFilename = true
            };

            var result = await _cloudinary.UploadAsync(uploadParams);
            return Response<string>.Success(result.PublicId);
        }
        catch (Exception ex)
        {
            return Response<string>.InternalError(ex);
        }
    }

    public async Task<Response<bool>> DeleteAsync(string publicId)
    {
        try
        {
            var result = await _cloudinary.DeleteResourcesAsync(publicId);

            // Check the Deleted dictionary for "not_found"
            if (result.Deleted != null &&
                result.Deleted.ContainsKey(publicId) &&
                result.Deleted[publicId] == "not_found")
            {
                return Response<bool>.Fail(404, "Resource not found");
            }

            return Response<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return Response<bool>.InternalError(ex);
        }
    }


    public async Task<Response<bool>> ExistsAsync(string publicId)
    {
        try
        {
            var result = await _cloudinary.GetResourceAsync(publicId);

            // Check if there's an error in the response
            if (result.Error != null || result.StatusCode == HttpStatusCode.NotFound)
            {
                return Response<bool>.Success(false);
            }

            return Response<bool>.Success(result.PublicId != null);
        }
        catch (Exception)
        {
            return Response<bool>.Success(false);
        }
    }

    public string GetFileUrl(string publicId)
    {
        return _cloudinary.Api.UrlImgUp.BuildUrl(publicId);
    }
}