using API.DTOs;
using API.Entities;
using API.Misc;
using API.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class AccountManager (UserManager<User> userManager, ITokenService tokenService) : IAccountManager
{
    public async Task<ManagerResponse<UserLoginDto>> RegisterAsync(RegisterDto regDto)
    {
        if (await userManager.Users.AnyAsync(user => user.NormalizedUserName == regDto.UserName.ToUpper()))
        {
            return new ManagerResponse<UserLoginDto>() 
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = [$"An account with username {regDto.UserName} already exists."]
            };
        }

        if (await userManager.Users.AnyAsync(user => user.NormalizedEmail == regDto.Email.ToUpper()))
        {
            
            return new ManagerResponse<UserLoginDto>() 
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = [$"An account with email {regDto.Email} already exists."]
            };
        }

        var user = new User
        {
            UserName = regDto.UserName,
            Email = regDto.Email,
            FullName = $"{regDto.FirstName} {regDto.LastName}",
            DisplayName = $"{regDto.FirstName} {regDto.LastName}"
        };

        user.ProfileImage = new UserMedia
        {
            UserId = user.Id,
            User = user,
            Url = Constants.DEFAULT_PROFILE_PIC_URL,
            CloudId = Constants.DEFAULT_PROFILE_PIC_CLOUD_ID,
            Type = MediaType.Image        
        };

        var userResult = await userManager.CreateAsync(user, regDto.Password);
        if (!userResult.Succeeded)
        {   
            return new ManagerResponse<UserLoginDto>() 
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = userResult.Errors.Select(error => error.Description).ToList()
            };
        }

        var roleResult = await userManager.AddToRoleAsync(user, Role.User);
        if (!userResult.Succeeded)
        {   
            return new ManagerResponse<UserLoginDto>() 
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = roleResult.Errors.Select(error => error.Description).ToList()
            };
        }

        return new ManagerResponse<UserLoginDto>(new UserLoginDto
        {
            UserName = user.UserName,
            Token = await tokenService.CreateToken(user)
        });
    }

    public async Task<ManagerResponse<UserLoginDto>> LoginAsync(UserLoginDto loginDto)
    {
        var user = await userManager.Users.SingleOrDefaultAsync(user => user.NormalizedUserName == loginDto.UserName.ToUpper());

        if (user == null)
        {            
            return new ManagerResponse<UserLoginDto>() 
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"Invalid UserName: {loginDto.UserName}."]
            };
        }

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result)
        {
            return new ManagerResponse<UserLoginDto>() 
            {
                ResponseType = ManagerResponseType.Unauthorized,
                ErrorMessages = ["Your username or password was incorrect."]
            };
        }

        return new ManagerResponse<UserLoginDto>(new UserLoginDto
        {
            UserName = user.UserName ?? "",
            Token = await tokenService.CreateToken(user)
        });
    }
}