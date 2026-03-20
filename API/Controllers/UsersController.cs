using System.Security.Claims;
using API.Data;
using API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Authorize]
public class UsersController(ICraftyUserManager userManager) : BaseApiController
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
    {
        return Ok(await userManager.GetUsersAsync());
    }

    [AllowAnonymous]
    [HttpGet("{userName}")]
    public async Task<ActionResult<UserDto>> GetUser(string userName)
    {
        var userDto = await userManager.GetUserAsync(userName);

        if (userDto == null)
        {
            return NotFound($"User with UserName {userName} not found.");
        }

        return Ok(userDto);
    }

    [HttpPut]
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
