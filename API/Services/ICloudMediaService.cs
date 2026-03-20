using System;
using API.Entities;
using CloudinaryDotNet.Actions;

namespace API.Services;

public interface ICloudMediaService
{
    Task<ImageUploadResult> AddImageAsync(IFormFile file);
    Task<DeletionResult> DeleteImageAsync(string publicId);
    Task<VideoUploadResult> AddVideoAsync(IFormFile file);
    Task<DeletionResult> DeleteVideoAsync(string publicId);
    Task<RawUploadResult> AddRawDataAsync(IFormFile file);
    Task<DeletionResult> DeleteRawDataAsync(string publicId);
}
