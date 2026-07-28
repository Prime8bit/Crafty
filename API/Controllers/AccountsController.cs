using API.Data;
using API.Extensions;
using CraftyCommon.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

public class AccountsController(IAccountManager accountRepo) : BaseApiController
{

    [HttpPost("register")]
    [EnableRateLimiting(RateLimiters.Register)]
    public async Task<ActionResult<UserTokenDto>> Register(RegisterDto regDto)
    {
        var regDtoErrors = new List<string>();
        if (string.IsNullOrEmpty(regDto.UserName)) regDtoErrors.Add("Username is missing.");
        if (string.IsNullOrEmpty(regDto.Password)) regDtoErrors.Add("Password is missing.");
        if (string.IsNullOrEmpty(regDto.Email)) regDtoErrors.Add("Email is missing.");        
        if (regDtoErrors.Count > 0) return BadRequest(regDtoErrors);

        return GetActionResult(await accountRepo.RegisterAsync(regDto));
    }

    [HttpPost("login")]
    [EnableRateLimiting(RateLimiters.Login)]
    public async Task<ActionResult<UserTokenDto>> Login(UserLoginRequestDto loginDto)
    {        
        var regDtoErrors = new List<string>();
        if (string.IsNullOrEmpty(loginDto.UserName)) regDtoErrors.Add("Username is missing.");
        if (string.IsNullOrEmpty(loginDto.Password)) regDtoErrors.Add("Password is missing."); 
        if (regDtoErrors.Count > 0) return BadRequest(regDtoErrors);

        return GetActionResult(await accountRepo.LoginAsync(loginDto));
    }
}
