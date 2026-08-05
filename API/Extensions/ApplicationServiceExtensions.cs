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

        var dbHost = config["POSTGRES_HOST"];
        var dbPort = config["POSTGRES_PORT"];
        var dbName = config["POSTGRES_DB"];
        var dbUsername = config["POSTGRES_USER"];
        var dbPassword = config["POSTGRES_PASSWORD"];
        var dbConnectionString = $"Host={dbHost};Port={dbPort};Database={dbName};Username={dbUsername};Password={dbPassword}";
        services.AddDbContext<DataContext>(options => options.UseNpgsql(dbConnectionString));
        
        services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));

        services.AddControllers();
        services.AddCors();        
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICraftyUserManager, CraftyUserManager>();
        services.AddScoped<ICraftManager, CraftManager>();
        services.AddScoped<ICloudMediaService, CloudMediaService>();
        services.AddScoped<ICraftWishlistManager, CraftWishlistManager>();
        services.AddScoped<IOrderManager, OrderManager>();
        services.AddScoped<IAccountManager, AccountManager>();
        services.AddScoped<IMessageManager, MessageManager>();

        services.AddSingleton(mapsterConfig);

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = $"{config["REDIS_HOST"]}:{config["REDIS_PORT"]},abortConnect=false,connectRetry=10,connectTimeout=5000";
            options.InstanceName = "crafty:";
        });

        services.AddScoped<ICacheService, CacheService>();
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
