using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

public class Craft
{
    // If a public property is named "Id" it is implicitly chosen as the primary key
    // If you want to change the name of this property then uncomment the line below
    // [Key]
    public long Id { get; set; }
    public required string Name { get; set; }
    public required float Price { get; set; }
    public string? Description { get; set; }
    public required uint Stock { get; set; }
    public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);
    public bool IsArchived { get; set; } = false;
    public bool IsInappropriate { get; set; } = false;

    // Navigation Properties
    public long SellerId { get; set; }
    public User? Seller { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    // For 1->0...1 relationships, the foreign key should be nullable and set to null by default.
    public long? SearchImageId { get; set; }
    public CraftMedia? SearchImage { get; set; }
    public ICollection<CraftMedia> Medias { get; set; } = new List<CraftMedia>();
    public ICollection<User> WishListingUsers { get; set; } = new List<User>();
}