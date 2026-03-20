using System;

namespace API.Entities;

public enum MediaType
{
    // I add explicit numeric values so even if I remove or add items in this enum,
    // The database will still continue to work as expected.
    None = 0,
    Image = 1,
    Video = 2,
    Model3d = 3
}


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
    public Craft Craft { get; set; } = null!;
}

public class UserMedia : Media
{
    public long UserId { get; set; }
    public User User { get; set; } = null!;
}