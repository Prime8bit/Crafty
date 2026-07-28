using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace API.Extensions;

public class RateLimiters
{
    public const string Register = "newAccountPolicy";
    public const string Login = "loginPolicy";
    public const string UserWrite = "userWritePolicy";
    public const string UserRead = "userReadPolicy";
}

public static class RateLimitingServiceExtensions
{
    
    public static IServiceCollection AddRateLimitingServices(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // This is how you would implement a global limiter. I use nginx for ip based limiting
            // so I don't think I will need one, but I want this for reference.
            // Use on an endpoint with [EnableRateLimiting("limiterName")]
            /**
            options.AddFixedWindowLimiter("limiterName", cfg =>
            {
                cfg.PermitLimit = 3;
                cfg.Window = TimeSpan.FromMinutes(1);
            });
            **/

            // New account policy. This is the strictest as it is a commonly attacked endpoint.
            // Since the user id isn't known, the ip address is used instead.
            options.AddPolicy(RateLimiters.Register, httpContext =>
            {                
                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 8,
                        Window = TimeSpan.FromHours(1),
                        SegmentsPerWindow = 4,
                        QueueLimit = 0
                    }
                );
            });

            // Login policy. I still want this to be strict as it is the most commonly attacked endpoint
            // but logins happen more than registers, so it should be a bit less strict than that
            options.AddPolicy(RateLimiters.Login, httpContext =>
            {                
                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromHours(5),
                        SegmentsPerWindow = 5,
                        QueueLimit = 0
                    }
                );
            });

            // Write policy. Write should be stricter than reads.
            options.AddPolicy(RateLimiters.UserWrite, httpContext =>
            {        
                string userId = httpContext.User.FindFirstValue("userId") ?? "_"; 

                // I use "_" to represent anonymous users as it is always an invalid username.
                // Posting/deleting anonymously should not be possible in my app so I just bundle them
                // together for rate limiting for safety. I don't like empty string as it is less deliberate.
                return RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: userId == "" ? "_" : userId,
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 20,
                        TokensPerPeriod = 20,
                        ReplenishmentPeriod = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }
                );
            });

            // Read policy. Most relaxed. I just want to prevent ddos attacks
            options.AddPolicy(RateLimiters.UserRead, httpContext =>
            {        
                string userId = httpContext.User.FindFirstValue("userId") ?? "_"; 

                // I use "_" to represent anonymous users as it is always an invalid username.
                // Posting/deleting anonymously should not be possible in my app so I just bundle them
                // together for rate limiting for safety. I don't like empty string as it is less deliberate.
                return RateLimitPartition.GetTokenBucketLimiter(
                    partitionKey: userId == "" ? "_" : userId,
                    factory: _ => new TokenBucketRateLimiterOptions
                    {
                        TokenLimit = 100,
                        TokensPerPeriod = 5,
                        ReplenishmentPeriod = TimeSpan.FromSeconds(3),
                        QueueLimit = 5,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        AutoReplenishment = true
                    }
                );
            });
        });

        return services;
    }
}
