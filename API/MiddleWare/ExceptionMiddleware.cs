using System;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace API.MiddleWare;

public class ApiException(int statusCode, string Message, string? details = "")
{
    public int StatusCode { get; set; } = statusCode;
    public string Message { get; set; } = Message;
    public string? Details { get; set; } = details;
}

public class ExceptionMiddleware(
    RequestDelegate next, 
    ILogger<ExceptionMiddleware> logger, 
    IHostEnvironment env)
{
    
    public async Task InvokeAsync(HttpContext context)
    {        
        try
        {
            // I am going to restart the entire docker container every day, otherwise I would set up
            // rolling log files.
            var stopwatch = Stopwatch.StartNew(); 
            logger.LogInformation($"Request received: {context.Request.Method} {context.Request.Path}");
            await next(context);
            logger.LogInformation($"Request completed: {context.Request.Method} {context.Request.Path} {context.Response.StatusCode} {stopwatch.ElapsedMilliseconds}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

            var response = env.IsDevelopment()
                ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                : new ApiException(context.Response.StatusCode, "Internal Server Error");

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            var json = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(json);
        }
    }
}
