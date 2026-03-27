using API.Entities;
using API.Misc;
using API.Services;
using CraftyCommon.DTOs;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class CraftyUserManager (UserManager<User> userManager, ICloudMediaService cloudMediaService) : ICraftyUserManager
{
    public async Task<UserDto?> GetUserAsync(long id)
    {
        return await userManager.Users
            .Include(user => user.Products)
            .Select(user => new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                DisplayName = user.DisplayName,
                Created = user.Created,
                LastActive = user.LastActive,
                Address = user.Address,
                ProfileImage = user.ProfileImage == null ? null : user.ProfileImage.Adapt<UserMediaDto>(),
                Products = user.Products.Select(product => new CraftDto
                {
                    Id = product.Id,
                    Name = product.Name,
                    Price = product.Price,
                    Stock = product.Stock,
                    CreatedAt = product.CreatedAt.ToString("o"),
                    IsArchived = product.IsArchived,
                    SellerId = product.SellerId,
                    SearchImageId = product.SearchImageId,
                    SearchImage = product.SearchImage == null ? null : product.SearchImage.Adapt<CraftMediaDto>(),
                    Medias = new List<CraftMediaDto>()
                }).ToList(),
            })
            .SingleOrDefaultAsync( user => user.Id == id);
    }

    public async Task<IEnumerable<UserDto>> GetUsersAsync()
    {
        // I optimized the query for only the information needed by UserDto
        // Getting the list of crafts is not important for this query.
        return await userManager.Users.Select(user => new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FullName = user.FullName,
                DisplayName = user.DisplayName,
                Created = user.Created,
                LastActive = user.LastActive,
                Address = user.Address,
                ProfileImage = user.ProfileImage == null ? null : user.ProfileImage.Adapt<UserMediaDto>()
            }).ToListAsync();
    }

    public async Task<ManagerResponse> AddCraftToSellerAsync(long sellerId, Craft newCraft)
    {
        var seller = await userManager.Users
            .Include(user => user.Products)
            .SingleOrDefaultAsync(user => user.Id == sellerId);
            
        if (seller == null)
        {
            return new ManagerResponse()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"Seller with id '{sellerId}' not found"]
            };
        }
        seller.Products.Add(newCraft);

        return new ManagerResponse() { ResponseType = ManagerResponseType.Ok };
    }

    public async Task<ManagerResponse<UserDto>> UpdateUserAsync(UserDto userDto)
    {
        var user = await userManager.Users
            .Include(user => user.Products)
            .Include(user => user.ProfileImage)
            .SingleOrDefaultAsync( user => user.Id == userDto.Id);
        
        if (user == null)
        {
            return new ManagerResponse<UserDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"Seller with id '{userDto.Id}' not found"]
            };
        }

        if (!string.IsNullOrEmpty(userDto.DisplayName))
        {
            user.DisplayName = userDto.DisplayName;
        }

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            return new ManagerResponse<UserDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = [$"Unable to save changes to user with id {userDto.Id}"]
            };
        }

        return new ManagerResponse<UserDto>(userDto);
    }

    public async Task<ManagerResponse<UserMediaDto>> SetUserProfileImageAsync(long userId, IFormFile file)
    {
        var user = await userManager.Users
            .Include(user => user.ProfileImage)
            .SingleOrDefaultAsync(user => user.Id == userId);

        if (user == null )
        {
            return new ManagerResponse<UserMediaDto>()
            {
                ResponseType = ManagerResponseType.NotFound,
                ErrorMessages = [$"User with id '{userId}' does not exist."]
            };
        }       
                        
        var cloudMediaResult = await cloudMediaService.AddImageAsync(file);
        if (cloudMediaResult.Error != null)
        {
            return new ManagerResponse<UserMediaDto>()
            {
                ResponseType = ManagerResponseType.BadRequest,
                ErrorMessages = [cloudMediaResult.Error.Message]
            };
        }

        if (user.ProfileImage?.CloudId != null 
            && user.ProfileImage.CloudId.Length > 0 
            && user.ProfileImage.CloudId != Constants.DEFAULT_PROFILE_PIC_CLOUD_ID)
        {
            var deleteResult = await cloudMediaService.DeleteImageAsync(user.ProfileImage.CloudId);
            if (deleteResult.Error != null)
            {
                return new ManagerResponse<UserMediaDto>()
                {
                    ResponseType = ManagerResponseType.BadRequest,
                    ErrorMessages = [$"Failed to delete previous profile image: {deleteResult.Error.Message}"]
                };
            }
        }

        var profileImage = new UserMedia
        {
            UserId = user.Id,
            User = user,
            Url = cloudMediaResult.SecureUrl.AbsoluteUri,
            CloudId = cloudMediaResult.PublicId,
            Type = MediaType.Image        
        };

        user.ProfileImage = profileImage;
        var userResult = await userManager.UpdateAsync(user);

        if (!userResult.Succeeded)
        {
                return new ManagerResponse<UserMediaDto>()
                {
                    ResponseType = ManagerResponseType.BadRequest,
                    ErrorMessages = [$"Failed to set profile image for user with id {userId}"]
                };
        }      

        return new ManagerResponse<UserMediaDto>(profileImage.Adapt<UserMediaDto>());
    }

    public async Task<bool> IsUserInRoleAsync(long userId, string roleName)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        
        if (user == null)
        {
            return false;
        }

        return await userManager.IsInRoleAsync(user, roleName);
    }
}
