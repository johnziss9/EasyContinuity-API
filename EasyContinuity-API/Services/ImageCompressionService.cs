using EasyContinuity_API.Helpers;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

public class ImageCompressionService : IImageCompressionService
{
    private readonly Dictionary<string, string> _supportedFormats = new()
    {
        {"image/jpeg", ".jpg"},
        {"image/jpg", ".jpg"},
        {"image/png", ".png"}
    };

    public async Task<Response<byte[]>> CompressImageAsync(IFormFile file, int maxWidth = 1200, int quality = 80)
    {
        try
        {
            // Validate image format
            if (!IsValidImageFormat(file.ContentType))
            {
                return Response<byte[]>.BadRequest("Only JPEG and PNG formats are supported");
            }

            // Load image from stream
            using var image = await Image.LoadAsync(file.OpenReadStream());
            
            // Calculate scaling factor to maintain aspect ratio
            var scaleFactor = (double)maxWidth / image.Width;
            if (scaleFactor > 1) scaleFactor = 1; // Prevent upscaling
            
            var newWidth = (int)(image.Width * scaleFactor);
            var newHeight = (int)(image.Height * scaleFactor);

            // Resize image
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(newWidth, newHeight),
                Mode = ResizeMode.Max
            }));
            
            // Prepare for saving
            using var ms = new MemoryStream();
            
            // Save with format-specific settings
            switch (file.ContentType.ToLower())
            {
                case "image/png":
                    await image.SaveAsPngAsync(ms, new PngEncoder 
                    { 
                        CompressionLevel = PngCompressionLevel.BestCompression
                    });
                    break;
                case "image/jpeg":
                case "image/jpg":
                    await image.SaveAsJpegAsync(ms, new JpegEncoder 
                    { 
                        Quality = quality // Adjustable quality
                    });
                    break;
            }
            
            return Response<byte[]>.Success(ms.ToArray());
        }
        catch (Exception ex)
        {
            return Response<byte[]>.InternalError(ex);
        }
    }

    public bool IsValidImageFormat(string contentType)
    {
        return _supportedFormats.ContainsKey(contentType.ToLower());
    }
}