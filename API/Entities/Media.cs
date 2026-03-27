using CraftyCommon.DTOs;
using System;

namespace API.Entities;


/// <summary>
/// This class represents either a photo or a video that is stored in the cloud.
/// </summary>
public class Media
{
    public long Id { get; set; }     
    public required string Url { get; set; }
    public string? CloudId { get; set; }
    public required MediaType Type { get; set; } = MediaType.Image;
}


public class CraftMedia : Media
{
    public long CraftId { get; set; }
    public Craft? Craft { get; set; }
}

public class UserMedia : Media
{
    public long UserId { get; set; }
    public User? User { get; set; }
}