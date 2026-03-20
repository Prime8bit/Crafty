using System;

namespace API.DTOs;

public class UserLoginDto
{
    public required string UserName { get; set; }

    public string Password { get; set; } = "";
    public string Token { get; set; } = "";
}
