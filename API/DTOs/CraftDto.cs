using API.Entities;

namespace API.DTOs;

public class CraftDto
{
    public long Id { get; set; }
    public string? SellerUserName { get; set; }
    public string? SellerDisplayName { get; set; }
    public string? Name { get; set; }
    public float Price { get; set; }
    public string? Description { get; set; }
    public uint Stock { get; set; }
    public string? CreatedAt { get; set; }
    public long? SearchImageId { get; set; } = null;
    public CraftMediaDto? SearchImage { get; set;} = null;
    public ICollection<CraftMediaDto> Medias { get; set; } = new List<CraftMediaDto>();
    public bool IsArchived { get; set; } = false;

    // An explicit parameterless constructor is needed for deserialization
    public CraftDto() { }
    
    public CraftDto(Craft craft)
    {
        Id = craft.Id;
        SellerUserName = craft.Seller?.UserName ?? "";
        SellerDisplayName = craft.Seller?.DisplayName;
        Name = craft.Name;
        Price = craft.Price;
        Description = craft.Description;
        Stock = craft.Stock;
        CreatedAt = craft.CreatedAt.ToString("o"); // ISO 8601 format
        SearchImageId = craft.SearchImage?.Id;
        if (craft.SearchImage != null)
        {
            SearchImage = new CraftMediaDto(craft.SearchImage)
            {
                CraftName = craft.Name
            };
        }

        foreach (var mediaItem in craft.Medias)
        {
            var craftMediaItemDto = new CraftMediaDto(mediaItem)
            {
                CraftName = craft.Name
            };
            Medias.Add(craftMediaItemDto);
        }

        IsArchived = craft.IsArchived;
    }
}