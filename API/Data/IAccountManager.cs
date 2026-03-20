using API.DTOs;
using API.Misc;

namespace API.Data;

public interface IAccountManager
{
    Task<ManagerResponse<UserLoginDto>> RegisterAsync(RegisterDto regDto);
    Task<ManagerResponse<UserLoginDto>> LoginAsync(UserLoginDto loginDto);
}