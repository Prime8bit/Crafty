using System;
using API.Data;
using API.Data.Configuration;
using API.Misc;
using API.Services;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services, 
        IConfiguration config)
    {
        var mapsterConfig = TypeAdapterConfig.GlobalSettings;
        mapsterConfig.Scan(typeof(Program).Assembly);

        services.AddControllers();
        services.AddDbContext<DataContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));
        services.AddCors();        
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICraftyUserManager, CraftyUserManager>();
        services.AddScoped<ICraftManager, CraftManager>();
        services.AddScoped<ICloudMediaService, CloudMediaService>();
        services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
        services.AddScoped<ICraftWishlistManager, CraftWishlistManager>();
        services.AddScoped<IOrderManager, OrderManager>();
        services.AddScoped<IAccountManager, AccountManager>();
        services.AddScoped<IMessageManager, MessageManager>();
        services.AddSingleton(mapsterConfig);
#if DEBUG
        services.AddSignalR(options => options.EnableDetailedErrors = true);
#else
        services.AddSignalR();
#endif

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        // FUTURE: Nate add OpenAPI documentation
        services.AddOpenApi();
        return services;
    }
}
