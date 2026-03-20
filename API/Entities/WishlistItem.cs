namespace API.Entities;

public class WishlistItem
{
    public User WishlistingUser { get; set; } = null!;
    public long WishlistingUserId { get; set; }
    public Craft WishListedCraft { get; set; } = null!;
    public long WishListedCraftId { get; set; }
}