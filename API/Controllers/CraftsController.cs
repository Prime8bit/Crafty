using System.Security.Claims;
using API.Data;
using API.Entities;
using API.Extensions;
using API.Misc;
using CraftyCommon.Pagination;
using CraftyCommon.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

public class CraftsController(
    ICraftManager craftManager
    ) : BaseApiController
{
    [HttpGet]
    [EnableRateLimiting(RateLimiters.UserRead)]
    public async Task<ActionResult<IEnumerable<CraftDto>>> GetCrafts([FromQuery] CraftListParams paginationParams)
    {
        var crafts = await craftManager.GetCraftsAsync(paginationParams);

        return Ok(crafts);
    }

    [HttpGet("{craftId}")]
    [EnableRateLimiting(RateLimiters.UserRead)]
    public async Task<ActionResult<CraftDto>> GetCraft(long craftId)
    {
        var craft = await craftManager.GetCraftAsync(craftId);
        if (craft == null)
        {
            return NotFound($"Craft with id {craftId} not found.");
        }

        return Ok(craft);
    }

    [Authorize]
    [HttpPost]
    [EnableRateLimiting(RateLimiters.UserWrite)]
    public async Task<ActionResult<CraftDto>> NewCraft(CraftDto newCraft)
    {
        if (string.IsNullOrEmpty(newCraft.Name))
        {
            return NotFound("You cannot create a craft without a name.");
        }

        var response = await craftManager.CreateCraftAsync(newCraft);
        if (response.ResponseType != ManagerResponseType.Ok || response.Data?.Id == null)
        {
            GetActionResult(response);
        }

        /** In ASP.NET Core, using the suffix "Async" in function names can break some of the reflection
        * resolution in some ASP.NET functionality. If you use "Async" at the end of the functions they may work
        * for a long time until you use specific ASP.NET functionality, like CreatedAtAction
        **/
        return CreatedAtAction(nameof(GetCraft), new { craftId = response.Data!.Id }, response.Data);
    }

    [Authorize]
    [HttpPut("{craftId}")]
    [EnableRateLimiting(RateLimiters.UserWrite)]
    public async Task<ActionResult<CraftDto>> UpdateCraft(long craftId, CraftDto updatedCraft)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to edit a craft.");
        }

        if (string.IsNullOrEmpty(updatedCraft.Name))
        {
            return NotFound("You cannot update the craft without a name.");
        }
        
        if (updatedCraft.Id != craftId)
        {
            return BadRequest("You cannot change the id of a craft");
        }    

        return GetActionResult(await craftManager.UpdateCraftAsync(userId, updatedCraft));
    }

    [Authorize]
    [HttpPut("{craftId}/archive")]
    [EnableRateLimiting(RateLimiters.UserWrite)]
    public async Task<ActionResult<CraftDto>> ArchiveCraft(long craftId)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to archive a craft.");
        }    

        return GetActionResult(await craftManager.ArchiveCraftAsync(userId, craftId));
    }

    [Authorize]
    [HttpPut("{craftId}/inappropriate")]
    [EnableRateLimiting(RateLimiters.UserWrite)]
    public async Task<ActionResult> MarkCraftAsInappropriate(long craftId)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in mark a craft as inappropriate.");
        }    

        return GetActionResult(await craftManager.MarkCraftAsInappropriateAsync(userId, craftId));
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpPut("{craftId}/appropriate")]
    [EnableRateLimiting(RateLimiters.UserWrite)]
    public async Task<ActionResult> MarkCraftAsAppropriate(long craftId)
    {
        return GetActionResult(await craftManager.MarkCraftAsAppropriateAsync(craftId));
    }

    [Authorize(Policy = Policies.RequireAdminRole)]
    [HttpGet("inappropriate")]
    [EnableRateLimiting(RateLimiters.UserRead)]
    public async Task<ActionResult<PagedList<Craft>>> GetInappropriateCrafts([FromQuery] PaginationParams paginationParams)
    { 
        var craftPagedList = await craftManager.GetInappropriateCraftsAsync(paginationParams);

        return Ok(craftPagedList);
    }
}
