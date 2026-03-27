namespace API.Entities;

public class WishlistItem
{
    // Navigation properties
    public long WishlistingUserId { get; set; }
    public User? WishlistingUser { get; set; }
    public long WishlistedCraftId { get; set; }
    public Craft? WishlistedCraft { get; set; }
}