using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using EasyContinuity_API.Helpers;

public class CloudinaryStorageService : ICloudinaryStorageService
{
   private readonly Cloudinary _cloudinary;

   public CloudinaryStorageService(IConfiguration configuration)
   {
       var account = new Account(
           configuration["CLOUDINARY_CLOUD_NAME"],
           configuration["CLOUDINARY_API_KEY"], 
           configuration["CLOUDINARY_API_SECRET"]);

       _cloudinary = new Cloudinary(account);
   }

   public async Task<Response<string>> UploadAsync(IFormFile file)
   {
       try
       {
           using var stream = file.OpenReadStream();
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
           return Response<bool>.Success(result != null);
       }
       catch (Exception ex)
       {
           return Response<bool>.InternalError(ex);
       }
   }

   public string GetFileUrl(string publicId)
   {
       return _cloudinary.Api.UrlImgUp.BuildUrl(publicId);
   }
}