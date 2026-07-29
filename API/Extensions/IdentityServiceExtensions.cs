using System;
using System.Text;
using API.Data;
using API.Data.Configuration;
using API.Entities;
using API.Misc;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace API.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddIdentityCore<User>( options =>
        {
            // Change as desired for better security
            options.Password.RequiredUniqueChars = 0;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireDigit = false;
            options.Password.RequiredUniqueChars = 0;
            options.Password.RequiredLength = 4;
        }).AddRoles<Role>()
        .AddRoleManager<RoleManager<Role>>()
        .AddEntityFrameworkStores<DataContext>();
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var tokenKey = config["JWT_TOKEN_KEY"] ?? throw new Exception("JWT_TOKEN_KEY is missing from configuration.");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // Unless you want any key to be accepted, set this to true
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey)),            
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorizationBuilder()
            .AddPolicy(Policies.RequireAdminRole, PolicyServiceCollectionExtensions => PolicyServiceCollectionExtensions.RequireRole(Role.Admin));

        return services;
    }
}
