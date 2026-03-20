using API.Data;
using API.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class AccountsController(IAccountManager accountRepo) : BaseApiController
{

    [HttpPost("register")]
    public async Task<ActionResult<UserLoginDto>> Register(RegisterDto regDto)
    {
        var regDtoErrors = new List<string>();
        if (string.IsNullOrEmpty(regDto.UserName)) regDtoErrors.Add("Username is missing.");
        if (string.IsNullOrEmpty(regDto.Password)) regDtoErrors.Add("Password is missing.");
        if (string.IsNullOrEmpty(regDto.Email)) regDtoErrors.Add("Email is missing.");        
        if (regDtoErrors.Count > 0) return BadRequest(regDtoErrors);

        return GetActionResult(await accountRepo.RegisterAsync(regDto));
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserLoginDto>> Login(UserLoginDto loginDto)
    {        
        var regDtoErrors = new List<string>();
        if (string.IsNullOrEmpty(loginDto.UserName)) regDtoErrors.Add("Username is missing.");
        if (string.IsNullOrEmpty(loginDto.Password)) regDtoErrors.Add("Password is missing."); 
        if (regDtoErrors.Count > 0) return BadRequest(regDtoErrors);

        return GetActionResult(await accountRepo.LoginAsync(loginDto));
    }
}
