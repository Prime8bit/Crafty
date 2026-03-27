using API.Entities;
using API.Misc;
using CraftyCommon.DTOs;

namespace API.Data;

public interface ICraftyUserManager
{
    Task<IEnumerable<UserDto>> GetUsersAsync();
    Task<UserDto?> GetUserAsync(long id);
    Task<ManagerResponse> AddCraftToSellerAsync(long sellerId, Craft newCraft);
    Task<ManagerResponse<UserDto>> UpdateUserAsync(UserDto userDto);
    Task<ManagerResponse<UserMediaDto>> SetUserProfileImageAsync(long userId, IFormFile file);
    Task<bool> IsUserInRoleAsync(long userId, string roleName);
}
