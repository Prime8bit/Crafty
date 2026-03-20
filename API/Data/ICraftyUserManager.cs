using System;
using API.DTOs;
using API.Entities;
using API.Misc;
using CloudinaryDotNet.Actions;

namespace API.Data;

public interface ICraftyUserManager
{
    Task<IEnumerable<UserDto>> GetUsersAsync();
    Task<UserDto?> GetUserAsync(long id);
    Task<UserDto?> GetUserAsync(string userName);
    Task<ManagerResponse> AddCraftToSellerAsync(string sellerUserName, Craft newCraft);
    Task<ManagerResponse<UserDto>> UpdateUserAsync(UserDto userDto);
    Task<ManagerResponse<UserMediaDto>> SetUserProfileImageAsync(long userId, IFormFile file);
    Task<ManagerResponse<OrderDto>> AddOrderToUsersAsync(Order order);
    Task<bool> IsUserInRoleAsync(long userId, string roleName);
}
