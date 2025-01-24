using EasyContinuity_API.Helpers;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class CloudinaryController : ControllerBase
{
    private readonly ICloudinaryStorageService _cloudinaryService;

    public CloudinaryController(ICloudinaryStorageService cloudinaryService)
    {
        _cloudinaryService = cloudinaryService;
    }

    [HttpPost("test-upload")]
    public async Task<ActionResult<Response<string>>> TestUpload(IFormFile file)
    {
        var result = await _cloudinaryService.UploadAsync(file);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }
}