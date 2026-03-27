using API.Services;
using CloudinaryDotNet.Actions;
using CraftyCommon.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class MediasController (ICloudMediaService cloudMediaService) : BaseApiController
{
    readonly string[] videoExtensions = { ".mp4", ".webv", ".ogg" };
    readonly string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    readonly string[] model3dExtensions = { ".glb"};

    [HttpPost]
    public async Task<ActionResult<MediaDto>> UploadMedia([FromForm] IFormFile file)
    {
        MediaType type = MediaType.None;
        foreach (var extension in videoExtensions)
        {
            if (file.FileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
            {
                type = MediaType.Video;
                break;
            }
        }

        if (type == MediaType.None)
        {
            foreach (var extension in model3dExtensions)
            {
                if (file.FileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    type = MediaType.Model3d;
                    break;
                }
            }
        }

        if (type == MediaType.None)
        {            
            foreach (var extension in imageExtensions)
            {
                if (file.FileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                {
                    type = MediaType.Image;
                    break;
                }
            }
        }

        RawUploadResult? result = null;
        switch (type)
        {
            case MediaType.Image:
                result = await cloudMediaService.AddImageAsync(file);
                break;
            case MediaType.Video:
                result = await cloudMediaService.AddVideoAsync(file);
                break;
            case MediaType.Model3d:
                result = await cloudMediaService.AddRawDataAsync(file);
                break;
            default:
                return BadRequest("File type not supported.");
        }

        if (result == null || result.Error != null)
        {
            return BadRequest(result?.Error.Message);
        }

        var newMediaItem = new MediaDto
        {
            Url = result.SecureUrl.AbsoluteUri,
            CloudId = result.PublicId,
            Type = type        
        };

        return Ok(newMediaItem);
    }

    [HttpDelete("images/{cloudId}")]
    public async Task<ActionResult> DeleteImage(string cloudId)
    {
        cloudId = Uri.UnescapeDataString(cloudId);
        DeletionResult? result = await cloudMediaService.DeleteImageAsync(cloudId);

        if (result == null || result.Error != null)
        {
            return BadRequest(result?.Error.Message);
        }

        if (result.Result.Equals("not found"))
        {
            return NotFound($"Image with id {cloudId} not found.");
        }

        return NoContent();
    }

    [HttpDelete("videos/{cloudId}")]
    public async Task<ActionResult> DeleteVideo(string cloudId)
    {
        cloudId = Uri.UnescapeDataString(cloudId);
        DeletionResult? result = await cloudMediaService.DeleteVideoAsync(cloudId);

        if (result == null || result.Error != null)
        {
            return BadRequest(result?.Error.Message);
        }

        if (result.Result.Equals("not found"))
        {
            return NotFound($"Video with id {cloudId} not found.");
        }

        return NoContent();
    }

    // Cloudinary only has three types of data, Images, Videos, and RawData
    // Every delete function for another of my data types that doesn't represent
    // images or data should be mapped to rawdata
    [HttpDelete("model3d/{cloudId}")]
    public async Task<ActionResult> DeleteRawData(string cloudId)
    {
        cloudId = Uri.UnescapeDataString(cloudId);
        DeletionResult? result = await cloudMediaService.DeleteRawDataAsync(cloudId);

        if (result == null || result.Error != null)
        {
            return BadRequest(result?.Error.Message);
        }

        if (result.Result.Equals("not found"))
        {
            return NotFound($"Raw Data with id {cloudId} not found.");
        }

        return NoContent();
    }
}

