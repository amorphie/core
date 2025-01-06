using System.Net;
using System.Text.Json.Nodes;
using System.Text;
using System.Diagnostics;
namespace amorphie.core.Middleware.Logging;
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;
    private readonly LoggingOptions _loggingOptions;
    private const string _logTemplate = "RequestMethod : {RequestMethod}, RequestBody : {RequestBody}, RequestHost : {RequestHost}, RequestHeader : {RequestHeader}, ResponseBody : {ResponseBody}, ResponseStatus : {ResponseStatus}, ElapsedTime : {ElapsedTime}";
    public LoggingMiddleware(RequestDelegate next, ILoggerFactory loggerFactory, LoggingOptions loggingOptions)
    {
        _next = next;
        _logger = loggerFactory.CreateLogger<LoggingMiddleware>();
        _loggingOptions = loggingOptions;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Stream? originalResponseBody = null;
        JsonObject? requestHeaders = null;
        string? requestBody = null;
        string? responseBody = null;
        var stopWatch = Stopwatch.StartNew();
        try
        {
            if (!context.Request.Path.HasValue)
            {
                await _next(context);
            }
            //if path is ignored do not log
            else if (_loggingOptions.IgnorePaths != null && Array.Exists(_loggingOptions.IgnorePaths, context.Request.Path.Value.Contains))
            {
                await _next(context);
            }
            else
            {
                context.Request.Headers.TryGetValue(LoggingConstants.Headers.XWorkflowName, out var wfName);
                var ignoreByWfName = _loggingOptions.IgnoreContentByWorkflowName?.Contains(wfName.ToString()) == true;


                requestHeaders = LogRequestHeaders(context);

                requestBody = ignoreByWfName || context.Request.Method == HttpMethod.Get.Method ? "" : await LogRequestBodyAsync(context.Request);
                //LogResponse must be true and path must not be in IgnoreResponseByPaths
                if (_loggingOptions.LogResponse &&
                    !(
                    Array.Exists(_loggingOptions.IgnoreResponseByPaths, context.Request.Path.Value.Contains) || ignoreByWfName

                    )
                    )
                {
                    //Buffer response body
                    using var newResponseBody = new MemoryStream();
                    originalResponseBody = context.Response.Body;
                    context.Response.Body = newResponseBody;
                    responseBody = await InvokeInternalAsync(context, originalResponseBody, newResponseBody);
                }
                else
                {
                    await _next(context);
                }
                var headers = LogResponseHeaders(context, requestHeaders);
                stopWatch.Stop();
                _logger.LogInformation(_logTemplate,
                    context.Request.Method,
                    requestBody,
                    context.Request.Host,
                    headers,
                    responseBody,
                    context.Response.StatusCode,
                    stopWatch.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            if (originalResponseBody != null)
            {
                context.Response.Body = originalResponseBody;
            }
            stopWatch.Stop();

            await HandleExceptionAsync(context, ex, requestHeaders, requestBody, responseBody, stopWatch.ElapsedMilliseconds);
        }
    }


    private async Task<string?> InvokeInternalAsync(HttpContext context, Stream originalResponseBody, MemoryStream newResponseBody)
    {
        await _next(context);
        //Read response body
        newResponseBody.Seek(0, SeekOrigin.Begin);
        var responseBodyText = await new StreamReader(newResponseBody).ReadToEndAsync();

        //Rewind and set to originalResponseBody
        newResponseBody.Seek(0, SeekOrigin.Begin);
        await newResponseBody.CopyToAsync(originalResponseBody);

        return LogResponseBody(responseBodyText);
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex, JsonObject? requestHeaders, string? requestBody, string? responseBody, long elapsedTime)
    {
        var errorMessage = "An error occured and logged. Use trace identifier id to find out details";
        var errorDto = new ErrorModel
        {
            ErrorMessage = errorMessage,
            TraceIdentifier = context.TraceIdentifier,
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = ex is BadHttpRequestException badEx ? badEx.StatusCode : (int)HttpStatusCode.InternalServerError;
        _logger.LogError(ex, _logTemplate,
            context.Request.Method,
            requestBody,
            context.Request.Host,
            requestHeaders?.ToJsonString(),
            responseBody,
            context.Response.StatusCode,
            elapsedTime);
        await context.Response.WriteAsync(LoggingJsonSerializer.Serialize(errorDto));
    }

    private JsonObject LogRequestHeaders(HttpContext httpContext)
    {
        var requestHeaders = new JsonObject();
        foreach (var pair in httpContext.Request.Headers)
        {
            if (_loggingOptions.SanitizeHeaderNames?.Contains(pair.Key.ToLower()) == true)
            {
                requestHeaders.Add(pair.Key, "***");
            }
            else
            {
                requestHeaders.Add(pair.Key, pair.Value.ToString().Replace("\"", ""));
            }
        }
        return requestHeaders;
    }
    private async ValueTask<string> LogRequestBodyAsync(HttpRequest request, Encoding? encoding = null)
    {
        if (_loggingOptions.LogRequest)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body, encoding ?? Encoding.UTF8, leaveOpen: true);
            string body = await reader.ReadToEndAsync();
            body = body.Replace("\n", "").Replace("\r", "").Replace(" ", "");
            if (_loggingOptions.SanitizeFieldNames?.Length > 0)
            {
                body = LoggingHelper.FilterContent(body, _loggingOptions.SanitizeFieldNames);
            }
            request.Body.Position = 0;

            return body;
        }
        return "";
    }
    private string LogResponseBody(string responseBodyText)
    {
        if (_loggingOptions.SanitizeFieldNames?.Length > 0)
        {
            responseBodyText = LoggingHelper.FilterContent(responseBodyText, _loggingOptions.SanitizeFieldNames);
        }
        return responseBodyText;
    }
    private string LogResponseHeaders(HttpContext httpContext, JsonObject headers)
    {
        if (httpContext.Response.Headers.FirstOrDefault(p => p.Key == "FromCache").Key != null)
        {
            headers.TryAdd("ResponseHeader_FromCache", "true");
        }
        return headers.ToJsonString();
    }

    private class ErrorModel
    {
        public string? ErrorMessage { get; set; }
        public string? TraceIdentifier { get; set; }
    }
}