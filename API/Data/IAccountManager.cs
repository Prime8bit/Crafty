using API.Misc;
using CraftyCommon.DTOs;

namespace API.Data;

public interface IAccountManager
{
    Task<ManagerResponse<UserTokenDto>> RegisterAsync(RegisterDto regDto);
    Task<ManagerResponse<UserTokenDto>> LoginAsync(UserLoginRequestDto loginDto);
}