using API.Data;
using API.Data.Configuration;
using API.Entities;
using API.Extensions;
using API.MiddleWare;
using API.SignalR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);
builder.Services.AddRateLimitingServices();
builder.Services.Configure<KestrelServerOptions>(options =>
{
    // Set the max request body size to 100 MB
    options.Limits.MaxRequestBodySize = 100000000; // if don't set default value is: 30 MB
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<LoggingMiddleware>();

#if DEBUG
// The docker/k8s containers use a reverse proxy to keep traffic on the same host:port
// When in development, I need to allow CORS from the frontend because it runs on a different port
app.UseCors(policyBuilder => policyBuilder.AllowAnyHeader().AllowAnyMethod().AllowCredentials()
.WithOrigins("http://localhost:4200", "https://localhost:4200"));
#endif

// Authentication MUST come before Authorization and both need to happen
// before mapping controllers.
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapControllers();
app.MapHub<MessageHub>("hubs/message"); 

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;
try
{
    var context = services.GetRequiredService<DataContext>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<Role>>();
    await context.Database.MigrateAsync();
    await DataSeed.SeedUsers(userManager, roleManager);

    await context.MessageConnections.ExecuteDeleteAsync();
    await context.MessageGroups.ExecuteDeleteAsync();
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration");
}

app.Run();
