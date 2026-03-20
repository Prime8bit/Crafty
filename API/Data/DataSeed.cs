using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

/// <summary>
/// This class is used to seed the database with test data, which it will only do if the database is empty.
/// </summary>
public class DataSeed
{
    public static async Task SeedUsers(UserManager<User> userManager, RoleManager<Role> roleManager)
    {
        await CreateRoles(roleManager);

        if (await userManager.Users.AnyAsync()) return;

        var userData = await File.ReadAllTextAsync("Migrations/TestData.json");
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var users = JsonSerializer.Deserialize<List<User>>(userData, options);
        
        if (users == null)
            return;        

        foreach (var user in users)
        {
            await userManager.CreateAsync(user, "password");
            await userManager.AddToRoleAsync(user, Role.User);
        }

        var admin = new User()
        {
            UserName = "admin",
            FullName = "Admin",
        };

        await userManager.CreateAsync(admin, "password");
        await userManager.AddToRoleAsync(admin, Role.Admin);
    }

    private static async Task CreateRoles(RoleManager<Role> roleManager)
    {
        var roles = new List<Role>
        {
            new() { Name = Role.User },
            new() { Name = Role.Admin }
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role.Name!))
            {
                await roleManager.CreateAsync(role); 
            }
        }
    }
}
