using API.Entities;

namespace API.DTOs;

public class CraftWishlistItemDto
{
    public UserDto WishlistingUser { get; set; } = null!;
    public long WishlistingUserId { get; set; }
    public CraftDto WishListedCraft { get; set; } = null!;
    public long WishListedCraftId { get; set; }

    public CraftWishlistItemDto() {}

    public CraftWishlistItemDto (WishlistItem wishlistItem)
    {
        WishListedCraft = new CraftDto(wishlistItem.WishListedCraft);
        WishListedCraftId = wishlistItem.WishListedCraftId;
        WishlistingUser = new UserDto(wishlistItem.WishlistingUser);
        WishlistingUserId = wishlistItem.WishlistingUserId;
    }
}