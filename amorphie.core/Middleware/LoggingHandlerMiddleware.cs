using System;
using Microsoft.AspNetCore.Http;
using System.Net;
using System.Net.Http;

namespace amorphie.core.Middleware;


public class LoggingHandlerMiddleware
{

    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public LoggingHandlerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next = next;

        _logger = loggerFactory.CreateLogger<LoggingHandlerMiddleware>();
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            _logger.LogInformation($"Information");

            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }


    }

    private Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
    {

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        var now = DateTime.UtcNow;
        _logger.LogError($"{now.ToString("HH:mm:ss")} : {ex}");
        return httpContext.Response.WriteAsync(ex.Message);
    }
}