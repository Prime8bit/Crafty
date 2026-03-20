using API.DTOs;
using API.Entities;
using API.Misc;
using API.Pagination;

namespace API.Data;

public interface ICraftWishlistManager
{
    Task<ManagerResponse> ToggleWishlistItemAsync(CraftWishlistItemDto craftWishlistDto);
    Task<ManagerResponse<CraftWishlistItemDto>> GetCraftWishlistItemAsync(long userId, long craftId);
    Task<ManagerResponse<PagedList<CraftDto>>> GetWishlistedCraftsForUserAsync(long userId, CraftListParams craftListParams);
    Task<ManagerResponse<List<long>>> GetWishlistedCraftIdsForUserAsync(long userId);
    Task<ManagerResponse<int>> GetNumLikesForCraftAsync(long craftId);
}