using System;
using API.Entities;

namespace API.DTOs;

public class MediaDto
{
    public long Id { get; set; }
    public string? Url { get; set; }
    public string? CloudId { get; set; }
    public MediaType Type { get; set; } = MediaType.Image;
    
    // An explicit parameterless constructor is needed for deserialization
    public MediaDto() {}
    
    public MediaDto(Media media)
    {
        Id = media.Id;
        Url = media.Url;
        CloudId = media.CloudId;
        Type = media.Type;
    }
}

public class CraftMediaDto : MediaDto
{
    public long CraftId { get; set; }
    public string CraftName { get; set; } = null!;

    public CraftMediaDto() : base() {}
    
    public CraftMediaDto(CraftMedia media) : base(media)
    {
        CraftId = media.CraftId;
        CraftName = media.Craft?.Name ?? "";
    }
}

public class UserMediaDto : MediaDto
{
    public long UserId { get; set; }
    public string? UserUserName { get; set; } = null!;

    public UserMediaDto() : base() {}
    
    public UserMediaDto(UserMedia media) : base(media)
    {
        UserId = media.UserId;
        UserUserName = media?.User?.UserName ?? "";
    }
}