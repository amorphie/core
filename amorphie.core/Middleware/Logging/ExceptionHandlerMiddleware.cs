using System.Net;

namespace amorphie.core.Middleware.Logging;


public class ExceptionHandlerMiddleware
{

    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<ExceptionHandlerMiddleware>();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext httpContext, Exception ex)
    {
        var errorMessage = $"An error occured and logged with \"{httpContext.TraceIdentifier}\" trace identifier id";
        httpContext.Response.ContentType = "application/json";
        if (ex is BadHttpRequestException badEx)
        {
            httpContext.Response.StatusCode = badEx.StatusCode;
        }
        else
        {
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        }
        _logger.LogError($"TraceIdentifier : {httpContext.TraceIdentifier}. Exception: {ex}");
        await httpContext.Response.WriteAsync(errorMessage);

    }
}