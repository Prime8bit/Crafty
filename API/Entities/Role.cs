using Microsoft.AspNetCore.Identity;

namespace API.Entities;

public class Role: IdentityRole<long>
{
    // These need to be string constants so they can be used in [Authorise(Roles = Role.Admin)] and similar attributes
    // Enum.ToString() cannot be evaluated as a pre-compile constant.
    public const string Admin = "Admin";
    public const string User = "User";
    
    public ICollection<User> Users { get; set; } = []; 
}