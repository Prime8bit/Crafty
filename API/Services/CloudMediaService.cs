using System;
using API.Entities;
using API.Misc;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;

namespace API.Services;

public class CloudMediaService : ICloudMediaService
{
    const int MAX_PHOTO_DIMENSION = 400;
    const string CRAFTY_FOLDER_NAME = "Crafty";

    private readonly Cloudinary _cloudinary;
    

    public CloudMediaService(IOptions<CloudinarySettings> cloudinaryConfig)
    {
        var account = new Account(
            cloudinaryConfig.Value.CloudName,
            cloudinaryConfig.Value.ApiKey,
            cloudinaryConfig.Value.ApiSecret
        );
        _cloudinary = new Cloudinary(account);
    }

    public async Task<ImageUploadResult> AddImageAsync(IFormFile file)
    {
        var uploadResult = new ImageUploadResult();
        if (file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Transformation = new Transformation().Height(MAX_PHOTO_DIMENSION).Width(MAX_PHOTO_DIMENSION).Crop("fill").Gravity("face"),
                Folder = CRAFTY_FOLDER_NAME
            };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        return uploadResult;
    }

    public async Task<DeletionResult> DeleteImageAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Image
        };
        return await _cloudinary.DestroyAsync(deleteParams);
    }

    public async Task<VideoUploadResult> AddVideoAsync(IFormFile file)
    {
        var uploadResult = new VideoUploadResult();
        if (file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new VideoUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = CRAFTY_FOLDER_NAME
            };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        return uploadResult;
    }

    public async Task<DeletionResult> DeleteVideoAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Video
        };
        return await _cloudinary.DestroyAsync(deleteParams);
    }
    

    public async Task<RawUploadResult> AddRawDataAsync(IFormFile file)
    {
        var uploadResult = new RawUploadResult();
        if (file.Length > 0)
        {
            using var stream = file.OpenReadStream();
            var uploadParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = CRAFTY_FOLDER_NAME
            };
            uploadResult = await _cloudinary.UploadAsync(uploadParams);
        }

        return uploadResult;
    }    

    public async Task<DeletionResult> DeleteRawDataAsync(string publicId)
    {
        var deleteParams = new DeletionParams(publicId)
        {
            ResourceType = ResourceType.Raw
        };
        return await _cloudinary.DestroyAsync(deleteParams);
    }
}
