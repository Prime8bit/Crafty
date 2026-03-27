using API.Data.Configuration;
using API.Entities;
using API.Misc;
using API.Services;
using CraftyCommon.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class AccountManager (UserManager<User> userManager, DataContext context, ITokenService tokenService) : IAccountManager
{
    public async Task<ManagerResponse<UserTokenDto>> RegisterAsync(RegisterDto regDto)
    {
        using (var transaction = await context.Database.BeginTransactionAsync())
        {
            try
            {
                if (await userManager.Users.AnyAsync(user => user.NormalizedUserName == regDto.UserName.ToUpper()))
                {
                    return new ManagerResponse<UserTokenDto>() 
                    {
                        ResponseType = ManagerResponseType.BadRequest,
                        ErrorMessages = [$"An account with username {regDto.UserName} already exists."]
                    };
                }

                if (await userManager.Users.AnyAsync(user => user.NormalizedEmail == regDto.Email.ToUpper()))
                {
                    
                    return new ManagerResponse<UserTokenDto>() 
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
                    return new ManagerResponse<UserTokenDto>() 
                    {
                        ResponseType = ManagerResponseType.BadRequest,
                        ErrorMessages = userResult.Errors.Select(error => error.Description).ToList()
                    };
                }

                var roleResult = await userManager.AddToRoleAsync(user, Role.User);
                if (!userResult.Succeeded)
                {   
                    return new ManagerResponse<UserTokenDto>() 
                    {
                        ResponseType = ManagerResponseType.BadRequest,
                        ErrorMessages = roleResult.Errors.Select(error => error.Description).ToList()
                    };
                }

                await transaction.CommitAsync();

                return new ManagerResponse<UserTokenDto>(new UserTokenDto
                {
                    UserId = user.Id,
                    UserDisplayName = user.DisplayName,
                    Token = await tokenService.CreateToken(user)
                });
            } catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return new ManagerResponse<UserTokenDto>()
                {
                    ResponseType = ManagerResponseType.BadRequest,
                    ErrorMessages = [ex.Message]
                };
            }
        }
        
    }

    public async Task<ManagerResponse<UserTokenDto>> LoginAsync(UserLoginRequestDto loginDto)
    {
        var user = await userManager.Users.SingleOrDefaultAsync(user => user.NormalizedUserName == loginDto.UserName.ToUpper());

        if (user == null)
        {            
            return new ManagerResponse<UserTokenDto>() 
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"Invalid UserName: {loginDto.UserName}."]
            };
        }

        var result = await userManager.CheckPasswordAsync(user, loginDto.Password);

        if (!result)
        {
            return new ManagerResponse<UserTokenDto>() 
            {
                ResponseType = ManagerResponseType.Unauthorized,
                ErrorMessages = ["Your username or password was incorrect."]
            };
        }

        return new ManagerResponse<UserTokenDto>(new UserTokenDto
        {
            UserId = user.Id,
            UserDisplayName = user.DisplayName,
            Token = await tokenService.CreateToken(user)
        });
    }
}