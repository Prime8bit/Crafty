using API.Misc;
using API.Pagination;
using CraftyCommon.DTOs;

namespace API.Data;

public interface ICraftWishlistManager
{
    Task<ManagerResponse> ToggleWishlistItemAsync(WishlistItemDto craftWishlistDto);
    Task<ManagerResponse<WishlistItemDto>> GetCraftWishlistItemAsync(long userId, long craftId);
    Task<ManagerResponse<PagedList<CraftDto>>> GetWishlistedCraftsForUserAsync(long userId, CraftListParams craftListParams);
    Task<ManagerResponse<List<long>>> GetWishlistedCraftIdsForUserAsync(long userId);
    Task<ManagerResponse<int>> GetNumLikesForCraftAsync(long craftId);
}