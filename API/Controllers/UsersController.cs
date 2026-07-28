using System.Security.Claims;
using API.Data;
using API.Extensions;
using CraftyCommon.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[Authorize]
public class UsersController(ICraftyUserManager userManager) : BaseApiController
{
    [AllowAnonymous]
    [HttpGet]
    [EnableRateLimiting(RateLimiters.UserRead)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        return Ok(await userManager.GetUsersAsync());
    }

    [AllowAnonymous]
    [HttpGet("{userId}")]
    [EnableRateLimiting(RateLimiters.UserRead)]
    public async Task<ActionResult<UserDto>> GetUser(long userId)
    {
        var userDto = await userManager.GetUserAsync(userId);

        if (userDto == null)
        {
            return NotFound($"User with id {userId} not found.");
        }

        return Ok(userDto);
    }

    [HttpPut]
    [EnableRateLimiting(RateLimiters.UserWrite)]
    public async Task<ActionResult<UserDto>> UpdateUser(UserDto userDto)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to edit your user information.");
        }

        if (string.IsNullOrEmpty(userDto.DisplayName))
        {
            return BadRequest("Display name cannot be empty.");
        }
        
        if (string.IsNullOrEmpty(userDto?.UserName?.ToUpper()))
        {
            return BadRequest("UserName was not provided.");
        }

        if (string.IsNullOrEmpty(userDto?.Email))
        {
            return BadRequest("Email was not provided.");
        }

        if (userDto.Id != userId)
        {
            return Forbid();
        }

        return GetActionResult(await userManager.UpdateUserAsync(userDto));
    }
    
    [HttpPost("set-profile-image")]
    [EnableRateLimiting(RateLimiters.UserWrite)]
    public async Task<ActionResult<UserMediaDto>> SetProfileImage([FromForm] IFormFile file)
    {
        
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long userId;
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out userId))
        {
            return NotFound("You must be logged in to edit your user profile image.");
        }

        return GetActionResult(await userManager.SetUserProfileImageAsync(userId, file));
    }
}
