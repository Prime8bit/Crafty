using System.Security.Claims;
using API.Data;
using API.Extensions;
using API.Pagination;
using CraftyCommon.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class CraftWishlistsController (
    ICraftWishlistManager craftWishlistRepo
    ) : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<PagedList<CraftDto>>> GetWishList([FromQuery]CraftListParams craftListParams)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId = 0;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to view your wishlist.");
        }

        var response = await craftWishlistRepo.GetWishlistedCraftsForUserAsync(userId, craftListParams);

        if (response.Data != null)
        {
            Response.AddPaginationHeader(response.Data);
        }

        return GetActionResult(response);
    }

    [HttpPost("{targetCraftId:long}")]
    public async Task<ActionResult> ToggleWishlist (long targetCraftId)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to edit a craft.");
        }
        
        var craftWishlistItemDto = new WishlistItemDto() { WishlistedCraftId = targetCraftId, WishlistingUserId = userId };

        return GetActionResult(await craftWishlistRepo.ToggleWishlistItemAsync(craftWishlistItemDto));
    }

    [HttpGet("ids")]
    public async Task<ActionResult<List<long>>> GetCurrentWishlistIds()
    {
        
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to view your wishlist.");
        }

        return GetActionResult(await craftWishlistRepo.GetWishlistedCraftIdsForUserAsync(userId));
    }
}